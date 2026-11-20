<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import {
    Plus,
    Search,
    RefreshCw,
    ClipboardList,
    X,
    Eye,
    Pencil
} from 'lucide-vue-next'

import { workOrderService } from '../services/workOrderService'
import type {
    WorkOrder,
    WorkOrderStatus,
    Priority
} from '../services/workOrderService'

import { branchService } from '../services/branchService'
import type { Branch } from '../services/branchService'

const statusOptions: { value: WorkOrderStatus; label: string }[] = [
    { value: 'Open', label: 'Open' },
    { value: 'InProgress', label: 'In Progress' },
    { value: 'Done', label: 'Done' }
]

const workOrders = ref<WorkOrder[]>([])
const branches = ref<Branch[]>([])

const loading = ref(false)
const branchesLoading = ref(false)
const submitting = ref(false)
const updatingStatusId = ref<number | null>(null)

const error = ref('')
const formError = ref('')
const statusError = ref('')

const page = ref(1)
const pageSize = 10
const total = ref(0)

const search = ref('')

// Modal state
const showModal = ref(false)
const editingWorkOrderId = ref<number | null>(null)
const selectedWorkOrder = ref<WorkOrder | null>(null)
const detailsLoading = ref(false)
const detailsError = ref('')

// Form state
const form = ref({
    title: '',
    description: '',
    priority: 'Medium' as Priority,
    branchId: ''
})

const isEditing = computed(() => editingWorkOrderId.value !== null)

const totalPages = computed(() =>
    Math.max(1, Math.ceil(total.value / pageSize))
)

const filteredWorkOrders = computed(() => {
    const keyword = search.value.trim().toLowerCase()

    if (!keyword) return workOrders.value

    return workOrders.value.filter(order =>
        order.title.toLowerCase().includes(keyword) ||
        order.branchName.toLowerCase().includes(keyword)
    )
})

// Fetch work orders
async function fetchWorkOrders() {
    loading.value = true
    error.value = ''
    statusError.value = ''

    try {
        const response = await workOrderService.getAll(
            page.value,
            pageSize
        )

        workOrders.value = response.data.items
        total.value = response.data.total
    } catch (err: any) {
        console.error('API ERROR:', err)
        console.error('STATUS:', err.response?.status)
        console.error('RESPONSE:', err.response?.data)

        error.value = 'Unable to load work orders.'
    } finally {
        loading.value = false
    }
}

async function viewWorkOrder(id: number) {
    selectedWorkOrder.value = null
    detailsError.value = ''
    detailsLoading.value = true

    try {
        const response = await workOrderService.getById(id)
        selectedWorkOrder.value = response.data
    } catch (err) {
        console.error('Failed to load work order:', err)
        detailsError.value = 'Unable to load work order details.'
    } finally {
        detailsLoading.value = false
    }
}

async function updateWorkOrderStatus(id: number, newStatus: WorkOrderStatus) {
    if (updatingStatusId.value === id) return

    statusError.value = ''
    detailsError.value = ''
    updatingStatusId.value = id

    try {
        await workOrderService.updateStatus(id, newStatus)

        const existing = workOrders.value.find(o => o.id === id)
        if (existing) {
            existing.status = newStatus
        }

        if (selectedWorkOrder.value?.id === id) {
            selectedWorkOrder.value.status = newStatus
        }
    } catch (err: any) {
        console.error('Failed to update status:', err)
        const message = 'Failed to update work order status. Please try again.'
        if (selectedWorkOrder.value?.id === id) {
            detailsError.value = message
        } else {
            statusError.value = message
        }
    } finally {
        updatingStatusId.value = null
    }
}

function onStatusSelectChange(id: number, event: Event) {
    const select = event.target as HTMLSelectElement
    const newStatus = select.value as WorkOrderStatus
    const current = workOrders.value.find(o => o.id === id)?.status
        ?? selectedWorkOrder.value?.status

    updateWorkOrderStatus(id, newStatus).finally(() => {
        if (current && select.value !== current) {
            const latest = workOrders.value.find(o => o.id === id)?.status
                ?? selectedWorkOrder.value?.status
            if (latest) select.value = latest
        }
    })
}

// Fetch branches
async function fetchBranches() {
    branchesLoading.value = true

    try {
        const response = await branchService.getAll()
        branches.value = response.data
    } catch (err) {
        console.error(err)
        formError.value = 'Unable to load branches. Please try again.'
    } finally {
        branchesLoading.value = false
    }
}

