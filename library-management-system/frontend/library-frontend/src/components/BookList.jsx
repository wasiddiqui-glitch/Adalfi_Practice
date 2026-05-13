import { useState, useEffect } from 'react'
import { booksApi } from '../services/api'
import BookCard from './BookCard'

export default function BookList() {
  const [books, setBooks] = useState([])
  const [loading, setLoading] = useState(true)
  const [message, setMessage] = useState('')

  const fetchBooks = async () => {
    try {
      const { data } = await booksApi.getAvailable()
      setBooks(data)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { fetchBooks() }, [])

  const handleCheckout = async (bookId) => {
    try {
      await booksApi.checkout(bookId)
      setMessage('Book borrowed! Check your saved books.')
      fetchBooks()
      setTimeout(() => setMessage(''), 3000)
    } catch {
      setMessage('Could not borrow this book.')
      setTimeout(() => setMessage(''), 3000)
    }
  }

  if (loading) return <div className="loading">Loading books...</div>

  return (
    <div className="page">
      <h2>Available Books ({books.length})</h2>
      {message && <div className="toast">{message}</div>}
      {books.length === 0 ? (
        <p className="empty">All books have been borrowed. Check back later!</p>
      ) : (
        <div className="book-grid">
          {books.map((book) => (
            <BookCard key={book.id} book={book} onCheckout={handleCheckout} />
          ))}
        </div>
      )}
    </div>
  )
}
