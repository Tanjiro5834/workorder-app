import { useEffect, useState, type FormEvent } from 'react'
import { ApiError, branchesApi, workOrdersApi, type Branch, type Priority } from '../api/workOrders'

const PRIORITIES: Priority[] = ['Low', 'Medium', 'High']

interface Props {
  onCreated: () => void
}

export default function CreateWorkOrderForm({ onCreated }: Props) {
  const [branches, setBranches] = useState<Branch[]>([])
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [priority, setPriority] = useState<Priority>('Medium')
  const [branchId, setBranchId] = useState(0)
  const [errors, setErrors] = useState<Record<string, string[]>>({})
  const [formError, setFormError] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)

  useEffect(() => {
    branchesApi
      .list()
      .then(list => {
        setBranches(list)
        if (list.length > 0) setBranchId(list[0].id)
      })
      .catch(() => setFormError('Could not load branches.'))
  }, [])

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    setErrors({})
    setFormError(null)

    if (!title.trim()) {
      setErrors({ title: ['Title is required.'] })
      return
    }

    setSubmitting(true)
    try {
      await workOrdersApi.create({
        title: title.trim(),
        description: description.trim() || undefined,
        priority,
        branchId,
      })
      setTitle('')
      setDescription('')
      setPriority('Medium')
      onCreated()
    } catch (err) {
      if (err instanceof ApiError && Object.keys(err.fieldErrors).length > 0) {
        setErrors(err.fieldErrors)
      } else {
        setFormError((err as Error).message)
      }
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <form className="card form" onSubmit={handleSubmit} noValidate>
      <h2>New work order</h2>

      {formError && <p className="error">{formError}</p>}

      <label>
        Title
        <input value={title} maxLength={150} onChange={e => setTitle(e.target.value)} />
        {errors.title && <span className="field-error">{errors.title[0]}</span>}
      </label>

      <label>
        Description
        <textarea
          value={description}
          maxLength={1000}
          rows={2}
          onChange={e => setDescription(e.target.value)}
        />
        {errors.description && <span className="field-error">{errors.description[0]}</span>}
      </label>

      <div className="row">
        <label>
          Priority
          <select value={priority} onChange={e => setPriority(e.target.value as Priority)}>
            {PRIORITIES.map(p => (
              <option key={p} value={p}>{p}</option>
            ))}
          </select>
        </label>

        <label>
          Branch
          <select value={branchId} onChange={e => setBranchId(Number(e.target.value))}>
            {branches.map(b => (
              <option key={b.id} value={b.id}>{b.name}</option>
            ))}
          </select>
          {errors.branchId && <span className="field-error">{errors.branchId[0]}</span>}
        </label>
      </div>

      <button type="submit" className="primary" disabled={submitting || branches.length === 0}>
        {submitting ? 'Saving…' : 'Create'}
      </button>
    </form>
  )
}