// Open create modal
async function openCreateModal() {
    editingWorkOrderId.value = null
    resetForm()
    formError.value = ''
    showModal.value = true

    if (branches.value.length === 0) {
        await fetchBranches()
    }
}

// Open edit modal
async function openEditModal(order: WorkOrder) {
    editingWorkOrderId.value = order.id
    form.value = {
        title: order.title,
        description: order.description ?? '',
        priority: order.priority,
        branchId: String(order.branchId)
    }
    formError.value = ''
    showModal.value = true

    if (branches.value.length === 0) {
        await fetchBranches()
    }
}

// Close modal
function closeFormModal() {
    if (submitting.value) return

    showModal.value = false
    editingWorkOrderId.value = null
    formError.value = ''

    resetForm()
}

// Reset form
function resetForm() {
    form.value = {
        title: '',
        description: '',
        priority: 'Medium',
        branchId: ''
    }
}

// Create or update work order
async function saveWorkOrder() {
    formError.value = ''

    const title = form.value.title.trim()
    const description = form.value.description.trim()

    if (!title) {
        formError.value = 'Work order title is required.'
        return
    }

    if (title.length > 150) {
        formError.value = 'Title cannot exceed 150 characters.'
        return
    }

    if (description.length > 500) {
        formError.value = 'Description cannot exceed 500 characters.'
        return
    }

    if (!form.value.branchId) {
        formError.value = 'Please select a branch.'
        return
    }

    const payload = {
        title,
        description: description || null,
        priority: form.value.priority,
        branchId: Number(form.value.branchId)
    }

    submitting.value = true

    try {
        if (editingWorkOrderId.value !== null) {
            const response = await workOrderService.update(
                editingWorkOrderId.value,
                payload
            )
            const updated = response.data

            const index = workOrders.value.findIndex(o => o.id === updated.id)
            if (index !== -1) {
                workOrders.value[index] = updated
            }

            if (selectedWorkOrder.value?.id === updated.id) {
                selectedWorkOrder.value = updated
            }

            showModal.value = false
            editingWorkOrderId.value = null
            resetForm()
        } else {
            await workOrderService.create(payload)

            showModal.value = false
            resetForm()

            // Show the newest record on the first page.
            page.value = 1
            await fetchWorkOrders()
        }
    } catch (err: any) {
        console.error(err)

        formError.value =
            err.response?.data?.error ||
            `Failed to ${isEditing.value ? 'update' : 'create'} work order. Please try again.`
    } finally {
        submitting.value = false
    }
}

// Pagination
function changePage(newPage: number) {
    if (newPage < 1 || newPage > totalPages.value) return

    page.value = newPage
    fetchWorkOrders()
}

// Formatting
function formatDate(date: string) {
    return new Date(date).toLocaleDateString('en-PH', {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
    })
}

function statusClass(status: string) {
    const classes: Record<string, string> = {
        Open: 'open',
        InProgress: 'in-progress',
        Done: 'done'
    }

    return classes[status] ?? ''
}

function priorityClass(priority: string) {
    return priority.toLowerCase()
}

onMounted(() => {
    fetchWorkOrders()
})
</script>

