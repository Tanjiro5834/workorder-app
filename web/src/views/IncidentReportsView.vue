<script setup lang="ts">
import { ref, computed, watch, onMounted, onBeforeUnmount } from 'vue'
import {
    Plus,
    Search,
    RefreshCw,
    AlertTriangle,
    X,
    Eye,
    Pencil,
    Trash2,
    RotateCcw
} from 'lucide-vue-next'

import { incidentReportService } from '../services/incidentReportService'
import type {
    IncidentReport,
    IncidentStatus,
    IncidentSeverity
} from '../services/incidentReportService'

import { branchService } from '../services/branchService'
import type { Branch } from '../services/branchService'

const statusOptions: { value: IncidentStatus; label: string }[] = [
    { value: 'Open', label: 'Open' },
    { value: 'Investigating', label: 'Investigating' },
    { value: 'Resolved', label: 'Resolved' }
]

const severityOptions: IncidentSeverity[] = ['Low', 'Medium', 'High', 'Critical']
const pageSizeOptions = [10, 20, 50]

const incidents = ref<IncidentReport[]>([])
const branches = ref<Branch[]>([])

const loading = ref(false)
const branchesLoading = ref(false)
const submitting = ref(false)
const updatingStatusId = ref<number | null>(null)
const deleting = ref(false)

const error = ref('')
const formError = ref('')
const statusError = ref('')
const deleteError = ref('')

const page = ref(1)
const pageSize = ref(10)
const total = ref(0)

const search = ref('')
const statusFilter = ref<IncidentStatus | ''>('')
const severityFilter = ref<IncidentSeverity | ''>('')

let searchDebounceTimer: ReturnType<typeof setTimeout> | null = null

// Modal state
const showModal = ref(false)
const editingIncidentId = ref<number | null>(null)
const selectedIncident = ref<IncidentReport | null>(null)
const incidentToDelete = ref<IncidentReport | null>(null)
const detailsLoading = ref(false)
const detailsError = ref('')

// Form state
const form = ref({
    title: '',
    description: '',
    reportedBy: '',
    severity: 'Medium' as IncidentSeverity,
    branchId: ''
})

const isEditing = computed(() => editingIncidentId.value !== null)

const totalPages = computed(() =>
    Math.max(1, Math.ceil(total.value / pageSize.value))
)

const hasActiveFilters = computed(() =>
    Boolean(
        search.value.trim() ||
        statusFilter.value ||
        severityFilter.value
    )
)

// Fetch incident reports
async function fetchIncidents() {
    loading.value = true
    error.value = ''
    statusError.value = ''

    try {
        const response = await incidentReportService.getAll({
            page: page.value,
            pageSize: pageSize.value,
            status: statusFilter.value,
            severity: severityFilter.value,
            search: search.value
        })

        incidents.value = response.data.items
        total.value = response.data.total
    } catch (err: any) {
        console.error('API ERROR:', err)
        error.value = 'Unable to load incident reports.'
    } finally {
        loading.value = false
    }
}

watch(search, () => {
    if (searchDebounceTimer) {
        clearTimeout(searchDebounceTimer)
    }

    searchDebounceTimer = setTimeout(() => {
        page.value = 1
        fetchIncidents()
    }, 300)
})

function onFilterChange() {
    page.value = 1
    fetchIncidents()
}

function onPageSizeChange() {
    page.value = 1
    fetchIncidents()
}

function clearFilters() {
    if (searchDebounceTimer) {
        clearTimeout(searchDebounceTimer)
        searchDebounceTimer = null
    }

    search.value = ''
    statusFilter.value = ''
    severityFilter.value = ''
    page.value = 1
    fetchIncidents()
}

async function viewIncident(id: number) {
    selectedIncident.value = null
    detailsError.value = ''
    detailsLoading.value = true

    try {
        const response = await incidentReportService.getById(id)
        selectedIncident.value = response.data
    } catch (err) {
        console.error('Failed to load incident report:', err)
        detailsError.value = 'Unable to load incident report details.'
    } finally {
        detailsLoading.value = false
    }
}

