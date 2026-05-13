import { useState, useEffect } from 'react'
import { booksApi } from '../services/api'
import BookDetailModal from './CountryModal'

export default function SavedBooks() {
  const [books, setBooks] = useState([])
  const [loading, setLoading] = useState(true)
  const [selectedBook, setSelectedBook] = useState(null)
  const [reportingBook, setReportingBook] = useState(null)
  const [faultyReason, setFaultyReason] = useState('')
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

  const showMessage = (msg) => {
    setMessage(msg)
    setTimeout(() => setMessage(''), 3000)
  }

  const handleReturn = async (bookId) => {
    try {
      await booksApi.returnBook(bookId)
      showMessage('Book returned successfully.')
      fetchBooks()
    } catch {
      showMessage('Could not return this book.')
    }
  }

  const handleSubmitFaultyReport = async () => {
    if (!faultyReason.trim()) return
    try {
      await booksApi.reportFaulty(reportingBook.bookId, faultyReason)
      showMessage(`Copy #${reportingBook.copyNumber} (ID: ${reportingBook.bookCopyId}) reported as faulty.`)
      setReportingBook(null)
      setFaultyReason('')
      fetchBooks()
    } catch {
      showMessage('Could not submit the faulty report.')
    }
  }

  const isOverdue = (dueDate) => new Date(dueDate) < new Date()

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
            <div key={book.id} className={`book-card saved ${isOverdue(book.dueDate) ? 'overdue' : ''}`}>
              <div className="book-id">Book #{book.bookId}</div>
              <h3 className="book-title">{book.title}</h3>
              <div className="book-meta">
                <span className="book-author">by {book.author}</span>
                <span className="book-genre">{book.genre}</span>
              </div>
              <div className="copy-tag">
                Copy {book.copyNumber} of 20 &middot; ID #{book.bookCopyId}
              </div>
              <div className="book-dates">
                <p className="book-date">Borrowed: {new Date(book.checkedOutAt).toLocaleDateString()}</p>
                <p className="book-date">
                  Due: {new Date(book.dueDate).toLocaleDateString()}
                  {isOverdue(book.dueDate) && <span className="overdue-badge">OVERDUE</span>}
                </p>
              </div>
              <div className="book-actions">
                <button className="btn-info" onClick={() => setSelectedBook(book)}>
                  Book Overview
                </button>
                <button className="btn-return" onClick={() => handleReturn(book.bookId)}>
                  Return Book
                </button>
                <button className="btn-report-faulty" onClick={() => setReportingBook(book)}>
                  Report This Copy as Faulty
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      {selectedBook && (
        <BookDetailModal book={selectedBook} onClose={() => setSelectedBook(null)} />
      )}

      {reportingBook && (
        <div className="modal-overlay" onClick={() => setReportingBook(null)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <button className="modal-close" onClick={() => setReportingBook(null)}>&#x2715;</button>
            <div className="modal-header">
              <h2>Report Copy as Faulty</h2>
              <span className="modal-id">
                {reportingBook.title} &mdash; Copy {reportingBook.copyNumber} (ID #{reportingBook.bookCopyId})
              </span>
            </div>
            <div className="modal-body">
              <p style={{ marginBottom: '1rem' }}>
                Describe the damage so the librarian can review this specific copy:
              </p>
              <textarea
                className="faulty-reason-input"
                placeholder="e.g. Pages torn, water damage, missing chapters, broken spine..."
                value={faultyReason}
                onChange={(e) => setFaultyReason(e.target.value)}
                rows={3}
                autoFocus
              />
              <button
                className="btn-checkout"
                style={{ marginTop: '1rem', background: '#e94560', width: '100%' }}
                onClick={handleSubmitFaultyReport}
                disabled={!faultyReason.trim()}
              >
                Submit Faulty Report
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