<template>
    <div class="work-orders-page">

        <!-- Page Header -->
        <div class="page-heading">
            <div>
                <h2>Work Orders</h2>
                <p>Manage and monitor all work orders.</p>
            </div>

            <button
                class="primary-button"
                @click="openCreateModal"
            >
                <Plus :size="17" />
                New Work Order
            </button>
        </div>

        <!-- Work Orders Table -->
        <div class="content-card">

            <div class="table-toolbar">
                <div class="search-box">
                    <Search :size="17" />

                    <input
                        v-model="search"
                        type="text"
                        placeholder="Search title or branch..."
                    />
                </div>

                <button
                    class="secondary-button"
                    @click="fetchWorkOrders"
                    :disabled="loading"
                >
                    <RefreshCw :size="15" />
                    Refresh
                </button>
            </div>

            <div v-if="statusError" style="padding: 12px 20px 0;">
                <div class="form-error">{{ statusError }}</div>
            </div>

            <div v-if="loading" class="table-message">
                Loading work orders...
            </div>

            <div v-else-if="error" class="table-message error-message">
                {{ error }}

                <button class="text-link" @click="fetchWorkOrders">
                    Try again
                </button>
            </div>

            <div v-else-if="filteredWorkOrders.length === 0" class="table-message">
                <ClipboardList :size="32" />
                <strong>No work orders found</strong>
                <span>No records match your search.</span>
            </div>

            <div v-else class="table-wrapper">
                <table class="data-table">
                    <thead>
                        <tr>
                            <th>WORK ORDER</th>
                            <th>BRANCH</th>
                            <th>PRIORITY</th>
                            <th>STATUS</th>
                            <th>DATE CREATED</th>
                            <th>ACTIONS</th>
                        </tr>
                    </thead>

                    <tbody>
                        <tr
                            v-for="order in filteredWorkOrders"
                            :key="order.id"
                        >
                            <td>
                                <div class="order-title">
                                    <strong>{{ order.title }}</strong>
                                    <span>
                                        WO-{{ String(order.id).padStart(4, '0') }}
                                    </span>
                                </div>
                            </td>

                            <td>{{ order.branchName }}</td>

                            <td>
                                <span
                                    class="priority-badge"
                                    :class="priorityClass(order.priority)"
                                >
                                    {{ order.priority }}
                                </span>
                            </td>

                            <td>
                                <select
                                    class="status-select"
                                    :class="statusClass(order.status)"
                                    :value="order.status"
                                    :disabled="updatingStatusId === order.id"
                                    :aria-label="`Status for ${order.title}`"
                                    @change="onStatusSelectChange(order.id, $event)"
                                >
                                    <option
                                        v-for="opt in statusOptions"
                                        :key="opt.value"
                                        :value="opt.value"
                                    >
                                        {{ opt.label }}
                                    </option>
                                </select>
                            </td>

                            <td>{{ formatDate(order.createdAt) }}</td>

                            <td>
                                <div class="action-group">
                                    <button
                                        class="icon-button"
                                        title="View details"
                                        @click="viewWorkOrder(order.id)"
                                    >
                                        <Eye :size="16" />
                                    </button>

                                    <button
                                        class="icon-button"
                                        title="Edit work order"
                                        @click="openEditModal(order)"
                                    >
                                        <Pencil :size="16" />
                                    </button>
                                </div>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>

            <!-- Pagination -->
            <div
                v-if="!loading && !error && total > 0"
                class="table-footer"
            >
                <span>
                    Showing {{ (page - 1) * pageSize + 1 }}–{{
                        Math.min(page * pageSize, total)
                    }} of {{ total }} work orders
                </span>

                <div class="pagination">
                    <button
                        class="page-button"
                        :disabled="page === 1"
                        @click="changePage(page - 1)"
                    >
                        Previous
                    </button>

                    <span>Page {{ page }} of {{ totalPages }}</span>

                    <button
                        class="page-button"
                        :disabled="page >= totalPages"
                        @click="changePage(page + 1)"
                    >
                        Next
                    </button>
                </div>
            </div>

        </div>

        <!-- Create / Edit Work Order Modal -->
        <div
            v-if="showModal"
            class="modal-overlay"
            @click.self="closeFormModal"
        >
            <div class="modal-container">

                <div class="modal-header">
                    <div>
                        <h3>{{ isEditing ? 'Edit Work Order' : 'Create Work Order' }}</h3>
                        <p>
                            {{
                                isEditing
                                    ? 'Update the details of the selected work order.'
                                    : 'Enter the details of the new work order.'
                            }}
                        </p>
                    </div>

                    <button
                        class="modal-close"
                        @click="closeFormModal"
                        :disabled="submitting"
                        aria-label="Close modal"
                    >
                        <X :size="20" />
                    </button>
                </div>

                <form @submit.prevent="saveWorkOrder">

                    <div class="modal-body">

                        <div class="form-group">
                            <label for="title">
                                Title <span>*</span>
                            </label>

                            <input
                                id="title"
                                v-model="form.title"
                                type="text"
                                maxlength="150"
                                placeholder="Enter work order title"
                                required
                            />
                        </div>

                        <div class="form-group">
                            <label for="description">
                                Description
                            </label>

                            <textarea
                                id="description"
                                v-model="form.description"
                                maxlength="500"
                                rows="4"
                                placeholder="Describe the work required..."
                            ></textarea>

                            <small>
                                {{ form.description.length }}/500 characters
                            </small>
                        </div>

                        <div class="form-row">

                            <div class="form-group">
                                <label for="priority">
                                    Priority <span>*</span>
                                </label>

                                <select
                                    id="priority"
                                    v-model="form.priority"
                                    required
                                >
                                    <option value="Low">Low</option>
                                    <option value="Medium">Medium</option>
                                    <option value="High">High</option>
                                </select>
                            </div>

                            <div class="form-group">
                                <label for="branch">
                                    Branch <span>*</span>
                                </label>

                                <select
                                    id="branch"
                                    v-model="form.branchId"
                                    required
                                    :disabled="branchesLoading"
                                >
                                    <option value="" disabled>
                                        {{
                                            branchesLoading
                                                ? 'Loading branches...'
                                                : 'Select branch'
                                        }}
                                    </option>

                                    <option
                                        v-for="branch in branches"
                                        :key="branch.id"
                                        :value="String(branch.id)"
                                    >
                                        {{ branch.name }}
                                    </option>
                                </select>
                            </div>

                        </div>

                        <div v-if="formError" class="form-error">
                            {{ formError }}
                        </div>

                    </div>

                    <div class="modal-footer">
                        <button
                            type="button"
                            class="secondary-button"
                            @click="closeFormModal"
                            :disabled="submitting"
                        >
                            Cancel
                        </button>

                        <button
                            type="submit"
                            class="primary-button"
                            :disabled="submitting || branchesLoading || branches.length === 0"
                        >
                            {{
                                submitting
                                    ? (isEditing ? 'Saving...' : 'Creating...')
                                    : (isEditing ? 'Save Changes' : 'Create Work Order')
                            }}
                        </button>
                    </div>

                </form>
            </div>
        </div>


        <!-- Work Order Details Modal -->
        <div
            v-if="detailsLoading || selectedWorkOrder || detailsError"
            class="modal-overlay"
            @click.self="selectedWorkOrder = null; detailsError = ''"
        >
            <div class="modal-container">

                <div class="modal-header">
                    <div>
                        <h3>Work Order Details</h3>
                        <p>View complete work order information.</p>
                    </div>

                    <button
                        class="modal-close"
                        @click="selectedWorkOrder = null; detailsError = ''"
                        aria-label="Close modal"
                    >
                        <X :size="20" />
                    </button>
                </div>

                <div v-if="detailsLoading" class="table-message">
                    Loading work order details...
                </div>

                <div v-else-if="detailsError && !selectedWorkOrder" class="modal-body">
                    <div class="form-error">{{ detailsError }}</div>
                </div>

                <div v-else-if="selectedWorkOrder" class="modal-body">

                    <div class="detail-reference">
                        WO-{{ String(selectedWorkOrder.id).padStart(4, '0') }}
                    </div>

                    <div class="detail-group">
                        <label>Title</label>
                        <strong>{{ selectedWorkOrder.title }}</strong>
                    </div>

                    <div class="detail-group">
                        <label>Description</label>
                        <p>
                            {{ selectedWorkOrder.description || 'No description provided.' }}
                        </p>
                    </div>

                    <div class="detail-grid">

                        <div class="detail-group">
                            <label>Branch</label>
                            <strong>{{ selectedWorkOrder.branchName }}</strong>
                        </div>

                        <div class="detail-group">
                            <label>Priority</label>
                            <span
                                class="priority-badge"
                                :class="priorityClass(selectedWorkOrder.priority)"
                            >
                                {{ selectedWorkOrder.priority }}
                            </span>
                        </div>

                        <div class="detail-group">
                            <label>Status</label>
                            <div>
                                <select
                                    class="status-select"
                                    :class="statusClass(selectedWorkOrder.status)"
                                    :value="selectedWorkOrder.status"
                                    :disabled="updatingStatusId === selectedWorkOrder.id"
                                    aria-label="Work order status"
                                    @change="onStatusSelectChange(selectedWorkOrder.id, $event)"
                                >
                                    <option
                                        v-for="opt in statusOptions"
                                        :key="opt.value"
                                        :value="opt.value"
                                    >
                                        {{ opt.label }}
                                    </option>
                                </select>
                            </div>
                        </div>

                        <div class="detail-group">
                            <label>Date Created</label>
                            <strong>{{ formatDate(selectedWorkOrder.createdAt) }}</strong>
                        </div>

                    </div>

                    <div v-if="detailsError" class="form-error">
                        {{ detailsError }}
                    </div>
                </div>

                <div class="modal-footer">
                    <button
                        v-if="selectedWorkOrder"
                        type="button"
                        class="secondary-button"
                        @click="openEditModal(selectedWorkOrder)"
                    >
                        <Pencil :size="14" />
                        Edit
                    </button>

                    <button
                        class="secondary-button"
                        @click="selectedWorkOrder = null; detailsError = ''"
                    >
                        Close
                    </button>
                </div>

            </div>
        </div>
    </div>
</template>