
<script setup lang="ts">
import {
    ClipboardList,
    Clock,
    Wrench,
    CheckCircle,
    AlertTriangle
} from 'lucide-vue-next'

const statistics = [
    {
        label: 'Total Work Orders',
        value: '128',
        icon: ClipboardList,
        color: 'blue'
    },
    {
        label: 'Pending',
        value: '18',
        icon: Clock,
        color: 'orange'
    },
    {
        label: 'In Progress',
        value: '14',
        icon: Wrench,
        color: 'purple'
    },
    {
        label: 'Completed',
        value: '96',
        icon: CheckCircle,
        color: 'green'
    }
]
</script>

<template>
    <div class="dashboard">
        <div class="page-heading">
            <div>
                <h2>Overview</h2>
                <p>Here's what's happening with your operations.</p>
            </div>
        </div>

        <div class="stats-grid">
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

                <h3>{{ stat.value }}</h3>
            </div>
        </div>

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

                <div class="empty-state">
                    <ClipboardList :size="32" />
                    <p>Work orders will appear here.</p>
                </div>
            </section>

            <section class="content-card">
                <div class="card-header">
                    <div>
                        <h3>Incident Reports</h3>
                        <p>Incident monitoring</p>
                    </div>

                    <RouterLink to="/incidents" class="text-link">
                        View all
                    </RouterLink>
                </div>

                <div class="empty-state">
                    <AlertTriangle :size="32" />
                    <p>No incident data available yet.</p>
                </div>
            </section>
        </div>
    </div>
</template>
