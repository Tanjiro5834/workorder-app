
import { createRouter, createWebHistory } from 'vue-router'

import DashboardLayout from '../layouts/DashboardLayout.vue'
import DashboardView from '../views/DashboardView.vue'
import WorkOrdersView from '../views/WorkOrdersView.vue'
import IncidentReportsView from '../views/IncidentReportsView.vue'
import BranchesView from '../views/BranchesView.vue'

const router = createRouter({
    history: createWebHistory(),
    routes: [
        {
            path: '/',
            component: DashboardLayout,
            children: [
                {
                    path: '',
                    name: 'dashboard',
                    component: DashboardView,
                    meta: { title: 'Dashboard' }
                },
                {
                    path: 'work-orders',
                    name: 'work-orders',
                    component: WorkOrdersView,
                    meta: { title: 'Work Orders' }
                },
                {
                    path: 'incidents',
                    name: 'incidents',
                    component: IncidentReportsView,
                    meta: { title: 'Incident Reports' }
                },
                {
                    path: 'branches',
                    name: 'branches',
                    component: BranchesView,
                    meta: { title: 'Branches' }
                },
                {
                    path: 'settings',
                    name: 'settings',
                    component: () => import('../views/SettingsView.vue'),
                    meta: { title: 'Settings' }
                }
            ]
        }
    ]
})

export default router
