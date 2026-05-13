export default function BookCard({ book, onCheckout }) {
  const unavailable = book.availableCopies === 0

  return (
    <div className={`book-card ${unavailable ? 'unavailable' : ''}`}>
      <div className="book-id">Book #{book.id}</div>
      <h3 className="book-title">{book.title}</h3>
      <div className="book-meta">
        <span className="book-author">by {book.author}</span>
        <span className="book-genre">{book.genre}</span>
      </div>
      <div className={`book-copies ${unavailable ? 'copies-empty' : ''}`}>
        {unavailable
          ? 'All copies checked out'
          : `${book.availableCopies} / ${book.totalCopies} copies available`}
      </div>
      <button
        className="btn-checkout"
        onClick={() => onCheckout(book.id)}
        disabled={unavailable}
      >
        {unavailable ? 'Unavailable' : 'Borrow Book'}
      </button>
    </div>
  )
}
