import { useState, useEffect } from 'react';
import api from '../services/api';

interface CEASalesperson {
  id: number;
  salespersonName: string;
  registrationNumber: string;
  estateAgentName: string;
  estateAgentLicenseNo: string;
  registrationStartDate: string;
  registrationEndDate: string;
  status: string;
}

interface CEASalespersonStats {
  totalCount: number;
  activeCount: number;
  agencyDistribution: Record<string, number>;
}

export const CEASalesperson = () => {
  const [salespersons, setSalespersons] = useState<CEASalesperson[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [search, setSearch] = useState<string>('');
  const [selectedStatus, setSelectedStatus] = useState<string>('');
  const [selectedAgency, setSelectedAgency] = useState<string>('');
  const [agencies, setAgencies] = useState<string[]>([]);
  const [stats, setStats] = useState<CEASalespersonStats | null>(null);
  const [currentPage, setCurrentPage] = useState<number>(0);
  const [pageSize, setPageSize] = useState<number>(50);
  const [totalCount, setTotalCount] = useState<number>(0);
  const [totalPages, setTotalPages] = useState<number>(0);

  useEffect(() => {
    fetchStatistics();
    fetchAgencies();
  }, []);

  useEffect(() => {
    fetchSalespersons();
  }, [search, selectedStatus, selectedAgency, currentPage, pageSize]);

  const fetchStatistics = async (): Promise<void> => {
    try {
      const response = await api.get('/ceasalesperson/statistics');
      setStats(response.data.data);
    } catch (error) {
      console.error('Error fetching statistics:', error);
    }
  };

  const fetchAgencies = async (): Promise<void> => {
    try {
      const response = await api.get('/ceasalesperson/agencies');
      setAgencies(response.data.data || []);
      console.log('Agencies loaded:', response.data.data);
    } catch (error) {
      console.error('Error fetching agencies:', error);
    }
  };

  const fetchSalespersons = async (): Promise<void> => {
    try {
      setLoading(true);
      const params: Record<string, any> = {
        limit: pageSize,
        offset: currentPage * pageSize
      };
      if (search) params.search = search;
      if (selectedStatus) params.status = selectedStatus;
      if (selectedAgency) params.agency = selectedAgency;
      
      console.log('Fetching with params:', params);
      const response = await api.get('/ceasalesperson/list', { params });
      console.log('Response data:', response.data);
      
      setSalespersons(response.data.data || []);
      setTotalCount(response.data.total || response.data.count || 0);
      setTotalPages(Math.ceil((response.data.total || response.data.count || 0) / pageSize));
    } catch (error) {
      console.error('Error fetching salespersons:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleSearch = (e: React.FormEvent<HTMLFormElement>): void => {
    e.preventDefault();
    setCurrentPage(0);
    fetchSalespersons();
  };

  const handleClearFilters = (): void => {
    setSearch('');
    setSelectedStatus('');
    setSelectedAgency('');
    setCurrentPage(0);
  };

  const goToPage = (page: number): void => {
    if (page >= 0 && page < totalPages) {
      setCurrentPage(page);
    }
  };

  const getStatusBadgeColor = (status: string): string => {
    if (!status) return 'bg-gray-200 text-gray-700';
    const lowerStatus = status.toLowerCase();
    if (lowerStatus === 'active') return 'bg-green-100 text-green-800';
    if (lowerStatus === 'expired') return 'bg-red-100 text-red-800';
    if (lowerStatus === 'pending') return 'bg-yellow-100 text-yellow-800';
    return 'bg-gray-200 text-gray-700';
  };

  const getPageNumbers = (): number[] => {
    const pages: number[] = [];
    const maxVisible = 5;
    let startPage = 0;
    let endPage = totalPages - 1;
    
    if (totalPages > maxVisible) {
      if (currentPage < 3) {
        startPage = 0;
        endPage = maxVisible - 1;
      } else if (currentPage > totalPages - 3) {
        startPage = totalPages - maxVisible;
        endPage = totalPages - 1;
      } else {
        startPage = currentPage - 2;
        endPage = currentPage + 2;
      }
    }
    
    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }
    return pages;
  };

  const renderPagination = (): JSX.Element | null => {
    if (totalPages <= 1) {
      return null;
    }

    return (
      <div className="flex flex-wrap justify-center items-center gap-2 mt-6">
        <button
          onClick={() => goToPage(0)}
          disabled={currentPage === 0}
          className="px-3 py-1 border rounded hover:bg-gray-100 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          First
        </button>
        <button
          onClick={() => goToPage(currentPage - 1)}
          disabled={currentPage === 0}
          className="px-3 py-1 border rounded hover:bg-gray-100 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          Previous
        </button>
        
        {getPageNumbers().map((pageNum) => (
          <button
            key={pageNum}
            onClick={() => goToPage(pageNum)}
            className={`px-3 py-1 border rounded ${
              currentPage === pageNum 
                ? 'bg-primary-900 text-white' 
                : 'hover:bg-gray-100'
            }`}
          >
            {pageNum + 1}
          </button>
        ))}
        
        <button
          onClick={() => goToPage(currentPage + 1)}
          disabled={currentPage === totalPages - 1}
          className="px-3 py-1 border rounded hover:bg-gray-100 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          Next
        </button>
        <button
          onClick={() => goToPage(totalPages - 1)}
          disabled={currentPage === totalPages - 1}
          className="px-3 py-1 border rounded hover:bg-gray-100 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          Last
        </button>
      </div>
    );
  };

  if (loading && salespersons.length === 0) {
    return <div className="loading-spinner">Loading salespersons...</div>;
  }

  return (
    <div className="max-w-full">
      {/* Header */}
      <div className="mb-6">
        <h1 className="text-3xl font-bold text-primary-900">CEA Salesperson Information</h1>
        <p className="text-gray-600 mt-1">
          {totalCount > 0 
            ? `Showing ${totalCount.toLocaleString()} salespersons` 
            : 'No records found'}
        </p>
      </div>

      {/* Statistics Cards */}
      {stats && (
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-6">
          <div className="stat-card">
            <p className="stat-label">Total Salespersons</p>
            <p className="stat-value">{stats.totalCount.toLocaleString()}</p>
          </div>
          <div className="stat-card">
            <p className="stat-label">Active Salespersons</p>
            <p className="stat-value">{stats.activeCount.toLocaleString()}</p>
          </div>
          <div className="stat-card">
            <p className="stat-label">Total Agencies</p>
            <p className="stat-value">{Object.keys(stats.agencyDistribution || {}).length.toLocaleString()}</p>
          </div>
        </div>
      )}

      {/* Search and Filters */}
      <div className="bg-white p-4 rounded-lg shadow-sm mb-6">
        <form onSubmit={handleSearch} className="flex flex-col md:flex-row gap-4">
          <div className="flex-1">
            <input
              type="text"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Search by name, registration number, or agency..."
              className="input-field"
            />
          </div>
          <div className="md:w-40">
            <select
              value={selectedStatus}
              onChange={(e) => setSelectedStatus(e.target.value)}
              className="select-field w-full"
            >
              <option value="">All Status</option>
              <option value="Active">Active</option>
              <option value="Expired">Expired</option>
              <option value="Pending">Pending</option>
            </select>
          </div>
          <div className="md:w-48">
            <select
              value={selectedAgency}
              onChange={(e) => setSelectedAgency(e.target.value)}
              className="select-field w-full"
            >
              <option value="">All Agencies</option>
              {agencies.map((agency) => (
                <option key={agency} value={agency}>{agency}</option>
              ))}
            </select>
          </div>
          <button type="submit" className="btn-primary whitespace-nowrap">
            Search
          </button>
          <button
            type="button"
            onClick={handleClearFilters}
            className="bg-gray-500 text-white px-4 py-2 rounded-lg hover:bg-gray-600 transition-colors"
          >
            Clear
          </button>
        </form>
      </div>

      {/* Results */}
      {salespersons.length === 0 ? (
        <div className="bg-white p-8 rounded-lg shadow-sm text-center">
          <p className="text-gray-500 text-lg">No salespersons found matching your criteria</p>
        </div>
      ) : (
        <div>
          <div className="overflow-x-auto bg-white rounded-lg shadow-sm">
            <table className="w-full text-sm">
              <thead>
                <tr className="bg-primary-900 text-white">
                  <th className="px-3 py-3 text-left">Name</th>
                  <th className="px-3 py-3 text-left">Registration No.</th>
                  <th className="px-3 py-3 text-left">Agency</th>
                  <th className="px-3 py-3 text-left">License No.</th>
                  <th className="px-3 py-3 text-left">Start Date</th>
                  <th className="px-3 py-3 text-left">End Date</th>
                  <th className="px-3 py-3 text-left">Status</th>
                </tr>
              </thead>
              <tbody>
                {salespersons.map((person) => (
                  <tr key={person.id} className="border-t border-gray-100 hover:bg-gray-50 transition-colors">
                    <td className="px-3 py-2 font-medium">{person.salespersonName}</td>
                    <td className="px-3 py-2">{person.registrationNumber}</td>
                    <td className="px-3 py-2">{person.estateAgentName}</td>
                    <td className="px-3 py-2">{person.estateAgentLicenseNo}</td>
                    <td className="px-3 py-2">{person.registrationStartDate}</td>
                    <td className="px-3 py-2">{person.registrationEndDate}</td>
                    <td className="px-3 py-2">
                      <span className={`px-2 py-1 rounded-full text-xs font-medium ${getStatusBadgeColor(person.status)}`}>
                        {person.status}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* Pagination */}
          {renderPagination()}
          
          {/* Page Info */}
          {totalPages > 0 && (
            <div className="text-center text-sm text-gray-500 mt-4">
              Showing {salespersons.length} records • Page {currentPage + 1} of {totalPages}
            </div>
          )}
        </div>
      )}
    </div>
  );
};

export default CEASalesperson;