import { useState, useEffect } from 'react'
import { booksApi, adminApi } from '../services/api'
import { useAuth } from '../context/AuthContext'

export default function FaultyBooks() {
  const { user } = useAuth()
  const [books, setBooks] = useState([])
  const [loading, setLoading] = useState(true)
  const [message, setMessage] = useState('')

  const fetchBooks = async () => {
    setLoading(true)
    try {
      const { data } = await booksApi.getFaulty()
      setBooks(data)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { fetchBooks() }, [])

  const handleRestore = async (copyId, bookTitle, copyNumber) => {
    try {
      await adminApi.restoreCopy(copyId)
      setMessage(`Copy ${copyNumber} (ID #${copyId}) of "${bookTitle}" restored to circulation.`)
      fetchBooks()
      setTimeout(() => setMessage(''), 3000)
    } catch {
      setMessage('Failed to restore copy.')
      setTimeout(() => setMessage(''), 3000)
    }
  }

  if (loading) return <div className="loading">Loading faulty books...</div>

  const totalFaultyCopies = books.reduce((sum, b) => sum + b.faultyCopies.length, 0)

  return (
    <div className="page">
      <h2>Faulty / Damaged Books ({totalFaultyCopies} copies)</h2>
      {message && <div className="toast">{message}</div>}
      {books.length === 0 ? (
        <p className="empty">No faulty books — all copies are in good condition!</p>
      ) : (
        <div className="faulty-books-list">
          {books.map((book) => (
            <div key={book.bookId} className="faulty-book-group">
              <div className="faulty-book-header">
                <div>
                  <strong>{book.title}</strong>
                  <span className="book-author-inline"> by {book.author}</span>
                </div>
                <span className="book-genre">{book.genre}</span>
              </div>
              <div className="faulty-copies-grid">
                {book.faultyCopies.map((copy) => (
                  <div key={copy.copyId} className="faulty-copy-card">
                    <div className="copy-id-label">Copy #{copy.copyNumber} &middot; ID {copy.copyId}</div>
                    <div className="faulty-reason">
                      <strong>Reason:</strong> {copy.faultyReason || 'Not specified'}
                    </div>
                    {user.isAdmin ? (
                      <button
                        className="btn-restore"
                        onClick={() => handleRestore(copy.copyId, book.title, copy.copyNumber)}
                      >
                        Not Faulty — Restore
                      </button>
                    ) : (
                      <div className="under-review-label">Pending admin review</div>
                    )}
                  </div>
                ))}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
