import { Link, useLocation } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

export default function Navbar() {
  const { user, logout } = useAuth()
  const location = useLocation()

  return (
    <nav className="navbar">
      <div className="nav-brand">Library Management System</div>
      <div className="nav-links">
        <Link to="/" className={location.pathname === '/' ? 'active' : ''}>
          Browse Books
        </Link>
        <Link to="/saved" className={location.pathname === '/saved' ? 'active' : ''}>
          My Books
        </Link>
        <span className="nav-user">Hello, {user.username}</span>
        <button onClick={logout} className="btn-logout">Logout</button>
      </div>
    </nav>
  )
}
