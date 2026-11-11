import { useCallback, useEffect, useState } from 'react'
import { workOrdersApi, type PagedResult, type WorkOrder, type WorkOrderStatus } from './api/workOrders'

const STATUSES: WorkOrderStatus[] = ['Open', 'InProgress', 'Done']

function formatDate(iso: string) {
  return new Date(iso).toLocaleString('en-PH', { timeZone: 'Asia/Manila' })
}

export default function App() {
  const [status, setStatus] = useState<WorkOrderStatus | ''>('')
  const [page, setPage] = useState(1)
  const [data, setData] = useState<PagedResult<WorkOrder> | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      setData(await workOrdersApi.list(status, page))
    } catch (e) {
      setError((e as Error).message)
    } finally {
      setLoading(false)
    }
  }, [status, page])

  useEffect(() => {
    load()
  }, [load])

  async function changeStatus(id: number, next: WorkOrderStatus) {
    try {
      await workOrdersApi.updateStatus(id, next)
      await load()
    } catch (e) {
      setError((e as Error).message)
    }
  }

  async function remove(id: number) {
    if (!window.confirm('Delete this work order?')) return
    try {
      await workOrdersApi.remove(id)
      if (data && data.items.length === 1 && page > 1) setPage(p => p - 1)
      else await load()
    } catch (e) {
      setError((e as Error).message)
    }
  }

  const totalPages = data ? Math.max(1, Math.ceil(data.total / data.pageSize)) : 1

  return (
    <main className="container">
      <header className="header">
        <h1>Work Orders</h1>
        <select
          value={status}
          onChange={e => {
            setStatus(e.target.value as WorkOrderStatus | '')
            setPage(1)
          }}
        >
          <option value="">All statuses</option>
          {STATUSES.map(s => (
            <option key={s} value={s}>{s}</option>
          ))}
        </select>
      </header>

      {error && <p className="error">{error}</p>}

      {loading ? (
        <p>Loading…</p>
      ) : data && data.items.length > 0 ? (
        <table>
          <thead>
            <tr>
              <th>Title</th>
              <th>Branch</th>
              <th>Priority</th>
              <th>Status</th>
              <th>Created</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {data.items.map(w => (
              <tr key={w.id}>
                <td>
                  <strong>{w.title}</strong>
                  {w.description && <div className="muted">{w.description}</div>}
                </td>
                <td>{w.branchName}</td>
                <td>
                  <span className={`badge ${w.priority.toLowerCase()}`}>{w.priority}</span>
                </td>
                <td>
                  <select
                    value={w.status}
                    onChange={e => changeStatus(w.id, e.target.value as WorkOrderStatus)}
                  >
                    {STATUSES.map(s => (
                      <option key={s} value={s}>{s}</option>
                    ))}
                  </select>
                </td>
                <td className="muted">{formatDate(w.createdAt)}</td>
                <td>
                  <button className="danger" onClick={() => remove(w.id)}>Delete</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      ) : (
        <p className="muted">No work orders found.</p>
      )}

      <footer className="pager">
        <button disabled={page <= 1} onClick={() => setPage(p => p - 1)}>Prev</button>
        <span>Page {page} of {totalPages}</span>
        <button disabled={page >= totalPages} onClick={() => setPage(p => p + 1)}>Next</button>
      </footer>
    </main>
  )
}