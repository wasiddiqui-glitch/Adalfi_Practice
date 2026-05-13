import { useState, useEffect } from 'react'
import { booksApi } from '../services/api'
import BookCard from './BookCard'

export default function BookList() {
  const [books, setBooks] = useState([])
  const [loading, setLoading] = useState(true)
  const [message, setMessage] = useState('')
  const [query, setQuery] = useState('')

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
      setMessage('No copies available or you already have this book.')
      setTimeout(() => setMessage(''), 3000)
    }
  }

  if (loading) return <div className="loading">Loading books...</div>

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
                  <BookCard key={book.id} book={book} onCheckout={handleCheckout} />
                ))}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
