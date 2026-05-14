export default function BookCard({ book, onCheckout, onReserve, onCancelReservation, reservation }) {
  const physicalAvailable = book.availableCopies
  const pending = book.pendingReservations || 0
  const effectivelyAvailable = Math.max(0, physicalAvailable - pending)

  const hasReservation = !!reservation
  const canBorrowDirectly = !hasReservation && effectivelyAvailable > 0
  const canClaim = hasReservation && reservation.canCheckout

  const copiesLabel = physicalAvailable === 0
    ? 'All copies checked out'
    : `${physicalAvailable} / ${book.totalCopies} copies available`

  const isUnavailableStyle = !canBorrowDirectly && !canClaim && !hasReservation

  return (
    <div className={`book-card ${isUnavailableStyle ? 'unavailable' : ''}`}>
      <div className="book-id">Book #{book.id}</div>
      <h3 className="book-title">{book.title}</h3>
      <div className="book-meta">
        <span className="book-author">by {book.author}</span>
        <span className="book-genre">{book.genre}</span>
      </div>
      <div className={`book-copies ${physicalAvailable === 0 ? 'copies-empty' : ''}`}>
        {copiesLabel}
      </div>
      {pending > 0 && (
        <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)', marginBottom: '0.5rem' }}>
          {pending} {pending === 1 ? 'person' : 'people'} in queue
        </div>
      )}

      {hasReservation ? (
        <div>
          <div style={{ fontSize: '0.8rem', color: canClaim ? '#4caf50' : 'var(--text-muted)', marginBottom: '0.5rem', fontWeight: 600 }}>
            Queue position #{reservation.queuePosition}
            {canClaim && ' — Ready to claim!'}
          </div>
          <div style={{ display: 'flex', flexDirection: 'column', gap: '0.4rem' }}>
            {canClaim && (
              <button className="btn-checkout" onClick={() => onCheckout(book.id)}>
                Claim Your Copy
              </button>
            )}
            <button
              className="btn-return"
              style={{ fontSize: '0.8rem' }}
              onClick={() => onCancelReservation(reservation.id)}
            >
              Cancel Hold
            </button>
          </div>
        </div>
      ) : canBorrowDirectly ? (
        <button className="btn-checkout" onClick={() => onCheckout(book.id)}>
          Borrow Book
        </button>
      ) : (
        <button className="btn-reserve" onClick={() => onReserve(book.id)}>
          {physicalAvailable > 0 ? 'Join Queue' : 'Join Waitlist'}
        </button>
      )}
    </div>
  )
}
