import { Link, useLocation } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

export default function Navbar() {
  const { user, logout } = useAuth()
  const location = useLocation()

  const isActive = (path) => location.pathname === path ? 'active' : ''

  return (
    <nav className="navbar">
      <div className="nav-brand">Library Management System</div>
      <div className="nav-links">
        <Link to="/" className={isActive('/')}>Browse Books</Link>
        <Link to="/saved" className={isActive('/saved')}>My Books</Link>
        <Link to="/faulty" className={isActive('/faulty')}>Faulty Books</Link>
        {user.isAdmin && (
          <Link to="/admin" className={`admin-link ${isActive('/admin')}`}>Admin Panel</Link>
        )}
        <span className="nav-user">Hello, {user.username}</span>
        <button onClick={logout} className="btn-logout">Logout</button>
      </div>
    </nav>
  )
}
