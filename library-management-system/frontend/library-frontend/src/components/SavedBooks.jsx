import { useState, useEffect } from 'react'
import { booksApi } from '../services/api'
import BookDetailModal from './CountryModal'

export default function SavedBooks() {
  const [view, setView] = useState('current')
  const [books, setBooks] = useState([])
  const [history, setHistory] = useState([])
  const [loading, setLoading] = useState(true)
  const [historyLoading, setHistoryLoading] = useState(false)
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

  const fetchHistory = async () => {
    setHistoryLoading(true)
    try {
      const { data } = await booksApi.getHistory()
      setHistory(data)
    } finally {
      setHistoryLoading(false)
    }
  }

  useEffect(() => { fetchBooks() }, [])

  useEffect(() => {
    if (view === 'history' && history.length === 0) {
      fetchHistory()
    }
  }, [view])

  const showMessage = (msg) => {
    setMessage(msg)
    setTimeout(() => setMessage(''), 3000)
  }

  const handleReturn = async (bookId) => {
    try {
      await booksApi.returnBook(bookId)
      showMessage('Book returned successfully.')
      fetchBooks()
      if (view === 'history') fetchHistory()
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

  if (loading) return <div className="loading">Loading your books...</div>

  return (
    <div className="page">
      <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', marginBottom: '1.5rem', flexWrap: 'wrap' }}>
        <h2 style={{ margin: 0 }}>My Books</h2>
        <div className="tab-buttons" style={{ marginBottom: 0 }}>
          <button
            className={`tab-btn ${view === 'current' ? 'active' : ''}`}
            onClick={() => setView('current')}
          >
            Currently Borrowed ({books.length})
          </button>
          <button
            className={`tab-btn ${view === 'history' ? 'active' : ''}`}
            onClick={() => setView('history')}
          >
            Return History
          </button>
        </div>
      </div>

      {message && <div className="toast">{message}</div>}

      {view === 'current' && (
        books.length === 0 ? (
          <p className="empty">You haven&apos;t borrowed any books yet. Browse the library to get started!</p>
        ) : (
          <div className="book-grid">
            {books.map((book) => (
              <div key={book.id} className={`book-card saved ${book.isOverdue ? 'overdue' : ''}`}>
                <div className="book-id">Book #{book.bookId}</div>
                <h3 className="book-title">{book.title}</h3>
                <div className="book-meta">
                  <span className="book-author">by {book.author}</span>
                  <span className="book-genre">{book.genre}</span>
                </div>
                <div className="copy-tag">
                  Copy {book.copyNumber} &middot; ID #{book.bookCopyId}
                </div>
                <div className="book-dates">
                  <p className="book-date">Borrowed: {new Date(book.checkedOutAt).toLocaleDateString()}</p>
                  <p className="book-date">
                    Due: {new Date(book.dueDate).toLocaleDateString()}
                    {book.isOverdue && <span className="overdue-badge">OVERDUE</span>}
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
        )
      )}

      {view === 'history' && (
        historyLoading ? (
          <div className="loading">Loading history...</div>
        ) : history.length === 0 ? (
          <p className="empty">No return history yet.</p>
        ) : (
          <div className="book-grid">
            {history.map((book) => {
              const returnedLate = new Date(book.returnedAt) > new Date(book.dueDate)
              return (
                <div key={book.id} className="book-card saved">
                  <div className="book-id">Book #{book.bookId}</div>
                  <h3 className="book-title">{book.title}</h3>
                  <div className="book-meta">
                    <span className="book-author">by {book.author}</span>
                    <span className="book-genre">{book.genre}</span>
                  </div>
                  <div className="copy-tag">
                    Copy {book.copyNumber} &middot; ID #{book.bookCopyId}
                  </div>
                  <div className="book-dates">
                    <p className="book-date">Borrowed: {new Date(book.checkedOutAt).toLocaleDateString()}</p>
                    <p className="book-date">Due: {new Date(book.dueDate).toLocaleDateString()}</p>
                    <p className="book-date">
                      Returned: {new Date(book.returnedAt).toLocaleDateString()}
                      {returnedLate && <span className="overdue-badge">RETURNED LATE</span>}
                    </p>
                  </div>
                  <div className="book-actions">
                    <button className="btn-info" onClick={() => setSelectedBook(book)}>
                      Book Overview
                    </button>
                  </div>
                </div>
              )
            })}
          </div>
        )
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
