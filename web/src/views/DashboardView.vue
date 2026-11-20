<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import {
    ClipboardList,
    Clock,
    Wrench,
    CheckCircle,
    AlertTriangle,
    RefreshCw
} from 'lucide-vue-next'

import { dashboardService } from '../services/dashboardService'
import type { DashboardSummary } from '../services/dashboardService'

const summary = ref<DashboardSummary | null>(null)
const loading = ref(false)
const error = ref('')

async function fetchDashboard() {
    loading.value = true
    error.value = ''

    try {
        const response = await dashboardService.getSummary()
        summary.value = response.data
    } catch (err) {
        console.error('Failed to load dashboard:', err)
        error.value = 'Unable to load dashboard summary.'
    } finally {
        loading.value = false
    }
}

const statistics = computed(() => {
    const s = summary.value
    return [
        {
            label: 'Total Work Orders',
            value: s ? String(s.totalWorkOrders) : '—',
            subtext: 'All recorded work orders',
            icon: ClipboardList,
            color: 'blue'
        },
        {
            label: 'Open Work Orders',
            value: s ? String(s.openWorkOrders) : '—',
            subtext: 'Awaiting action',
            icon: Clock,
            color: 'orange'
        },
        {
            label: 'In Progress',
            value: s ? String(s.inProgressWorkOrders) : '—',
            subtext: 'Currently being worked on',
            icon: Wrench,
            color: 'purple'
        },
        {
            label: 'Completed',
            value: s ? String(s.completedWorkOrders) : '—',
            subtext: 'Finished work orders',
            icon: CheckCircle,
            color: 'green'
        },
        {
            label: 'Incident Reports',
            value: s ? String(s.totalIncidentReports) : '—',
            subtext: s ? `${s.openIncidentReports} active / unresolved` : 'All reported incidents',
            icon: AlertTriangle,
            color: 'red'
        }
    ]
})

// SVG Donut geometry (radius = 40, circumference = 2 * PI * 40 ≈ 251.33)
const radius = 40
const circumference = 2 * Math.PI * radius

const statusSegments = computed(() => {
    const s = summary.value
    const total = s?.totalWorkOrders ?? 0

    const raw = [
        { label: 'Open', count: s?.openWorkOrders ?? 0, color: '#ea580c' },
        { label: 'In Progress', count: s?.inProgressWorkOrders ?? 0, color: '#2563eb' },
        { label: 'Done', count: s?.completedWorkOrders ?? 0, color: '#059669' }
    ]

    let offset = 0
    return raw.map(seg => {
        const fraction = total > 0 ? seg.count / total : 0
        const length = fraction * circumference
        const item = {
            ...seg,
            percentage: total > 0 ? Math.round(fraction * 100) : 0,
            dashArray: `${length} ${circumference - length}`,
            dashOffset: -offset
        }
        offset += length
        return item
    })
})

const maxBranchCount = computed(() => {
    const list = summary.value?.branchBreakdown ?? []
    const highest = Math.max(
        1,
        ...list.flatMap(b => [b.workOrders, b.incidents])
    )
    return highest
})

function percentWidth(count: number, total: number) {
    if (total <= 0) return '0%'
    return `${Math.min(100, Math.round((count / total) * 100))}%`
}

function formatDate(date: string) {
    return new Date(date).toLocaleDateString('en-PH', {
        month: 'short',
        day: 'numeric',
        year: 'numeric'
    })
}

function workOrderStatusClass(status: string) {
    const map: Record<string, string> = {
        Open: 'open',
        InProgress: 'in-progress',
        Done: 'done'
    }
    return map[status] ?? ''
}

function workOrderStatusLabel(status: string) {
    return status === 'InProgress' ? 'In Progress' : status
}

onMounted(() => {
    fetchDashboard()
})
</script>

