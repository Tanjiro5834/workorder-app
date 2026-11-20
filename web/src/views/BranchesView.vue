<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import {
    Plus,
    Search,
    RefreshCw,
    Building2,
    X
} from 'lucide-vue-next'

import { branchService } from '../services/branchService'
import type { Branch } from '../services/branchService'
import { getApiErrorMessage } from '../services/api'

const branches = ref<Branch[]>([])
const loading = ref(false)
const submitting = ref(false)
const error = ref('')
const formError = ref('')
const search = ref('')

const showModal = ref(false)
const form = ref({
    name: '',
    address: ''
})

const filteredBranches = computed(() => {
    const keyword = search.value.trim().toLowerCase()
    if (!keyword) return branches.value

    return branches.value.filter(b =>
        b.name.toLowerCase().includes(keyword) ||
        (b.address && b.address.toLowerCase().includes(keyword))
    )
})

async function fetchBranches() {
    loading.value = true
    error.value = ''

    try {
        const response = await branchService.getAll()
        branches.value = response.data
    } catch (err) {
        console.error('Failed to load branches:', err)
        error.value = getApiErrorMessage(err, 'Unable to load branches.')
    } finally {
        loading.value = false
    }
}

function openCreateModal() {
    form.value = { name: '', address: '' }
    formError.value = ''
    showModal.value = true
}

function closeCreateModal() {
    if (submitting.value) return
    showModal.value = false
    formError.value = ''
}

async function createBranch() {
    formError.value = ''
    const name = form.value.name.trim()
    const address = form.value.address.trim()

    if (!name) {
        formError.value = 'Branch name is required.'
        return
    }

    if (name.length > 100) {
        formError.value = 'Branch name cannot exceed 100 characters.'
        return
    }

    if (address.length > 200) {
        formError.value = 'Address cannot exceed 200 characters.'
        return
    }

    submitting.value = true

    try {
        await branchService.create({
            name,
            address: address || null
        })

        showModal.value = false
        await fetchBranches()
    } catch (err) {
        console.error('Failed to create branch:', err)
        formError.value = getApiErrorMessage(err, 'Failed to create branch. Please try again.')
    } finally {
        submitting.value = false
    }
}

onMounted(() => {
    fetchBranches()
})
</script>

<template>
    <div class="branches-page">
        <div class="page-heading">
            <div>
                <h2>Branches</h2>
                <p>Manage company branches and view operational activity per location.</p>
            </div>

            <button class="primary-button" @click="openCreateModal">
                <Plus :size="17" />
                New Branch
            </button>
        </div>

        <div class="content-card">
            <div class="table-toolbar">
                <div class="search-box">
                    <Search :size="17" />
                    <input
                        v-model="search"
                        type="text"
                        placeholder="Search branch name or address..."
                    />
                </div>

                <button
                    type="button"
                    class="secondary-button"
                    :disabled="loading"
                    @click="fetchBranches"
                >
                    <RefreshCw :size="15" />
                    Refresh
                </button>
            </div>

            <div v-if="loading" class="table-message">
                Loading branches...
            </div>

            <div v-else-if="error" class="table-message error-message">
                {{ error }}
                <button class="text-link" @click="fetchBranches">
                    Try again
                </button>
            </div>

            <div v-else-if="filteredBranches.length === 0" class="table-message">
                <Building2 :size="32" />
                <strong>No branches found</strong>
                <span>
                    {{ search.trim() ? 'No branches match your search.' : 'Add a branch to get started.' }}
                </span>
            </div>

            <div v-else class="table-wrapper">
                <table class="data-table">
                    <thead>
                        <tr>
                            <th>BRANCH</th>
                            <th>ADDRESS</th>
                            <th>WORK ORDERS</th>
                            <th>INCIDENT REPORTS</th>
                        </tr>
                    </thead>

                    <tbody>
                        <tr v-for="branch in filteredBranches" :key="branch.id">
                            <td>
                                <div class="order-title">
                                    <strong>{{ branch.name }}</strong>
                                    <span>BR-{{ String(branch.id).padStart(3, '0') }}</span>
                                </div>
                            </td>
                            <td>{{ branch.address || 'No address recorded' }}</td>
                            <td>{{ branch.workOrderCount }}</td>
                            <td>{{ branch.incidentCount }}</td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>

        <!-- Create Branch Modal -->
        <div
            v-if="showModal"
            class="modal-overlay"
            @click.self="closeCreateModal"
        >
            <div class="modal-container">
                <div class="modal-header">
                    <div>
                        <h3>Create Branch</h3>
                        <p>Add a new company branch location.</p>
                    </div>

                    <button
                        class="modal-close"
                        :disabled="submitting"
                        aria-label="Close modal"
                        @click="closeCreateModal"
                    >
                        <X :size="20" />
                    </button>
                </div>

                <form @submit.prevent="createBranch">
                    <div class="modal-body">
                        <div class="form-group">
                            <label for="branch-name">
                                Branch Name <span>*</span>
                            </label>
                            <input
                                id="branch-name"
                                v-model="form.name"
                                type="text"
                                maxlength="100"
                                placeholder="e.g. Imus Branch"
                                required
                            />
                        </div>

                        <div class="form-group">
                            <label for="branch-address">
                                Address
                            </label>
                            <input
                                id="branch-address"
                                v-model="form.address"
                                type="text"
                                maxlength="200"
                                placeholder="Street, City, Province (optional)"
                            />
                        </div>

                        <div v-if="formError" class="form-error">
                            {{ formError }}
                        </div>
                    </div>

                    <div class="modal-footer">
                        <button
                            type="button"
                            class="secondary-button"
                            :disabled="submitting"
                            @click="closeCreateModal"
                        >
                            Cancel
                        </button>

                        <button
                            type="submit"
                            class="primary-button"
                            :disabled="submitting"
                        >
                            {{ submitting ? 'Creating...' : 'Create Branch' }}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    </div>
</template>