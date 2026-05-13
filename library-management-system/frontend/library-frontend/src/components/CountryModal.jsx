export default function CountryModal({ book, onClose }) {
  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal" onClick={(e) => e.stopPropagation()}>
        <button className="modal-close" onClick={onClose} aria-label="Close">&#x2715;</button>
        <div className="modal-header">
          <h2>{book.title}</h2>
          <span className="modal-id">Book #{book.bookId}</span>
        </div>
        <p className="modal-body">{book.countryOverview}</p>
      </div>
    </div>
  )
}
