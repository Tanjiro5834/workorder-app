import api from './api'
import type { WorkOrder } from './workOrderService'
import type { IncidentReport } from './incidentReportService'

export interface StatusCount {
    label: string
    count: number
}

export interface BranchBreakdown {
    branchName: string
    workOrders: number
    incidents: number
}

export interface DashboardSummary {
    totalWorkOrders: number
    openWorkOrders: number
    inProgressWorkOrders: number
    completedWorkOrders: number
    totalIncidentReports: number
    openIncidentReports: number
    workOrdersByPriority: StatusCount[]
    incidentsBySeverity: StatusCount[]
    branchBreakdown: BranchBreakdown[]
    recentWorkOrders: WorkOrder[]
    recentIncidentReports: IncidentReport[]
}

export const dashboardService = {
    getSummary() {
        return api.get<DashboardSummary>('/dashboard')
    }
}