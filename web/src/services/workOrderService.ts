import api from './api'

export type Priority = 'Low' | 'Medium' | 'High'

export type WorkOrderStatus = 'Open' | 'InProgress' | 'Done'

export interface WorkOrder {
    id: number
    title: string
    description: string | null
    status: WorkOrderStatus
    priority: Priority
    branchId: number
    branchName: string
    createdAt: string
}

export interface CreateWorkOrderRequest {
    title: string
    description: string | null
    priority: Priority
    branchId: number
}

export interface UpdateWorkOrderRequest {
    title: string
    description: string | null
    priority: Priority
    branchId: number
}

export interface WorkOrderQueryParams {
    page?: number
    pageSize?: number
    status?: WorkOrderStatus | ''
    priority?: Priority | ''
    search?: string
}

export interface UpdateStatusRequest {
    status: WorkOrderStatus
}

export interface PagedResult<T> {
    items: T[]
    total: number
    page: number
    pageSize: number
}

export const workOrderService = {
    getAll(params: WorkOrderQueryParams = {}) {
        const {page = 1, pageSize = 10, status, priority, search} = params
        return api.get<PagedResult<WorkOrder>>('/work-orders', {
            params: {
                page,
                pageSize,
                status: status ?? undefined,
                priority: priority ?? undefined,
                search: search?.trim() ?? undefined
            }
        })
    },

    getById(id: number) {
        return api.get<WorkOrder>(`/work-orders/${id}`)
    },

    create(data: CreateWorkOrderRequest) {
        return api.post<WorkOrder>('/work-orders', data)
    },

    update(id: number, data: UpdateWorkOrderRequest) {
        return api.put<WorkOrder>(`/work-orders/${id}`, data)
    },

    updateStatus(id: number, status: WorkOrderStatus) {
        const payload: UpdateStatusRequest = { status }
        return api.patch<void>(`/work-orders/${id}/status`, payload)
    },

    delete(id: number) {
        return api.delete(`/work-orders/${id}`)
    }
}