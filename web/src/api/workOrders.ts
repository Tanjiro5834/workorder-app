export type WorkOrderStatus = 'Open' | 'InProgress' | 'Done'
export type Priority = 'Low' | 'Medium' | 'High'

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

export interface PagedResult<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
}

export interface CreateWorkOrder {
  title: string
  description?: string
  priority: Priority
  branchId: number
}

export interface Branch {
  id: number
  name: string
}

export const branchesApi = {
  list() {
    return request<Branch[]>('/api/branches')
  },
}

export class ApiError extends Error {
  status: number
  fieldErrors: Record<string, string[]>

  constructor(message: string, status: number, fieldErrors: Record<string, string[]> = {}) {
    super(message)
    this.status = status
    this.fieldErrors = fieldErrors
  }
}

function normalizeErrors(errors: Record<string, string[]> | undefined) {
  const result: Record<string, string[]> = {}
  for (const [key, messages] of Object.entries(errors ?? {})) {
    const field = key.charAt(0).toLowerCase() + key.slice(1)
    result[field] = messages
  }
  return result
}

async function request<T>(url: string, init?: RequestInit): Promise<T> {
  const res = await fetch(url, {
    headers: { 'Content-Type': 'application/json' },
    ...init,
  })

  if (!res.ok) {
    const body = await res.json().catch(() => null)
    throw new ApiError(
      body?.error ?? body?.title ?? `Request failed (${res.status})`,
      res.status,
      normalizeErrors(body?.errors),
    )
  }

  return res.status === 204 ? (undefined as T) : res.json()
}

export const workOrdersApi = {
  list(status: WorkOrderStatus | '', page: number, pageSize = 10) {
    const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) })
    if (status) params.set('status', status)
    return request<PagedResult<WorkOrder>>(`/api/work-orders?${params}`)
  },
  create(data: CreateWorkOrder) {
    return request<WorkOrder>('/api/work-orders', { method: 'POST', body: JSON.stringify(data) })
  },
  updateStatus(id: number, status: WorkOrderStatus) {
    return request<void>(`/api/work-orders/${id}/status`, {
      method: 'PATCH',
      body: JSON.stringify({ status }),
    })
  },
  remove(id: number) {
    return request<void>(`/api/work-orders/${id}`, { method: 'DELETE' })
  },
}

