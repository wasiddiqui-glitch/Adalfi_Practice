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
  getMyBooks: () => api.get('/books/my-books'),
  checkout: (id) => api.post(`/books/${id}/checkout`),
  returnBook: (id) => api.post(`/books/${id}/return`),
}
