
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

export interface PagedResult<T> {
    items: T[]
    total: number
    page: number
    pageSize: number
}

export const workOrderService = {
    getAll(page = 1, pageSize = 10) {
        return api.get<PagedResult<WorkOrder>>('/work-orders', {
            params: { page, pageSize }
        })
    },

    getById(id: number) {
        return api.get<WorkOrder>(`/work-orders/${id}`)
    },

    create(data: CreateWorkOrderRequest) {
        return api.post<WorkOrder>('/work-orders', data)
    },

    delete(id: number) {
        return api.delete(`/work-orders/${id}`)
    }
}
