
import api from './api'

export interface Branch {
    id: number
    name: string
}

export const branchService = {
    getAll() {
        return api.get<Branch[]>('/branches')
    }
}