async function updateIncidentStatus(id: number, newStatus: IncidentStatus) {
    if (updatingStatusId.value === id) return

    statusError.value = ''
    detailsError.value = ''
    updatingStatusId.value = id

    try {
        await incidentReportService.updateStatus(id, newStatus)

        if (statusFilter.value && statusFilter.value !== newStatus) {
            if (incidents.value.length === 1 && page.value > 1) {
                page.value -= 1
            }
            await fetchIncidents()
        } else {
            const existing = incidents.value.find(i => i.id === id)
            if (existing) {
                existing.status = newStatus
            }
        }

        if (selectedIncident.value?.id === id) {
            selectedIncident.value.status = newStatus
        }
    } catch (err) {
        console.error('Failed to update status:', err)
        const message = 'Failed to update incident status. Please try again.'
        if (selectedIncident.value?.id === id) {
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
    const newStatus = select.value as IncidentStatus
    const current = incidents.value.find(i => i.id === id)?.status
        ?? selectedIncident.value?.status

    updateIncidentStatus(id, newStatus).finally(() => {
        if (current && select.value !== current) {
            const latest = incidents.value.find(i => i.id === id)?.status
                ?? selectedIncident.value?.status
            if (latest) select.value = latest
        }
    })
}

// Delete modal handlers
function openDeleteModal(incident: IncidentReport) {
    deleteError.value = ''
    incidentToDelete.value = incident
}

function closeDeleteModal() {
    if (deleting.value) return
    incidentToDelete.value = null
    deleteError.value = ''
}

async function confirmDeleteIncident() {
    if (!incidentToDelete.value || deleting.value) return

    const targetId = incidentToDelete.value.id
    deleting.value = true
    deleteError.value = ''

    try {
        await incidentReportService.delete(targetId)

        if (selectedIncident.value?.id === targetId) {
            selectedIncident.value = null
            detailsError.value = ''
        }

        incidentToDelete.value = null

        if (incidents.value.length === 1 && page.value > 1) {
            page.value -= 1
        }

        await fetchIncidents()
    } catch (err: any) {
        console.error('Failed to delete incident report:', err)
        deleteError.value =
            err.response?.data?.error ||
            'Failed to delete incident report. Please try again.'
    } finally {
        deleting.value = false
    }
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
    editingIncidentId.value = null
    resetForm()
    formError.value = ''
    showModal.value = true

    if (branches.value.length === 0) {
        await fetchBranches()
    }
}

// Open edit modal
async function openEditModal(incident: IncidentReport) {
    editingIncidentId.value = incident.id
    form.value = {
        title: incident.title,
        description: incident.description ?? '',
        reportedBy: incident.reportedBy ?? '',
        severity: incident.severity,
        branchId: String(incident.branchId)
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
    editingIncidentId.value = null
    formError.value = ''

    resetForm()
}

// Reset form
function resetForm() {
    form.value = {
        title: '',
        description: '',
        reportedBy: '',
        severity: 'Medium',
        branchId: ''
    }
}

// Create or update incident report
async function saveIncident() {
    formError.value = ''

    const title = form.value.title.trim()
    const description = form.value.description.trim()
    const reportedBy = form.value.reportedBy.trim()

    if (!title) {
        formError.value = 'Incident title is required.'
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

    if (reportedBy.length > 100) {
        formError.value = 'Reported by cannot exceed 100 characters.'
        return
    }

    if (!form.value.branchId) {
        formError.value = 'Please select a branch.'
        return
    }

    const payload = {
        title,
        description: description || null,
        reportedBy: reportedBy || null,
        severity: form.value.severity,
        branchId: Number(form.value.branchId)
    }

    submitting.value = true

    try {
        if (editingIncidentId.value !== null) {
            const response = await incidentReportService.update(
                editingIncidentId.value,
                payload
            )
            const updated = response.data

            if (selectedIncident.value?.id === updated.id) {
                selectedIncident.value = updated
            }

            showModal.value = false
            editingIncidentId.value = null
            resetForm()

            await fetchIncidents()
        } else {
            await incidentReportService.create(payload)

            showModal.value = false
            resetForm()

            page.value = 1
            await fetchIncidents()
        }
    } catch (err: any) {
        console.error(err)

        const data = err.response?.data
        const validationMessage = data?.errors
            ? Object.values(data.errors).flat().join(' ')
            : null

        formError.value =
            data?.error ||
            validationMessage ||
            data?.title ||
            `Failed to ${isEditing.value ? 'update' : 'create'} incident report. Please try again.`
    } finally {
        submitting.value = false
    }
}

// Pagination
function changePage(newPage: number) {
    if (newPage < 1 || newPage > totalPages.value) return

    page.value = newPage
    fetchIncidents()
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
    return status.toLowerCase()
}

function severityClass(severity: string) {
    return severity.toLowerCase()
}

onMounted(() => {
    fetchIncidents()
})

onBeforeUnmount(() => {
    if (searchDebounceTimer) {
        clearTimeout(searchDebounceTimer)
    }
})
</script>

<template>
    <div class="incident-reports-page">

        <!-- Page Header -->
        <div class="page-heading">
            <div>
                <h2>Incident Reports</h2>
                <p>Monitor and manage reported incidents.</p>
            </div>

            <button
                class="primary-button"
                @click="openCreateModal"
            >
                <Plus :size="17" />
                Report Incident
            </button>
        </div>

        <!-- Incident Reports Table -->
        <div class="content-card">

            <div class="table-toolbar">
                <div class="toolbar-controls">
                    <div class="search-box">
                        <Search :size="17" />

                        <input
                            v-model="search"
                            type="text"
                            placeholder="Search title, branch, or reporter..."
                        />
                    </div>

                    <select
                        v-model="statusFilter"
                        class="filter-select"
                        aria-label="Filter by status"
                        @change="onFilterChange"
                    >
                        <option value="">All Statuses</option>
                        <option
                            v-for="opt in statusOptions"
                            :key="opt.value"
                            :value="opt.value"
                        >
                            {{ opt.label }}
                        </option>
                    </select>

                    <select
                        v-model="severityFilter"
                        class="filter-select"
                        aria-label="Filter by severity"
                        @change="onFilterChange"
                    >
                        <option value="">All Severities</option>
                        <option
                            v-for="s in severityOptions"
                            :key="s"
                            :value="s"
                        >
                            {{ s }}
                        </option>
                    </select>
                </div>

                <div class="toolbar-actions">
                    <button
                        v-if="hasActiveFilters"
                        type="button"
                        class="secondary-button"
                        @click="clearFilters"
                        :disabled="loading"
                    >
                        <RotateCcw :size="15" />
                        Clear
                    </button>

                    <button
                        type="button"
                        class="secondary-button"
                        @click="fetchIncidents"
                        :disabled="loading"
                    >
                        <RefreshCw :size="15" />
                        Refresh
                    </button>
                </div>
            </div>

            <div v-if="statusError" style="padding: 12px 20px 0;">
                <div class="form-error">{{ statusError }}</div>
            </div>

            <div v-if="loading" class="table-message">
                Loading incident reports...
            </div>

            <div v-else-if="error" class="table-message error-message">
                {{ error }}

                <button class="text-link" @click="fetchIncidents">
                    Try again
                </button>
            </div>

            <div v-else-if="incidents.length === 0" class="table-message">
                <AlertTriangle :size="32" />
                <template v-if="hasActiveFilters">
                    <strong>No matching incident reports</strong>
                    <span>Try adjusting your search or filter criteria.</span>
                    <button
                        type="button"
                        class="secondary-button"
                        @click="clearFilters"
                    >
                        Clear Filters
                    </button>
                </template>
                <template v-else>
                    <strong>No incident reports yet</strong>
                    <span>Log an incident report to start tracking issues.</span>
                    <button
                        type="button"
                        class="primary-button"
                        @click="openCreateModal"
                    >
                        <Plus :size="15" />
                        Report Incident
                    </button>
                </template>
            </div>

            <div v-else class="table-wrapper">
                <table class="data-table">
                    <thead>
                        <tr>
                            <th>INCIDENT</th>
                            <th>BRANCH</th>
                            <th>REPORTED BY</th>
                            <th>SEVERITY</th>
                            <th>STATUS</th>
                            <th>DATE REPORTED</th>
                            <th>ACTIONS</th>
                        </tr>
                    </thead>

                    <tbody>
                        <tr
                            v-for="incident in incidents"
                            :key="incident.id"
                        >
                            <td>
                                <div class="order-title">
                                    <strong>{{ incident.title }}</strong>
                                    <span>
                                        IR-{{ String(incident.id).padStart(4, '0') }}
                                    </span>
                                </div>
                            </td>

                            <td>{{ incident.branchName }}</td>

                            <td>{{ incident.reportedBy || '—' }}</td>

                            <td>
                                <span
                                    class="priority-badge"
                                    :class="severityClass(incident.severity)"
                                >
                                    {{ incident.severity }}
                                </span>
                            </td>

                            <td>
                                <select
                                    class="status-select"
                                    :class="statusClass(incident.status)"
                                    :value="incident.status"
                                    :disabled="updatingStatusId === incident.id"
                                    :aria-label="`Status for ${incident.title}`"
                                    @change="onStatusSelectChange(incident.id, $event)"
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

                            <td>{{ formatDate(incident.createdAt) }}</td>

                            <td>
                                <div class="action-group">
                                    <button
                                        class="icon-button"
                                        title="View details"
                                        @click="viewIncident(incident.id)"
                                    >
                                        <Eye :size="16" />
                                    </button>

                                    <button
                                        class="icon-button"
                                        title="Edit incident report"
                                        @click="openEditModal(incident)"
                                    >
                                        <Pencil :size="16" />
                                    </button>

                                    <button
                                        class="icon-button danger"
                                        title="Delete incident report"
                                        @click="openDeleteModal(incident)"
                                    >
                                        <Trash2 :size="16" />
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
                    }} of {{ total }} incident reports
                </span>

                <div class="pagination-controls">
                    <label class="page-size-control">
                        <span>Rows</span>
                        <select
                            v-model.number="pageSize"
                            aria-label="Rows per page"
                            @change="onPageSizeChange"
                        >
                            <option
                                v-for="size in pageSizeOptions"
                                :key="size"
                                :value="size"
                            >
                                {{ size }}
                            </option>
                        </select>
                    </label>

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

        </div>

        <!-- Create / Edit Incident Report Modal -->
        <div
            v-if="showModal"
            class="modal-overlay"
            @click.self="closeFormModal"
        >
            <div class="modal-container">

                <div class="modal-header">
                    <div>
                        <h3>{{ isEditing ? 'Edit Incident Report' : 'Report Incident' }}</h3>
                        <p>
                            {{
                                isEditing
                                    ? 'Update the details of the selected incident report.'
                                    : 'Enter the details of the incident.'
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

                <form @submit.prevent="saveIncident">

                    <div class="modal-body">

                        <div class="form-group">
                            <label for="incident-title">
                                Title <span>*</span>
                            </label>

                            <input
                                id="incident-title"
                                v-model="form.title"
                                type="text"
                                maxlength="150"
                                placeholder="Enter incident title"
                                required
                            />
                        </div>

                        <div class="form-group">
                            <label for="incident-reported-by">
                                Reported By
                            </label>

                            <input
                                id="incident-reported-by"
                                v-model="form.reportedBy"
                                type="text"
                                maxlength="100"
                                placeholder="Name of person reporting (optional)"
                            />
                        </div>

                        <div class="form-group">
                            <label for="incident-description">
                                Description
                            </label>

                            <textarea
                                id="incident-description"
                                v-model="form.description"
                                maxlength="500"
                                rows="4"
                                placeholder="Describe what happened..."
                            ></textarea>

                            <small>
                                {{ form.description.length }}/500 characters
                            </small>
                        </div>

                        <div class="form-row">

                            <div class="form-group">
                                <label for="incident-severity">
                                    Severity <span>*</span>
                                </label>

                                <select
                                    id="incident-severity"
                                    v-model="form.severity"
                                    required
                                >
                                    <option value="Low">Low</option>
                                    <option value="Medium">Medium</option>
                                    <option value="High">High</option>
                                    <option value="Critical">Critical</option>
                                </select>
                            </div>

                            <div class="form-group">
                                <label for="incident-branch">
                                    Branch <span>*</span>
                                </label>

                                <select
                                    id="incident-branch"
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
                                    ? (isEditing ? 'Saving...' : 'Submitting...')
                                    : (isEditing ? 'Save Changes' : 'Submit Report')
                            }}
                        </button>
                    </div>

                </form>
            </div>
        </div>

        <!-- Incident Report Details Modal -->
        <div
            v-if="detailsLoading || selectedIncident || detailsError"
            class="modal-overlay"
            @click.self="selectedIncident = null; detailsError = ''"
        >
            <div class="modal-container">

                <div class="modal-header">
                    <div>
                        <h3>Incident Report Details</h3>
                        <p>View complete incident report information.</p>
                    </div>

                    <button
                        class="modal-close"
                        @click="selectedIncident = null; detailsError = ''"
                        aria-label="Close modal"
                    >
                        <X :size="20" />
                    </button>
                </div>

                <div v-if="detailsLoading" class="table-message">
                    Loading incident report details...
                </div>

                <div v-else-if="detailsError && !selectedIncident" class="modal-body">
                    <div class="form-error">{{ detailsError }}</div>
                </div>

                <div v-else-if="selectedIncident" class="modal-body">

                    <div class="detail-reference">
                        IR-{{ String(selectedIncident.id).padStart(4, '0') }}
                    </div>

                    <div class="detail-group">
                        <label>Title</label>
                        <strong>{{ selectedIncident.title }}</strong>
                    </div>

                    <div class="detail-group">
                        <label>Description</label>
                        <p>
                            {{ selectedIncident.description || 'No description provided.' }}
                        </p>
                    </div>

                    <div class="detail-grid">

                        <div class="detail-group">
                            <label>Branch</label>
                            <strong>{{ selectedIncident.branchName }}</strong>
                        </div>

                        <div class="detail-group">
                            <label>Reported By</label>
                            <strong>{{ selectedIncident.reportedBy || 'Not specified' }}</strong>
                        </div>

                        <div class="detail-group">
                            <label>Severity</label>
                            <span
                                class="priority-badge"
                                :class="severityClass(selectedIncident.severity)"
                            >
                                {{ selectedIncident.severity }}
                            </span>
                        </div>

                        <div class="detail-group">
                            <label>Status</label>
                            <div>
                                <select
                                    class="status-select"
                                    :class="statusClass(selectedIncident.status)"
                                    :value="selectedIncident.status"
                                    :disabled="updatingStatusId === selectedIncident.id"
                                    aria-label="Incident status"
                                    @change="onStatusSelectChange(selectedIncident.id, $event)"
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
                            <label>Date Reported</label>
                            <strong>{{ formatDate(selectedIncident.createdAt) }}</strong>
                        </div>

                    </div>

                    <div v-if="detailsError" class="form-error">
                        {{ detailsError }}
                    </div>
                </div>

                <div class="modal-footer">
                    <button
                        v-if="selectedIncident"
                        type="button"
                        class="danger-button"
                        @click="openDeleteModal(selectedIncident)"
                    >
                        <Trash2 :size="14" />
                        Delete
                    </button>

                    <button
                        v-if="selectedIncident"
                        type="button"
                        class="secondary-button"
                        @click="openEditModal(selectedIncident)"
                    >
                        <Pencil :size="14" />
                        Edit
                    </button>

                    <button
                        class="secondary-button"
                        @click="selectedIncident = null; detailsError = ''"
                    >
                        Close
                    </button>
                </div>

            </div>
        </div>

        <!-- Delete Confirmation Modal -->
        <div
            v-if="incidentToDelete"
            class="modal-overlay"
            @click.self="closeDeleteModal"
        >
            <div class="modal-container">

                <div class="modal-header">
                    <div>
                        <h3>Delete Incident Report</h3>
                        <p>This action cannot be undone.</p>
                    </div>

                    <button
                        class="modal-close"
                        @click="closeDeleteModal"
                        :disabled="deleting"
                        aria-label="Close modal"
                    >
                        <X :size="20" />
                    </button>
                </div>

                <div class="modal-body">
                    <p class="confirm-text">
                        Are you sure you want to delete
                        <strong>
                            IR-{{ String(incidentToDelete.id).padStart(4, '0') }}
                            ({{ incidentToDelete.title }})
                        </strong>?
                    </p>

                    <div v-if="deleteError" class="form-error">
                        {{ deleteError }}
                    </div>
                </div>

                <div class="modal-footer">
                    <button
                        type="button"
                        class="secondary-button"
                        @click="closeDeleteModal"
                        :disabled="deleting"
                    >
                        Cancel
                    </button>

                    <button
                        type="button"
                        class="danger-button"
                        @click="confirmDeleteIncident"
                        :disabled="deleting"
                    >
                        {{ deleting ? 'Deleting...' : 'Delete Incident' }}
                    </button>
                </div>

            </div>
        </div>
    </div>
</template>