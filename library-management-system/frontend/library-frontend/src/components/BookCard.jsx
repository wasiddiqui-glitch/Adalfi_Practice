export default function BookCard({ book, onCheckout }) {
  return (
    <div className="book-card">
      <div className="book-id">Book #{book.id}</div>
      <h3 className="book-title">{book.title}</h3>
      <button className="btn-checkout" onClick={() => onCheckout(book.id)}>
        Borrow Book
      </button>
    </div>
  )
}