<template>
    <div class="dashboard">
        <div class="page-heading">
            <div>
                <h2>Overview</h2>
                <p>Here's what's happening with your operations.</p>
            </div>

            <button
                type="button"
                class="secondary-button"
                :disabled="loading"
                @click="fetchDashboard"
            >
                <RefreshCw :size="15" />
                Refresh
            </button>
        </div>

        <div v-if="error" class="form-error" style="margin-bottom: 18px;">
            {{ error }}
            <button class="text-link" style="margin-left: 8px;" @click="fetchDashboard">
                Try again
            </button>
        </div>

        <!-- KPI Stat Cards -->
        <div class="stats-grid five-cols">
            <div
                v-for="stat in statistics"
                :key="stat.label"
                class="stat-card"
            >
                <div class="stat-top">
                    <span>{{ stat.label }}</span>

                    <div class="stat-icon" :class="stat.color">
                        <component :is="stat.icon" :size="20" />
                    </div>
                </div>

                <h3>{{ loading && !summary ? '...' : stat.value }}</h3>
                <span class="stat-subtext">{{ stat.subtext }}</span>
            </div>
        </div>

        <!-- Summary Charts -->
        <div v-if="summary" class="charts-grid">
            <!-- Chart 1: Work Order Status Distribution -->
            <section class="content-card">
                <div class="card-header">
                    <div>
                        <h3>Work Order Status</h3>
                        <p>Completion breakdown across all orders</p>
                    </div>
                </div>

                <div class="chart-body">
                    <div class="donut-wrapper">
                        <svg class="donut-chart" viewBox="0 0 100 100" aria-label="Work order status distribution">
                            <circle
                                cx="50"
                                cy="50"
                                :r="radius"
                                fill="none"
                                stroke="#f1f5f9"
                                stroke-width="12"
                            />
                            <template v-if="summary.totalWorkOrders > 0">
                                <circle
                                    v-for="seg in statusSegments"
                                    :key="seg.label"
                                    cx="50"
                                    cy="50"
                                    :r="radius"
                                    fill="none"
                                    :stroke="seg.color"
                                    stroke-width="12"
                                    :stroke-dasharray="seg.dashArray"
                                    :stroke-dashoffset="seg.dashOffset"
                                    transform="rotate(-90 50 50)"
                                />
                            </template>
                            <text
                                x="50"
                                y="48"
                                text-anchor="middle"
                                font-size="14"
                                font-weight="700"
                                fill="#172033"
                            >
                                {{ summary.totalWorkOrders }}
                            </text>
                            <text
                                x="50"
                                y="61"
                                text-anchor="middle"
                                font-size="7"
                                fill="#64748b"
                            >
                                TOTAL
                            </text>
                        </svg>

                        <div class="donut-legend">
                            <div
                                v-for="seg in statusSegments"
                                :key="seg.label"
                                class="legend-row"
                            >
                                <span class="legend-label">
                                    <span
                                        class="legend-dot"
                                        :style="{ backgroundColor: seg.color }"
                                    ></span>
                                    {{ seg.label }}
                                </span>
                                <strong>{{ seg.count }} ({{ seg.percentage }}%)</strong>
                            </div>
                        </div>
                    </div>
                </div>
            </section>

            <!-- Chart 2: Priority & Severity Breakdown -->
            <section class="content-card">
                <div class="card-header">
                    <div>
                        <h3>Priority &amp; Severity</h3>
                        <p>Work order priority vs. incident severity</p>
                    </div>
                </div>

                <div class="chart-body">
                    <div class="bar-list">
                        <div
                            v-for="item in summary.workOrdersByPriority"
                            :key="`wo-${item.label}`"
                            class="bar-item"
                        >
                            <div class="bar-meta">
                                <span>WO · {{ item.label }} Priority</span>
                                <strong>{{ item.count }}</strong>
                            </div>
                            <div class="bar-track">
                                <div
                                    class="bar-fill"
                                    :class="item.label.toLowerCase()"
                                    :style="{ width: percentWidth(item.count, summary.totalWorkOrders) }"
                                ></div>
                            </div>
                        </div>

                        <div
                            v-for="item in summary.incidentsBySeverity"
                            :key="`ir-${item.label}`"
                            class="bar-item"
                        >
                            <div class="bar-meta">
                                <span>IR · {{ item.label }} Severity</span>
                                <strong>{{ item.count }}</strong>
                            </div>
                            <div class="bar-track">
                                <div
                                    class="bar-fill"
                                    :class="item.label.toLowerCase()"
                                    :style="{ width: percentWidth(item.count, summary.totalIncidentReports) }"
                                ></div>
                            </div>
                        </div>
                    </div>
                </div>
            </section>

            <!-- Chart 3: Activity by Branch -->
            <section class="content-card">
                <div class="card-header">
                    <div>
                        <h3>Activity by Branch</h3>
                        <p>Work orders and incidents per location</p>
                    </div>
                </div>

                <div class="chart-body">
                    <div v-if="summary.branchBreakdown.length === 0" class="empty-state">
                        <p>No branch data available.</p>
                    </div>

                    <div v-else class="bar-list">
                        <div
                            v-for="branch in summary.branchBreakdown"
                            :key="branch.branchName"
                            class="bar-item"
                        >
                            <div class="bar-meta">
                                <span>{{ branch.branchName }}</span>
                                <strong>{{ branch.workOrders }} WO · {{ branch.incidents }} IR</strong>
                            </div>
                            <div class="branch-bar-group">
                                <div class="bar-track" title="Work Orders">
                                    <div
                                        class="bar-fill in-progress"
                                        :style="{ width: percentWidth(branch.workOrders, maxBranchCount) }"
                                    ></div>
                                </div>
                                <div class="bar-track" title="Incident Reports">
                                    <div
                                        class="bar-fill high"
                                        :style="{ width: percentWidth(branch.incidents, maxBranchCount) }"
                                    ></div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </section>
        </div>

        <!-- Recent Records Grid -->
        <div class="dashboard-grid">
            <section class="content-card">
                <div class="card-header">
                    <div>
                        <h3>Recent Work Orders</h3>
                        <p>Latest recorded work orders</p>
                    </div>

                    <RouterLink to="/work-orders" class="text-link">
                        View all
                    </RouterLink>
                </div>

                <div v-if="loading && !summary" class="table-message">
                    Loading recent work orders...
                </div>

                <div
                    v-else-if="!summary || summary.recentWorkOrders.length === 0"
                    class="empty-state"
                >
                    <ClipboardList :size="32" />
                    <p>Work orders will appear here.</p>
                </div>

                <div v-else class="activity-list">
                    <div
                        v-for="order in summary.recentWorkOrders"
                        :key="order.id"
                        class="activity-item"
                    >
                        <div class="activity-info">
                            <strong>{{ order.title }}</strong>
                            <span>
                                WO-{{ String(order.id).padStart(4, '0') }} ·
                                {{ order.branchName }} ·
                                {{ formatDate(order.createdAt) }}
                            </span>
                        </div>

                        <div class="activity-badges">
                            <span
                                class="priority-badge"
                                :class="order.priority.toLowerCase()"
                            >
                                {{ order.priority }}
                            </span>
                            <span
                                class="status-badge"
                                :class="workOrderStatusClass(order.status)"
                            >
                                {{ workOrderStatusLabel(order.status) }}
                            </span>
                        </div>
                    </div>
                </div>
            </section>

            <section class="content-card">
                <div class="card-header">
                    <div>
                        <h3>Recent Incident Reports</h3>
                        <p>Latest reported incidents</p>
                    </div>

                    <RouterLink to="/incidents" class="text-link">
                        View all
                    </RouterLink>
                </div>

                <div v-if="loading && !summary" class="table-message">
                    Loading recent incident reports...
                </div>

                <div
                    v-else-if="!summary || summary.recentIncidentReports.length === 0"
                    class="empty-state"
                >
                    <AlertTriangle :size="32" />
                    <p>No incident data available yet.</p>
                </div>

                <div v-else class="activity-list">
                    <div
                        v-for="incident in summary.recentIncidentReports"
                        :key="incident.id"
                        class="activity-item"
                    >
                        <div class="activity-info">
                            <strong>{{ incident.title }}</strong>
                            <span>
                                IR-{{ String(incident.id).padStart(4, '0') }} ·
                                {{ incident.branchName }} ·
                                {{ formatDate(incident.createdAt) }}
                            </span>
                        </div>

                        <div class="activity-badges">
                            <span
                                class="priority-badge"
                                :class="incident.severity.toLowerCase()"
                            >
                                {{ incident.severity }}
                            </span>
                            <span
                                class="status-badge"
                                :class="incident.status.toLowerCase()"
                            >
                                {{ incident.status }}
                            </span>
                        </div>
                    </div>
                </div>
            </section>
        </div>
    </div>
</template>