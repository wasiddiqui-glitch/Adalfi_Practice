import { useState, useEffect } from 'react'
import { booksApi, reservationsApi } from '../services/api'
import BookCard from './BookCard'

export default function BookList() {
  const [books, setBooks] = useState([])
  const [reservations, setReservations] = useState([])
  const [loading, setLoading] = useState(true)
  const [message, setMessage] = useState('')
  const [query, setQuery] = useState('')

  const fetchAll = async () => {
    try {
      const [booksRes, resvRes] = await Promise.all([
        booksApi.getAvailable(),
        reservationsApi.getMyReservations(),
      ])
      setBooks(booksRes.data)
      setReservations(resvRes.data)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { fetchAll() }, [])

  const showMessage = (msg) => {
    setMessage(msg)
    setTimeout(() => setMessage(''), 3000)
  }

  const handleCheckout = async (bookId) => {
    try {
      await booksApi.checkout(bookId)
      showMessage('Book borrowed! Check your saved books.')
      fetchAll()
    } catch (e) {
      showMessage(e.response?.data?.message || 'Could not borrow this book.')
    }
  }

  const handleReserve = async (bookId) => {
    try {
      await reservationsApi.reserve(bookId)
      showMessage("You've been added to the waitlist.")
      fetchAll()
    } catch (e) {
      showMessage(e.response?.data?.message || 'Could not join the waitlist.')
    }
  }

  const handleCancelReservation = async (reservationId) => {
    try {
      await reservationsApi.cancel(reservationId)
      showMessage('Reservation cancelled.')
      fetchAll()
    } catch {
      showMessage('Could not cancel reservation.')
    }
  }

  if (loading) return <div className="loading">Loading books...</div>

  const reservationsByBookId = Object.fromEntries(reservations.map(r => [r.bookId, r]))

  const q = query.trim().toLowerCase()
  const filtered = q
    ? books.filter(b =>
        b.title.toLowerCase().includes(q) ||
        b.author.toLowerCase().includes(q) ||
        b.genre.toLowerCase().includes(q) ||
        String(b.id).includes(q)
      )
    : books

  const grouped = filtered.reduce((acc, book) => {
    const letter = book.title[0].toUpperCase()
    if (!acc[letter]) acc[letter] = []
    acc[letter].push(book)
    return acc
  }, {})

  const letters = Object.keys(grouped).sort()

  return (
    <div className="page">
      <h2>Library Collection ({books.length} titles)</h2>
      <div className="search-bar">
        <input
          type="text"
          className="search-input"
          placeholder="Search by title, author, genre or book ID..."
          value={query}
          onChange={(e) => setQuery(e.target.value)}
        />
        {query && (
          <button className="search-clear" onClick={() => setQuery('')}>&#x2715;</button>
        )}
      </div>
      {query && (
        <p className="search-results-label">
          {filtered.length === 0
            ? 'No books match your search.'
            : `${filtered.length} result${filtered.length !== 1 ? 's' : ''} for "${query}"`}
        </p>
      )}
      {message && <div className="toast">{message}</div>}
      {filtered.length === 0 && !query ? (
        <p className="empty">No books in the library.</p>
      ) : (
        <div className="letter-grid">
          {letters.map((letter) => (
            <div key={letter} className="letter-section">
              <div className="letter-heading">{letter}</div>
              <div className="book-grid">
                {grouped[letter].map((book) => (
                  <BookCard
                    key={book.id}
                    book={book}
                    onCheckout={handleCheckout}
                    onReserve={handleReserve}
                    onCancelReservation={handleCancelReservation}
                    reservation={reservationsByBookId[book.id] || null}
                  />
                ))}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
