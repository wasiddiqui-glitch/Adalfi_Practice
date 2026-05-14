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
  getHistory: () => api.get('/books/history'),
  checkout: (id) => api.post(`/books/${id}/checkout`),
  returnBook: (id) => api.post(`/books/${id}/return`),
  reportFaulty: (id, reason) => api.post(`/books/${id}/report-faulty`, { reason }),
}

export const reservationsApi = {
  getMyReservations: () => api.get('/reservations'),
  reserve: (bookId) => api.post(`/reservations/${bookId}`),
  cancel: (id) => api.delete(`/reservations/${id}`),
}

export const adminApi = {
  getUsers: () => api.get('/admin/users'),
  getCheckouts: () => api.get('/admin/checkouts'),
  getOverdue: () => api.get('/admin/checkouts/overdue'),
  markCopyFaulty: (copyId, reason) => api.post(`/admin/copies/${copyId}/mark-faulty`, { reason }),
  restoreCopy: (copyId) => api.post(`/admin/copies/${copyId}/restore`),
  getBookDetail: (id) => api.get(`/admin/books/${id}`),
  addBook: (data) => api.post('/admin/books', data),
  updateBook: (id, data) => api.put(`/admin/books/${id}`, data),
  deleteBook: (id) => api.delete(`/admin/books/${id}`),
  addCopy: (bookId) => api.post(`/admin/books/${bookId}/copies`),
  deleteCopy: (id) => api.delete(`/admin/copies/${id}`),
}
