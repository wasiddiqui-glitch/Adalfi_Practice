import { useState, useEffect } from 'react'
import { booksApi } from '../services/api'
import CountryModal from './CountryModal'

export default function SavedBooks() {
  const [books, setBooks] = useState([])
  const [loading, setLoading] = useState(true)
  const [selectedBook, setSelectedBook] = useState(null)
  const [message, setMessage] = useState('')

  const fetchBooks = async () => {
    try {
      const { data } = await booksApi.getMyBooks()
      setBooks(data)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { fetchBooks() }, [])

  const handleReturn = async (bookId) => {
    try {
      await booksApi.returnBook(bookId)
      setMessage('Book returned successfully.')
      fetchBooks()
      setTimeout(() => setMessage(''), 3000)
    } catch {
      setMessage('Could not return this book.')
      setTimeout(() => setMessage(''), 3000)
    }
  }

  if (loading) return <div className="loading">Loading your books...</div>

  return (
    <div className="page">
      <h2>My Borrowed Books ({books.length})</h2>
      {message && <div className="toast">{message}</div>}
      {books.length === 0 ? (
        <p className="empty">You haven&apos;t borrowed any books yet. Browse the library to get started!</p>
      ) : (
        <div className="book-grid">
          {books.map((book) => (
            <div key={book.id} className="book-card saved">
              <div className="book-id">Book #{book.bookId}</div>
              <h3 className="book-title">{book.title}</h3>
              <p className="book-date">
                Borrowed: {new Date(book.checkedOutAt).toLocaleDateString()}
              </p>
              <div className="book-actions">
                <button className="btn-info" onClick={() => setSelectedBook(book)}>
                  Country Overview
                </button>
                <button className="btn-return" onClick={() => handleReturn(book.bookId)}>
                  Return Book
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
      {selectedBook && (
        <CountryModal book={selectedBook} onClose={() => setSelectedBook(null)} />
      )}
    </div>
  )
}
