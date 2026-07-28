import { useState } from 'react';
import { getPropertyInfo } from '../services/api';
import { PropertyInfo } from '../types';

export const PropertySearch = () => {
  const [block, setBlock] = useState<string>('');
  const [property, setProperty] = useState<PropertyInfo | null>(null);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string>('');

  const handleSearch = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!block.trim()) {
      setError('Please enter a block number');
      return;
    }

    setLoading(true);
    setError('');
    setProperty(null);

    try {
      const result = await getPropertyInfo(block.trim());
      setProperty(result);
      if (!result) {
        setError('Property not found');
      }
    } catch {
      setError('Property not found or an error occurred');
    } finally {
      setLoading(false);
    }
  };

  const renderBoolean = (value: boolean | null) => {
    if (value === null) return 'N/A';
    return value ? 'Yes' : 'No';
  };

  const renderNumber = (value: number | null) => {
    if (value === null) return 'N/A';
    return value.toString();
  };

  return (
    <div className="max-w-4xl mx-auto">
      <div className="mb-6">
        <h1 className="text-3xl font-bold text-primary-900">Property Information Search</h1>
        <p className="text-gray-600 mt-1">Search for HDB property details by block number</p>
      </div>

      <form onSubmit={handleSearch} className="mb-8">
        <div className="flex flex-col sm:flex-row gap-4">
          <input
            type="text"
            value={block}
            onChange={(e) => setBlock(e.target.value)}
            placeholder="Enter block number (e.g., 11)"
            className="input-field flex-1"
          />
          <button
            type="submit"
            disabled={loading}
            className="btn-primary whitespace-nowrap"
          >
            {loading ? 'Searching...' : 'Search'}
          </button>
        </div>
        {error && <div className="error-message mt-4">{error}</div>}
      </form>

      {property && (
        <div className="bg-white p-6 rounded-lg shadow-sm">
          <h2 className="text-2xl font-semibold text-primary-900 mb-6">
            Property Details for Block {property.block}
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <p className="text-sm text-gray-600 font-medium">Block</p>
              <p className="text-lg font-semibold">{property.block}</p>
            </div>
            <div>
              <p className="text-sm text-gray-600 font-medium">Street Name</p>
              <p className="text-lg">{property.streetName}</p>
            </div>
            <div>
              <p className="text-sm text-gray-600 font-medium">Town</p>
              <p className="text-lg">{property.town}</p>
            </div>
            <div>
              <p className="text-sm text-gray-600 font-medium">Postal Code</p>
              <p className="text-lg">{property.postalCode}</p>
            </div>
            <div>
              <p className="text-sm text-gray-600 font-medium">Max Floor Level</p>
              <p className="text-lg">{property.maxFloorLevel}</p>
            </div>
            <div>
              <p className="text-sm text-gray-600 font-medium">Year Completed</p>
              <p className="text-lg">{renderNumber(property.yearCompleted)}</p>
            </div>
            <div>
              <p className="text-sm text-gray-600 font-medium">Total Dwelling Units</p>
              <p className="text-lg">{renderNumber(property.totalDwellingUnits)}</p>
            </div>
            <div>
              <p className="text-sm text-gray-600 font-medium">Residential</p>
              <p className="text-lg">{renderBoolean(property.hasResidential)}</p>
            </div>
            <div>
              <p className="text-sm text-gray-600 font-medium">Commercial</p>
              <p className="text-lg">{renderBoolean(property.hasCommercial)}</p>
            </div>
            <div>
              <p className="text-sm text-gray-600 font-medium">Market/Hawker</p>
              <p className="text-lg">{renderBoolean(property.hasMarketHawker)}</p>
            </div>
            <div>
              <p className="text-sm text-gray-600 font-medium">Miscellaneous</p>
              <p className="text-lg">{renderBoolean(property.hasMiscellaneous)}</p>
            </div>
            <div>
              <p className="text-sm text-gray-600 font-medium">Multistorey Carpark</p>
              <p className="text-lg">{renderBoolean(property.hasMultistoreyCarpark)}</p>
            </div>
            <div>
              <p className="text-sm text-gray-600 font-medium">Precinct Pavilion</p>
              <p className="text-lg">{renderBoolean(property.hasPrecinctPavilion)}</p>
            </div>
          </div>
          
          <div className="mt-6">
            <h3 className="text-xl font-semibold text-primary-900 mb-4">Unit Sales Information</h3>
            <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
              <div className="bg-gray-50 p-3 rounded">
                <p className="text-sm text-gray-600 font-medium">1-Room Sold</p>
                <p className="text-lg font-semibold">{renderNumber(property.oneRoomSold)}</p>
              </div>
              <div className="bg-gray-50 p-3 rounded">
                <p className="text-sm text-gray-600 font-medium">2-Room Sold</p>
                <p className="text-lg font-semibold">{renderNumber(property.twoRoomSold)}</p>
              </div>
              <div className="bg-gray-50 p-3 rounded">
                <p className="text-sm text-gray-600 font-medium">3-Room Sold</p>
                <p className="text-lg font-semibold">{renderNumber(property.threeRoomSold)}</p>
              </div>
              <div className="bg-gray-50 p-3 rounded">
                <p className="text-sm text-gray-600 font-medium">4-Room Sold</p>
                <p className="text-lg font-semibold">{renderNumber(property.fourRoomSold)}</p>
              </div>
              <div className="bg-gray-50 p-3 rounded">
                <p className="text-sm text-gray-600 font-medium">5-Room Sold</p>
                <p className="text-lg font-semibold">{renderNumber(property.fiveRoomSold)}</p>
              </div>
              <div className="bg-gray-50 p-3 rounded">
                <p className="text-sm text-gray-600 font-medium">Executive Sold</p>
                <p className="text-lg font-semibold">{renderNumber(property.execSold)}</p>
              </div>
              <div className="bg-gray-50 p-3 rounded">
                <p className="text-sm text-gray-600 font-medium">Studio Apartment Sold</p>
                <p className="text-lg font-semibold">{renderNumber(property.studioApartmentSold)}</p>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};