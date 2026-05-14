import { useState, useEffect } from 'react'
import { reservationsApi, booksApi } from '../services/api'

export default function Reservations() {
  const [reservations, setReservations] = useState([])
  const [loading, setLoading] = useState(true)
  const [message, setMessage] = useState('')

  const fetchReservations = async () => {
    setLoading(true)
    try {
      const { data } = await reservationsApi.getMyReservations()
      setReservations(data)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { fetchReservations() }, [])

  const showMessage = (msg) => {
    setMessage(msg)
    setTimeout(() => setMessage(''), 3000)
  }

  const handleClaim = async (bookId) => {
    try {
      await booksApi.checkout(bookId)
      showMessage('Book borrowed! Check your saved books.')
      fetchReservations()
    } catch (e) {
      showMessage(e.response?.data?.message || 'Could not claim this book.')
    }
  }

  const handleCancel = async (id) => {
    try {
      await reservationsApi.cancel(id)
      showMessage('Reservation cancelled.')
      fetchReservations()
    } catch {
      showMessage('Could not cancel reservation.')
    }
  }

  if (loading) return <div className="loading">Loading reservations...</div>

  return (
    <div className="page">
      <h2>My Waitlist ({reservations.length})</h2>
      {message && <div className="toast">{message}</div>}
      {reservations.length === 0 ? (
        <p className="empty">
          You have no active reservations. When a book you want is unavailable, join its waitlist from the Browse page.
        </p>
      ) : (
        <div className="book-grid">
          {reservations.map((r) => (
            <div key={r.id} className={`book-card saved ${r.canCheckout ? 'ready' : ''}`}>
              <div className="book-id">Book #{r.bookId}</div>
              <h3 className="book-title">{r.bookTitle}</h3>
              <div className="book-meta">
                <span className="book-author">by {r.bookAuthor}</span>
                <span className="book-genre">{r.bookGenre}</span>
              </div>
              <div className="book-dates">
                <p className="book-date">Reserved: {new Date(r.reservedAt).toLocaleDateString()}</p>
              </div>
              <div style={{ marginBottom: '0.75rem' }}>
                {r.canCheckout ? (
                  <span style={{ color: '#4caf50', fontWeight: 700, fontSize: '0.875rem' }}>
                    A copy is ready for you!
                  </span>
                ) : (
                  <span style={{ color: 'var(--text-muted)', fontSize: '0.875rem' }}>
                    Queue position #{r.queuePosition}
                  </span>
                )}
              </div>
              <div className="book-actions">
                {r.canCheckout && (
                  <button className="btn-checkout" onClick={() => handleClaim(r.bookId)}>
                    Claim Your Copy
                  </button>
                )}
                <button className="btn-return" onClick={() => handleCancel(r.id)}>
                  Cancel Hold
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
