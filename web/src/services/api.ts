import axios from 'axios'

const api = axios.create({
    baseURL: import.meta.env.VITE_API_URL,
    timeout: 15000,
    headers: {
        'Content-Type': 'application/json'
    }
})

export function getApiErrorMessage(err: unknown, fallback: string): string {
    if (!axios.isAxiosError(err)) return fallback

    if (err.code === 'ECONNABORTED') {
        return 'Request timed out. Please check your connection and try again.'
    }

    if (!err.response) {
        return 'Unable to reach the server. Please verify the API is running.'
    }

    const data = err.response.data as {
        error?: string
        title?: string
        errors?: Record<string, string[]>
    } | undefined

    if (data?.error) return data.error

    if (data?.errors) {
        const messages = Object.values(data.errors).flat()
        if (messages.length > 0) return messages.join(' ')
    }

    if (data?.title) return data.title

    return fallback
}

export default api