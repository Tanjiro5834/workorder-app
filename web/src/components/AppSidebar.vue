
<script setup lang="ts">
import { RouterLink } from 'vue-router'
import {
    LayoutDashboard,
    ClipboardList,
    AlertTriangle,
    Building2,
    Settings,
    ClipboardCheck
} from 'lucide-vue-next'

const navigation = [
    {
        label: 'Dashboard',
        path: '/',
        icon: LayoutDashboard
    },
    {
        label: 'Work Orders',
        path: '/work-orders',
        icon: ClipboardList
    },
    {
        label: 'Incident Reports',
        path: '/incidents',
        icon: AlertTriangle
    },
    {
        label: 'Branches',
        path: '/branches',
        icon: Building2
    }
]
</script>

<template>
    <aside class="sidebar">
        <div class="sidebar-brand">
            <div class="brand-icon">
                <ClipboardCheck :size="22" />
            </div>

            <div>
                <h2>WorkOrder</h2>
                <span>Management System</span>
            </div>
        </div>

        <div class="sidebar-section">
            <span class="sidebar-label">MAIN MENU</span>

            <nav class="sidebar-nav">
                <RouterLink
                    v-for="item in navigation"
                    :key="item.path"
                    :to="item.path"
                    class="nav-link"
                    active-class="active"
                    exact-active-class="active"
                >
                    <component :is="item.icon" :size="19" />
                    <span>{{ item.label }}</span>
                </RouterLink>
            </nav>
        </div>

        <div class="sidebar-bottom">
            <RouterLink to="/settings" class="nav-link">
                <Settings :size="19" />
                <span>Settings</span>
            </RouterLink>
        </div>
    </aside>
</template>
