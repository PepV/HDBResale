import { useState, useEffect } from 'react';
import api from '../services/api';

interface PropertyRecord {
  id: number;
  block: string;
  street: string;
  town: string;
  maxFloorLevel: string;
  yearCompleted: string;
  residential: string;
  commercial: string;
  marketHawker: string;
  miscellaneous: string;
  multistoreyCarpark: string;
  precinctPavilion: string;
  totalDwellingUnits: string;
  oneRoomSold: string;
  twoRoomSold: string;
  threeRoomSold: string;
  fourRoomSold: string;
  fiveRoomSold: string;
  execSold: string;
  studioApartmentSold: string;
}

export const PropertyGrid = () => {
  const [properties, setProperties] = useState<PropertyRecord[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [searchTerm, setSearchTerm] = useState<string>('');
  const [selectedTown, setSelectedTown] = useState<string>('');
  const [towns, setTowns] = useState<string[]>([]);
  const [currentPage, setCurrentPage] = useState<number>(0);
  const [pageSize, setPageSize] = useState<number>(50);
  const [totalCount, setTotalCount] = useState<number>(0);
  const [totalPages, setTotalPages] = useState<number>(0);

  // Load towns on component mount
  useEffect(() => {
    loadTowns();
  }, []);

  // Load properties when filters change
  useEffect(() => {
    loadProperties();
  }, [searchTerm, selectedTown, currentPage, pageSize]);

  const loadTowns = async (): Promise<void> => {
    try {
      const response = await api.get('/property/towns');
      setTowns(response.data.data || []);
    } catch (error) {
      console.error('Error loading towns:', error);
    }
  };

  const loadProperties = async (): Promise<void> => {
    try {
      setLoading(true);
      const params: Record<string, any> = {
        limit: pageSize,
        offset: currentPage * pageSize
      };
      
      if (searchTerm.trim()) {
        params.search = searchTerm.trim();
      }
      if (selectedTown) {
        params.town = selectedTown;
      }
      
      const response = await api.get('/property/list', { params });
      const data = response.data.data || [];
      const total = response.data.total || response.data.count || 0;
      
      setProperties(data);
      setTotalCount(total);
      setTotalPages(Math.ceil(total / pageSize));
    } catch (error) {
      console.error('Error loading properties:', error);
      setProperties([]);
      setTotalCount(0);
      setTotalPages(0);
    } finally {
      setLoading(false);
    }
  };

  const handleSearch = (e: React.FormEvent<HTMLFormElement>): void => {
    e.preventDefault();
    setCurrentPage(0);
    loadProperties();
  };

  const handlePageSizeChange = (e: React.ChangeEvent<HTMLSelectElement>): void => {
    setPageSize(Number(e.target.value));
    setCurrentPage(0);
  };

  const handleClearFilters = (): void => {
    setSearchTerm('');
    setSelectedTown('');
    setCurrentPage(0);
  };

  const goToPage = (page: number): void => {
    if (page >= 0 && page < totalPages) {
      setCurrentPage(page);
    }
  };

  const renderYesNo = (value: string): string => {
    if (!value) return '—';
    return value.toUpperCase() === 'Y' ? 'Yes' : 'No';
  };

  const renderPaginationButtons = (): JSX.Element[] => {
    const buttons: JSX.Element[] = [];
    const maxVisible = 5;
    let start = 0;
    let end = totalPages - 1;
    
    if (totalPages > maxVisible) {
      if (currentPage < 3) {
        start = 0;
        end = maxVisible - 1;
      } else if (currentPage > totalPages - 3) {
        start = totalPages - maxVisible;
        end = totalPages - 1;
      } else {
        start = currentPage - 2;
        end = currentPage + 2;
      }
    }
    
    for (let i = start; i <= end; i++) {
      const isActive = i === currentPage;
      buttons.push(
        <button
          key={i}
          onClick={() => goToPage(i)}
          className={`px-3 py-1 border rounded ${
            isActive 
              ? 'bg-primary-900 text-white' 
              : 'hover:bg-gray-100'
          }`}
        >
          {i + 1}
        </button>
      );
    }
    return buttons;
  };

  if (loading) {
    return (
      <div className="flex justify-center items-center min-h-[400px]">
        <div className="text-gray-500 text-lg">Loading properties...</div>
      </div>
    );
  }

  return (
    <div className="max-w-full">
      {/* Header */}
      <div className="mb-6">
        <h1 className="text-3xl font-bold text-primary-900">HDB Property Information</h1>
        <p className="text-gray-600 mt-1">
          {totalCount > 0 
            ? `Showing ${totalCount.toLocaleString()} records` 
            : 'No records found'}
        </p>
      </div>

      {/* Search and Filters */}
      <div className="bg-white p-4 rounded-lg shadow-sm mb-6">
        <form onSubmit={handleSearch} className="flex flex-col md:flex-row gap-4">
          <div className="flex-1">
            <input
              type="text"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              placeholder="Search by block, street, or town..."
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent outline-none"
            />
          </div>
          
          <div className="md:w-48">
            <select
              value={selectedTown}
              onChange={(e) => setSelectedTown(e.target.value)}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent outline-none bg-white"
            >
              <option value="">All Towns</option>
              {towns.map((town) => (
                <option key={town} value={town}>{town}</option>
              ))}
            </select>
          </div>
          
          <div className="md:w-32">
            <select
              value={pageSize}
              onChange={handlePageSizeChange}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent outline-none bg-white"
            >
              <option value={20}>20 per page</option>
              <option value={50}>50 per page</option>
              <option value={100}>100 per page</option>
            </select>
          </div>
          
          <button
            type="submit"
            className="px-6 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 transition-colors font-medium"
          >
            Search
          </button>
          
          <button
            type="button"
            onClick={handleClearFilters}
            className="px-6 py-2 bg-gray-500 text-white rounded-lg hover:bg-gray-600 transition-colors font-medium"
          >
            Clear
          </button>
        </form>
      </div>

      {/* Results */}
      {properties.length === 0 ? (
        <div className="bg-white p-8 rounded-lg shadow-sm text-center">
          <p className="text-gray-500 text-lg">No properties found matching your criteria</p>
        </div>
      ) : (
        <>
          {/* Table */}
          <div className="overflow-x-auto bg-white rounded-lg shadow-sm">
            <table className="w-full text-sm">
              <thead>
                <tr className="bg-primary-900 text-white">
                  <th className="px-3 py-3 text-left whitespace-nowrap">Block</th>
                  <th className="px-3 py-3 text-left whitespace-nowrap">Street</th>
                  <th className="px-3 py-3 text-left whitespace-nowrap">Town</th>
                  <th className="px-3 py-3 text-left whitespace-nowrap">Max Floor</th>
                  <th className="px-3 py-3 text-left whitespace-nowrap">Year</th>
                  <th className="px-3 py-3 text-left whitespace-nowrap">Res</th>
                  <th className="px-3 py-3 text-left whitespace-nowrap">Com</th>
                  <th className="px-3 py-3 text-left whitespace-nowrap">Market</th>
                  <th className="px-3 py-3 text-left whitespace-nowrap">Misc</th>
                  <th className="px-3 py-3 text-left whitespace-nowrap">Carpark</th>
                  <th className="px-3 py-3 text-left whitespace-nowrap">Pavilion</th>
                  <th className="px-3 py-3 text-left whitespace-nowrap">Total Units</th>
                  <th className="px-3 py-3 text-left whitespace-nowrap">3R Sold</th>
                  <th className="px-3 py-3 text-left whitespace-nowrap">4R Sold</th>
                  <th className="px-3 py-3 text-left whitespace-nowrap">5R Sold</th>
                </tr>
              </thead>
              <tbody>
                {properties.map((prop) => (
                  <tr key={prop.id} className="border-t border-gray-100 hover:bg-gray-50 transition-colors">
                    <td className="px-3 py-2 font-medium whitespace-nowrap">{prop.block}</td>
                    <td className="px-3 py-2">{prop.street}</td>
                    <td className="px-3 py-2">{prop.town}</td>
                    <td className="px-3 py-2">{prop.maxFloorLevel}</td>
                    <td className="px-3 py-2">{prop.yearCompleted}</td>
                    <td className="px-3 py-2">{renderYesNo(prop.residential)}</td>
                    <td className="px-3 py-2">{renderYesNo(prop.commercial)}</td>
                    <td className="px-3 py-2">{renderYesNo(prop.marketHawker)}</td>
                    <td className="px-3 py-2">{renderYesNo(prop.miscellaneous)}</td>
                    <td className="px-3 py-2">{renderYesNo(prop.multistoreyCarpark)}</td>
                    <td className="px-3 py-2">{renderYesNo(prop.precinctPavilion)}</td>
                    <td className="px-3 py-2 text-center">{prop.totalDwellingUnits}</td>
                    <td className="px-3 py-2 text-center">{prop.threeRoomSold}</td>
                    <td className="px-3 py-2 text-center">{prop.fourRoomSold}</td>
                    <td className="px-3 py-2 text-center">{prop.fiveRoomSold}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* Pagination */}
          {totalPages > 1 && (
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
              
              {renderPaginationButtons()}
              
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
          )}
          
          {/* Page Info */}
          {totalPages > 0 && (
            <div className="text-center text-sm text-gray-500 mt-4">
              Page {currentPage + 1} of {totalPages} • {properties.length} records shown
            </div>
          )}
        </>
      )}
    </div>
  );
};

export default PropertyGrid;