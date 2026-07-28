import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './hooks/useAuth';
import { PrivateRoute } from './components/PrivateRoute';
import { Dashboard } from './pages/Dashboard';
import { Login } from './pages/Login';
import StatisticsPage from './pages/Statistics';
import { PropertySearch } from './pages/PropertySearch';
import { PropertyGrid } from './pages/PropertyGrid';
import { CEASalesperson } from './pages/CEASalesperson';
import { Navbar } from './components/Navbar';

function App() {
  return (
    <AuthProvider>
      <Router>
        <div className="min-h-screen flex flex-col bg-gray-50">
          <Navbar />
          <main className="flex-1 container mx-auto px-4 py-8 max-w-7xl">
            <Routes>
              <Route path="/login" element={<Login />} />
              <Route path="/" element={<PrivateRoute><Dashboard /></PrivateRoute>} />
              <Route path="/statistics" element={<PrivateRoute><StatisticsPage /></PrivateRoute>} />
              <Route path="/property-search" element={<PrivateRoute><PropertySearch /></PrivateRoute>} />
              <Route path="/property-grid" element={<PrivateRoute><PropertyGrid /></PrivateRoute>} />
              <Route path="/cea-salesperson" element={<PrivateRoute><CEASalesperson /></PrivateRoute>} />
              <Route path="*" element={<Navigate to="/" replace />} />
            </Routes>
          </main>
        </div>
      </Router>
    </AuthProvider>
  );
}

export default App;