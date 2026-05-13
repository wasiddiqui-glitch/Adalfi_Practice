import axios from 'axios'

const api = axios.create({
  baseURL: 'http://localhost:5000/api',
})

api.interceptors.request.use((config) => {
  const stored = localStorage.getItem('lms_user')
  const user = stored ? JSON.parse(stored) : null
  if (user?.token) {
    config.headers.Authorization = `Bearer ${user.token}`
  }
  return config
})

export const authApi = {
  login: (data) => api.post('/auth/login', data),
  register: (data) => api.post('/auth/register', data),
}

export const booksApi = {
  getAvailable: () => api.get('/books'),
  getFaulty: () => api.get('/books/faulty'),
  getMyBooks: () => api.get('/books/my-books'),
  checkout: (id) => api.post(`/books/${id}/checkout`),
  returnBook: (id) => api.post(`/books/${id}/return`),
  reportFaulty: (id, reason) => api.post(`/books/${id}/report-faulty`, { reason }),
}

export const adminApi = {
  getUsers: () => api.get('/admin/users'),
  getCheckouts: () => api.get('/admin/checkouts'),
  markCopyFaulty: (copyId, reason) => api.post(`/admin/copies/${copyId}/mark-faulty`, { reason }),
  restoreCopy: (copyId) => api.post(`/admin/copies/${copyId}/restore`),
}
