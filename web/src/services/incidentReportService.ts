import api from './api'
import type { PagedResult } from './workOrderService'

export type IncidentSeverity = 'Low' | 'Medium' | 'High' | 'Critical'

export type IncidentStatus = 'Open' | 'Investigating' | 'Resolved'

export interface IncidentReport {
    id: number
    title: string
    description: string | null
    reportedBy: string | null
    status: IncidentStatus
    severity: IncidentSeverity
    branchId: number
    branchName: string
    createdAt: string
}

export interface CreateIncidentReportRequest {
    title: string
    description: string | null
    reportedBy: string | null
    severity: IncidentSeverity
    branchId: number
}

export interface UpdateIncidentReportRequest {
    title: string
    description: string | null
    reportedBy: string | null
    severity: IncidentSeverity
    branchId: number
}

export interface UpdateIncidentStatusRequest {
    status: IncidentStatus
}

export interface IncidentReportQueryParams {
    page?: number
    pageSize?: number
    status?: IncidentStatus | ''
    severity?: IncidentSeverity | ''
    search?: string
}

export const incidentReportService = {
    getAll(params: IncidentReportQueryParams = {}) {
        const { page = 1, pageSize = 10, status, severity, search } = params

        return api.get<PagedResult<IncidentReport>>('/incident-reports', {
            params: {
                page,
                pageSize,
                status: status || undefined,
                severity: severity || undefined,
                search: search?.trim() || undefined
            }
        })
    },

    getById(id: number) {
        return api.get<IncidentReport>(`/incident-reports/${id}`)
    },

    create(data: CreateIncidentReportRequest) {
        return api.post<IncidentReport>('/incident-reports', data)
    },

    update(id: number, data: UpdateIncidentReportRequest) {
        return api.put<IncidentReport>(`/incident-reports/${id}`, data)
    },

    updateStatus(id: number, status: IncidentStatus) {
        const payload: UpdateIncidentStatusRequest = { status }
        return api.patch<void>(`/incident-reports/${id}/status`, payload)
    },

    delete(id: number) {
        return api.delete(`/incident-reports/${id}`)
    }
}