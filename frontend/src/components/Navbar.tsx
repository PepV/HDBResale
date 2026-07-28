import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';

export const Navbar = () => {
  const { isAuthenticated, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  if (!isAuthenticated) {
    return null;
  }

  return (
    <nav className="bg-primary-900 text-white shadow-lg">
      <div className="container mx-auto px-4 max-w-7xl">
        <div className="flex justify-between items-center h-16">
          <Link to="/" className="flex items-center space-x-2 text-xl font-bold hover:text-gray-300 transition-colors">
            <span>🏠</span>
            <span>HDB Analytics</span>
          </Link>
          <div className="flex items-center space-x-6">
            <Link to="/" className="hover:text-gray-300 transition-colors">Dashboard</Link>
            <Link to="/statistics" className="hover:text-gray-300 transition-colors">Statistics</Link>
            <Link to="/property-search" className="hover:text-gray-300 transition-colors">Property Search</Link>
            <Link to="/property-grid" className="hover:text-gray-300 transition-colors">Property Grid</Link>
            <Link to="/cea-salesperson" className="hover:text-gray-300 transition-colors">CEA Salesperson</Link>
            <button
              onClick={handleLogout}
              className="bg-red-600 hover:bg-red-700 px-4 py-2 rounded-lg transition-colors font-medium"
            >
              Logout
            </button>
          </div>
        </div>
      </div>
    </nav>
  );
};