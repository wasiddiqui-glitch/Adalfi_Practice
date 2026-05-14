import { useState, useEffect } from 'react'
import { adminApi, booksApi } from '../services/api'

const emptyBookForm = { title: '', author: '', genre: '', description: '', initialCopies: 5 }

export default function AdminPage() {
  const [tab, setTab] = useState('checkouts')
  const [checkouts, setCheckouts] = useState([])
  const [users, setUsers] = useState([])
  const [allBooks, setAllBooks] = useState([])
  const [loading, setLoading] = useState(true)
  const [message, setMessage] = useState('')
  const [overdueOnly, setOverdueOnly] = useState(false)

  // mark-faulty modal (existing)
  const [faultyModal, setFaultyModal] = useState(null)
  const [faultyReason, setFaultyReason] = useState('')

  // book form modal (add / edit)
  const [bookFormModal, setBookFormModal] = useState(null) // { mode: 'add'|'edit', book?: {} }
  const [bookForm, setBookForm] = useState(emptyBookForm)
  const [bookFormSaving, setBookFormSaving] = useState(false)

  // copies management modal
  const [copiesModal, setCopiesModal] = useState(null) // { bookId, bookTitle, copies: [] }
  const [copiesLoading, setCopiesLoading] = useState(false)

  // delete confirmation
  const [deleteBookModal, setDeleteBookModal] = useState(null) // { id, title }

  const fetchData = async () => {
    setLoading(true)
    try {
      const [checkoutsRes, usersRes, booksRes] = await Promise.all([
        adminApi.getCheckouts(),
        adminApi.getUsers(),
        booksApi.getAvailable(),
      ])
      setCheckouts(checkoutsRes.data)
      setUsers(usersRes.data)
      setAllBooks(booksRes.data)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { fetchData() }, [])

  const showMessage = (msg) => {
    setMessage(msg)
    setTimeout(() => setMessage(''), 3000)
  }

  // ── Checkouts ────────────────────────────────────────────────────────────────

  const displayedCheckouts = overdueOnly ? checkouts.filter(c => c.isOverdue) : checkouts
  const overdueCount = checkouts.filter(c => c.isOverdue).length

  // ── Book form ────────────────────────────────────────────────────────────────

  const openAddBookModal = () => {
    setBookForm(emptyBookForm)
    setBookFormModal({ mode: 'add' })
  }

  const openEditBookModal = (book) => {
    setBookForm({ title: book.title, author: book.author, genre: book.genre, description: book.description, initialCopies: 5 })
    setBookFormModal({ mode: 'edit', book })
  }

  const handleBookFormSubmit = async () => {
    if (!bookForm.title.trim() || !bookForm.author.trim()) return
    setBookFormSaving(true)
    try {
      if (bookFormModal.mode === 'add') {
        await adminApi.addBook({ ...bookForm, initialCopies: parseInt(bookForm.initialCopies) || 1 })
        showMessage('Book added successfully.')
      } else {
        await adminApi.updateBook(bookFormModal.book.id, {
          title: bookForm.title,
          author: bookForm.author,
          genre: bookForm.genre,
          description: bookForm.description,
        })
        showMessage('Book updated.')
      }
      setBookFormModal(null)
      fetchData()
    } catch (e) {
      showMessage(e.response?.data?.message || 'Failed to save book.')
    } finally {
      setBookFormSaving(false)
    }
  }

  // ── Delete book ───────────────────────────────────────────────────────────────

  const handleDeleteBook = async () => {
    try {
      await adminApi.deleteBook(deleteBookModal.id)
      showMessage('Book deleted.')
      setDeleteBookModal(null)
      fetchData()
    } catch (e) {
      showMessage(e.response?.data?.message || 'Cannot delete this book.')
      setDeleteBookModal(null)
    }
  }

  // ── Copies modal ─────────────────────────────────────────────────────────────

  const openCopiesModal = async (book) => {
    setCopiesModal({ bookId: book.id, bookTitle: book.title, copies: [] })
    setCopiesLoading(true)
    try {
      const { data } = await adminApi.getBookDetail(book.id)
      setCopiesModal({ bookId: data.id, bookTitle: data.title, copies: data.copies })
    } finally {
      setCopiesLoading(false)
    }
  }

  const handleAddCopy = async () => {
    try {
      const { data: newCopy } = await adminApi.addCopy(copiesModal.bookId)
      setCopiesModal(prev => ({ ...prev, copies: [...prev.copies, newCopy] }))
      fetchData()
    } catch {
      showMessage('Failed to add copy.')
    }
  }

  const handleDeleteCopy = async (copyId) => {
    try {
      await adminApi.deleteCopy(copyId)
      setCopiesModal(prev => ({ ...prev, copies: prev.copies.filter(c => c.id !== copyId) }))
      fetchData()
    } catch (e) {
      showMessage(e.response?.data?.message || 'Cannot delete this copy.')
    }
  }

  if (loading) return <div className="loading">Loading admin data...</div>

  const totalBorrowed = checkouts.length

  return (
    <div className="page admin-page">
      <h2>Admin Panel</h2>
      {message && <div className="toast">{message}</div>}

      <div className="admin-stats">
        <div className="stat-card">
          <div className="stat-number">{users.length}</div>
          <div className="stat-label">Registered Users</div>
        </div>
        <div className="stat-card">
          <div className="stat-number">{allBooks.length}</div>
          <div className="stat-label">Book Titles</div>
        </div>
        <div className="stat-card">
          <div className="stat-number">{allBooks.reduce((s, b) => s + b.totalCopies, 0)}</div>
          <div className="stat-label">Total Copies</div>
        </div>
        <div className="stat-card">
          <div className="stat-number">{totalBorrowed}</div>
          <div className="stat-label">Active Checkouts</div>
        </div>
        <div className="stat-card danger">
          <div className="stat-number">{overdueCount}</div>
          <div className="stat-label">Overdue</div>
        </div>
      </div>

      <div className="tab-buttons">
        <button className={`tab-btn ${tab === 'checkouts' ? 'active' : ''}`} onClick={() => setTab('checkouts')}>
          Active Checkouts ({checkouts.length})
        </button>
        <button className={`tab-btn ${tab === 'users' ? 'active' : ''}`} onClick={() => setTab('users')}>
          All Users ({users.length})
        </button>
        <button className={`tab-btn ${tab === 'books' ? 'active' : ''}`} onClick={() => setTab('books')}>
          Inventory ({allBooks.length} titles)
        </button>
      </div>

      {/* ── Checkouts tab ── */}
      {tab === 'checkouts' && (
        <div className="admin-table-wrap">
          <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', padding: '0.75rem 1rem', borderBottom: '1px solid rgba(255,255,255,0.06)' }}>
            <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', cursor: 'pointer', fontSize: '0.875rem', color: 'var(--text-muted)' }}>
              <input
                type="checkbox"
                checked={overdueOnly}
                onChange={(e) => setOverdueOnly(e.target.checked)}
                style={{ accentColor: '#e94560' }}
              />
              Show overdue only ({overdueCount})
            </label>
          </div>
          {displayedCheckouts.length === 0 ? (
            <p className="empty" style={{ padding: '1.5rem' }}>
              {overdueOnly ? 'No overdue checkouts.' : 'No active checkouts.'}
            </p>
          ) : (
            <table className="admin-table">
              <thead>
                <tr>
                  <th>#</th>
                  <th>User</th>
                  <th>Book Title</th>
                  <th>Copy #</th>
                  <th>Copy ID</th>
                  <th>Checked Out</th>
                  <th>Due Date</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {displayedCheckouts.map((c, i) => (
                  <tr key={c.userBookId} className={c.isOverdue ? 'row-overdue' : ''}>
                    <td>{i + 1}</td>
                    <td>{c.username}</td>
                    <td>{c.bookTitle}</td>
                    <td>Copy {c.copyNumber}</td>
                    <td className="copy-id-cell">#{c.bookCopyId}</td>
                    <td>{new Date(c.checkedOutAt).toLocaleDateString()}</td>
                    <td>{new Date(c.dueDate).toLocaleDateString()}</td>
                    <td>
                      {c.isOverdue
                        ? <span className="overdue-badge">OVERDUE</span>
                        : <span className="status-ok">On Time</span>}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      )}

      {/* ── Users tab ── */}
      {tab === 'users' && (
        <div className="admin-users">
          {users.map((u) => (
            <div key={u.id} className="admin-user-card">
              <div className="admin-user-header">
                <strong>{u.username}</strong>
                {u.isAdmin && <span className="admin-badge">Admin</span>}
                <span className="checkout-count">{u.currentCheckouts.length} books borrowed</span>
              </div>
              {u.currentCheckouts.length > 0 && (
                <ul className="user-checkout-list">
                  {u.currentCheckouts.map((c) => (
                    <li key={c.userBookId} className={c.isOverdue ? 'overdue-item' : ''}>
                      <span>{c.bookTitle}</span>
                      <span className="copy-tag-inline">Copy {c.copyNumber} · ID #{c.bookCopyId}</span>
                      <span className="checkout-due">Due: {new Date(c.dueDate).toLocaleDateString()}</span>
                      {c.isOverdue && <span className="overdue-badge">OVERDUE</span>}
                    </li>
                  ))}
                </ul>
              )}
            </div>
          ))}
        </div>
      )}

      {/* ── Books tab ── */}
      {tab === 'books' && (
        <div className="admin-table-wrap">
          <div style={{ display: 'flex', justifyContent: 'flex-end', padding: '0.75rem 1rem', borderBottom: '1px solid rgba(255,255,255,0.06)' }}>
            <button className="btn-checkout" onClick={openAddBookModal}>
              + Add Book
            </button>
          </div>
          <table className="admin-table">
            <thead>
              <tr>
                <th>Title</th>
                <th>Author</th>
                <th>Genre</th>
                <th>Available</th>
                <th>Total</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {allBooks.map((b) => (
                <tr key={b.id}>
                  <td>{b.title}</td>
                  <td>{b.author}</td>
                  <td>{b.genre}</td>
                  <td className={b.availableCopies === 0 ? 'copies-empty' : ''}>{b.availableCopies}</td>
                  <td>{b.totalCopies}</td>
                  <td>
                    <div style={{ display: 'flex', gap: '0.4rem', flexWrap: 'wrap' }}>
                      <button className="btn-info" style={{ fontSize: '0.75rem', padding: '0.25rem 0.6rem' }} onClick={() => openCopiesModal(b)}>
                        Copies
                      </button>
                      <button className="btn-info" style={{ fontSize: '0.75rem', padding: '0.25rem 0.6rem' }} onClick={() => openEditBookModal(b)}>
                        Edit
                      </button>
                      <button
                        className="btn-report-faulty"
                        style={{ fontSize: '0.75rem', padding: '0.25rem 0.6rem' }}
                        onClick={() => setDeleteBookModal({ id: b.id, title: b.title })}
                      >
                        Delete
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* ── Mark Faulty modal (existing) ── */}
      {faultyModal && (
        <div className="modal-overlay" onClick={() => setFaultyModal(null)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <button className="modal-close" onClick={() => setFaultyModal(null)}>&#x2715;</button>
            <div className="modal-header">
              <h2>Mark Copy as Faulty</h2>
              <span className="modal-id">{faultyModal.title} — Copy #{faultyModal.copyNumber} (ID {faultyModal.copyId})</span>
            </div>
            <div className="modal-body">
              <textarea
                className="faulty-reason-input"
                placeholder="Describe the damage..."
                value={faultyReason}
                onChange={(e) => setFaultyReason(e.target.value)}
                rows={3}
              />
              <button
                className="btn-checkout"
                style={{ marginTop: '1rem', background: '#e94560', width: '100%' }}
                onClick={async () => {
                  if (!faultyReason.trim()) return
                  await adminApi.markCopyFaulty(faultyModal.copyId, faultyReason)
                  showMessage(`Copy ${faultyModal.copyNumber} marked as faulty.`)
                  setFaultyModal(null)
                  setFaultyReason('')
                  fetchData()
                }}
                disabled={!faultyReason.trim()}
              >
                Confirm — Mark Faulty
              </button>
            </div>
          </div>
        </div>
      )}

      {/* ── Add / Edit Book modal ── */}
      {bookFormModal && (
        <div className="modal-overlay" onClick={() => setBookFormModal(null)}>
          <div className="modal" style={{ maxWidth: '520px' }} onClick={(e) => e.stopPropagation()}>
            <button className="modal-close" onClick={() => setBookFormModal(null)}>&#x2715;</button>
            <div className="modal-header">
              <h2>{bookFormModal.mode === 'add' ? 'Add New Book' : 'Edit Book'}</h2>
            </div>
            <div className="modal-body" style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
              <input
                className="faulty-reason-input"
                style={{ padding: '0.5rem 0.75rem' }}
                placeholder="Title *"
                value={bookForm.title}
                onChange={(e) => setBookForm(f => ({ ...f, title: e.target.value }))}
              />
              <input
                className="faulty-reason-input"
                style={{ padding: '0.5rem 0.75rem' }}
                placeholder="Author *"
                value={bookForm.author}
                onChange={(e) => setBookForm(f => ({ ...f, author: e.target.value }))}
              />
              <input
                className="faulty-reason-input"
                style={{ padding: '0.5rem 0.75rem' }}
                placeholder="Genre"
                value={bookForm.genre}
                onChange={(e) => setBookForm(f => ({ ...f, genre: e.target.value }))}
              />
              <textarea
                className="faulty-reason-input"
                placeholder="Description"
                value={bookForm.description}
                onChange={(e) => setBookForm(f => ({ ...f, description: e.target.value }))}
                rows={3}
              />
              {bookFormModal.mode === 'add' && (
                <input
                  className="faulty-reason-input"
                  style={{ padding: '0.5rem 0.75rem' }}
                  type="number"
                  min={1}
                  max={100}
                  placeholder="Initial number of copies"
                  value={bookForm.initialCopies}
                  onChange={(e) => setBookForm(f => ({ ...f, initialCopies: e.target.value }))}
                />
              )}
              <button
                className="btn-checkout"
                style={{ marginTop: '0.5rem' }}
                onClick={handleBookFormSubmit}
                disabled={bookFormSaving || !bookForm.title.trim() || !bookForm.author.trim()}
              >
                {bookFormSaving ? 'Saving...' : bookFormModal.mode === 'add' ? 'Add Book' : 'Save Changes'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* ── Copies management modal ── */}
      {copiesModal && (
        <div className="modal-overlay" onClick={() => setCopiesModal(null)}>
          <div className="modal" style={{ maxWidth: '600px', maxHeight: '80vh', display: 'flex', flexDirection: 'column' }} onClick={(e) => e.stopPropagation()}>
            <button className="modal-close" onClick={() => setCopiesModal(null)}>&#x2715;</button>
            <div className="modal-header">
              <h2>Manage Copies</h2>
              <span className="modal-id">{copiesModal.bookTitle}</span>
            </div>
            <div className="modal-body" style={{ overflowY: 'auto', flex: 1 }}>
              {copiesLoading ? (
                <p style={{ color: 'var(--text-muted)' }}>Loading copies...</p>
              ) : (
                <>
                  <table className="admin-table" style={{ marginBottom: '1rem' }}>
                    <thead>
                      <tr>
                        <th>Copy #</th>
                        <th>ID</th>
                        <th>Status</th>
                        <th>Action</th>
                      </tr>
                    </thead>
                    <tbody>
                      {copiesModal.copies.map((c) => (
                        <tr key={c.id}>
                          <td>{c.copyNumber}</td>
                          <td className="copy-id-cell">#{c.id}</td>
                          <td>
                            {c.isFaulty
                              ? <span className="overdue-badge">Faulty</span>
                              : c.isCheckedOut
                                ? <span style={{ color: '#f0a500', fontSize: '0.8rem', fontWeight: 600 }}>Checked Out</span>
                                : <span className="status-ok">Available</span>}
                          </td>
                          <td>
                            {c.isCheckedOut ? (
                              <span style={{ color: 'var(--text-muted)', fontSize: '0.75rem' }}>in use</span>
                            ) : (
                              <button
                                className="btn-report-faulty"
                                style={{ fontSize: '0.75rem', padding: '0.2rem 0.5rem' }}
                                onClick={() => handleDeleteCopy(c.id)}
                              >
                                Delete
                              </button>
                            )}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                  <button className="btn-checkout" onClick={handleAddCopy}>
                    + Add Copy
                  </button>
                </>
              )}
            </div>
          </div>
        </div>
      )}

      {/* ── Delete book confirmation ── */}
      {deleteBookModal && (
        <div className="modal-overlay" onClick={() => setDeleteBookModal(null)}>
          <div className="modal" style={{ maxWidth: '400px' }} onClick={(e) => e.stopPropagation()}>
            <button className="modal-close" onClick={() => setDeleteBookModal(null)}>&#x2715;</button>
            <div className="modal-header">
              <h2>Delete Book</h2>
            </div>
            <div className="modal-body">
              <p style={{ marginBottom: '1.25rem' }}>
                Are you sure you want to delete <strong>{deleteBookModal.title}</strong>? This will remove all copies and history. Books with active checkouts cannot be deleted.
              </p>
              <div style={{ display: 'flex', gap: '0.75rem' }}>
                <button
                  className="btn-checkout"
                  style={{ flex: 1, background: '#e94560' }}
                  onClick={handleDeleteBook}
                >
                  Delete
                </button>
                <button
                  className="btn-info"
                  style={{ flex: 1 }}
                  onClick={() => setDeleteBookModal(null)}
                >
                  Cancel
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
