import api from './api'

export interface Branch {
    id: number
    name: string
    address: string | null
    workOrderCount: number
    incidentCount: number
}

export interface CreateBranchRequest {
    name: string
    address: string | null
}

export const branchService = {
    getAll() {
        return api.get<Branch[]>('/branches')
    },

    create(data: CreateBranchRequest) {
        return api.post<Branch>('/branches', data)
    }
}