import { useState, useEffect } from 'react'
import { adminApi, booksApi } from '../services/api'

export default function AdminPage() {
  const [tab, setTab] = useState('checkouts')
  const [checkouts, setCheckouts] = useState([])
  const [users, setUsers] = useState([])
  const [allBooks, setAllBooks] = useState([])
  const [loading, setLoading] = useState(true)
  const [message, setMessage] = useState('')
  const [faultyModal, setFaultyModal] = useState(null)
  const [faultyReason, setFaultyReason] = useState('')

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

  const isOverdue = (dueDate) => new Date(dueDate) < new Date()

  if (loading) return <div className="loading">Loading admin data...</div>

  const totalBorrowed = checkouts.length
  const overdueCount = checkouts.filter(c => isOverdue(c.dueDate)).length

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

      {tab === 'checkouts' && (
        <div className="admin-table-wrap">
          {checkouts.length === 0 ? (
            <p className="empty" style={{ padding: '1.5rem' }}>No active checkouts.</p>
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
                {checkouts.map((c, i) => (
                  <tr key={c.userBookId} className={isOverdue(c.dueDate) ? 'row-overdue' : ''}>
                    <td>{i + 1}</td>
                    <td>{c.username}</td>
                    <td>{c.bookTitle}</td>
                    <td>Copy {c.copyNumber}</td>
                    <td className="copy-id-cell">#{c.bookCopyId}</td>
                    <td>{new Date(c.checkedOutAt).toLocaleDateString()}</td>
                    <td>{new Date(c.dueDate).toLocaleDateString()}</td>
                    <td>
                      {isOverdue(c.dueDate)
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
                    <li key={c.userBookId} className={isOverdue(c.dueDate) ? 'overdue-item' : ''}>
                      <span>{c.bookTitle}</span>
                      <span className="copy-tag-inline">Copy {c.copyNumber} · ID #{c.bookCopyId}</span>
                      <span className="checkout-due">Due: {new Date(c.dueDate).toLocaleDateString()}</span>
                      {isOverdue(c.dueDate) && <span className="overdue-badge">OVERDUE</span>}
                    </li>
                  ))}
                </ul>
              )}
            </div>
          ))}
        </div>
      )}

      {tab === 'books' && (
        <div className="admin-table-wrap">
          <table className="admin-table">
            <thead>
              <tr>
                <th>Title</th>
                <th>Author</th>
                <th>Genre</th>
                <th>Available Copies</th>
                <th>Total Copies</th>
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
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

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
    </div>
  )
}
