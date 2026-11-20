<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { RefreshCw, CheckCircle2, XCircle } from 'lucide-vue-next'
import { dashboardService } from '../services/dashboardService'
import { getApiErrorMessage } from '../services/api'

const apiBaseUrl = import.meta.env.VITE_API_URL
const checking = ref(false)
const apiConnected = ref<boolean | null>(null)
const statusMessage = ref('')

async function checkConnection() {
    checking.value = true
    statusMessage.value = ''

    try {
        await dashboardService.getSummary()
        apiConnected.value = true
        statusMessage.value = 'Connected to backend API.'
    } catch (err) {
        apiConnected.value = false
        statusMessage.value = getApiErrorMessage(err, 'Unable to reach backend API.')
    } finally {
        checking.value = false
    }
}

onMounted(() => {
    checkConnection()
})
</script>

<template>
    <div class="settings-page">
        <div class="page-heading">
            <div>
                <h2>Settings</h2>
                <p>Application configuration and API connectivity status.</p>
            </div>

            <button
                type="button"
                class="secondary-button"
                :disabled="checking"
                @click="checkConnection"
            >
                <RefreshCw :size="15" />
                {{ checking ? 'Checking...' : 'Test Connection' }}
            </button>
        </div>

        <div class="content-card">
            <div class="card-header">
                <div>
                    <h3>System Environment</h3>
                    <p>Active runtime settings for the Work Order &amp; Incident Management System</p>
                </div>
            </div>

            <div class="modal-body">
                <div class="detail-grid" style="border-top: none; padding-top: 0;">
                    <div class="detail-group">
                        <label>Backend API URL</label>
                        <strong>{{ apiBaseUrl }}</strong>
                    </div>

                    <div class="detail-group">
                        <label>API Connection Status</label>
                        <div style="display: inline-flex; align-items: center; gap: 6px;">
                            <CheckCircle2
                                v-if="apiConnected === true"
                                :size="16"
                                style="color: #059669;"
                            />
                            <XCircle
                                v-else-if="apiConnected === false"
                                :size="16"
                                style="color: #dc2626;"
                            />
                            <strong>
                                {{ checking ? 'Checking...' : statusMessage }}
                            </strong>
                        </div>
                    </div>

                    <div class="detail-group">
                        <label>Frontend Stack</label>
                        <strong>Vue 3 · TypeScript · Vite · Axios</strong>
                    </div>

                    <div class="detail-group">
                        <label>Backend Stack</label>
                        <strong>ASP.NET Core Web API · EF Core · SQL Server</strong>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>