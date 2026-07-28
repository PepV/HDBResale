import { useState, useEffect } from 'react';
import {
  LineChart,
  Line,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer
} from 'recharts';
import { getStatistics, getResalePrices, getTowns } from '../services/api';
import { Statistics, ResaleFlat } from '../types';

export const Dashboard = () => {
  const [statistics, setStatistics] = useState<Statistics | null>(null);
  const [recentTransactions, setRecentTransactions] = useState<ResaleFlat[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [selectedTown, setSelectedTown] = useState<string>('');
  const [towns, setTowns] = useState<string[]>([]);

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        const [stats, recent, townsList] = await Promise.all([
          getStatistics(selectedTown || undefined),
          getResalePrices({ limit: 5 }),
          getTowns(),
        ]);
        setStatistics(stats);
        setRecentTransactions(recent);
        setTowns(townsList);
      } catch (error) {
        console.error('Error fetching dashboard data:', error);
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, [selectedTown]);

  if (loading) {
    return (
      <div className="flex flex-col items-center justify-center min-h-[400px]">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-900 mb-4"></div>
        <p className="text-gray-600">Loading dashboard data...</p>
      </div>
    );
  }

  if (!statistics || statistics.totalTransactions === 0) {
    return (
      <div className="text-center py-12">
        <h2 className="text-2xl text-gray-600">No data available</h2>
        <p className="text-gray-500 mt-2">Please try selecting a different town or check back later</p>
      </div>
    );
  }

  const flatTypeData = Object.entries(statistics.priceByFlatType || {}).map(([name, data]) => ({
    name,
    averagePrice: data.averagePrice || 0,
    count: data.count || 0,
  }));

  const townData = Object.entries(statistics.priceByTown || {})
    .map(([name, data]) => ({
      name,
      averagePrice: data.averagePrice || 0,
      count: data.count || 0,
    }))
    .sort((a, b) => b.averagePrice - a.averagePrice)
    .slice(0, 10);

  const trendData = (statistics.priceTrend || []).map((item) => ({
    year: item.year.toString(),
    averagePrice: item.averagePrice || 0,
    transactions: item.transactionCount || 0,
  }));

  return (
    <div>
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-6">
        <h1 className="text-3xl font-bold text-primary-900">Dashboard</h1>
        <select
          value={selectedTown}
          onChange={(e) => setSelectedTown(e.target.value)}
          className="select-field"
        >
          <option value="">All Towns</option>
          {towns.map((town) => (
            <option key={town} value={town}>{town}</option>
          ))}
        </select>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
        <div className="stat-card">
          <p className="stat-label">Total Records</p>
          <p className="stat-value">{statistics.totalTransactions.toLocaleString()}</p>
        </div>
       <div className="stat-card">
          <p className="stat-label">Average Price</p>
          <p className="stat-value">${(statistics.averagePrice || 0).toLocaleString()}</p>
        </div>
        <div className="stat-card">
          <p className="stat-label">Min Price</p>
          <p className="stat-value">${(statistics.minPrice || 0).toLocaleString()}</p>
        </div>
        <div className="stat-card">
          <p className="stat-label">Max Price</p>
          <p className="stat-value">${(statistics.maxPrice || 0).toLocaleString()}</p>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="chart-container">
          <h2 className="text-xl font-semibold mb-4">Price Trends by Year</h2>
          <ResponsiveContainer width="100%" height={300}>
            <LineChart data={trendData}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="year" />
              <YAxis />
              <Tooltip />
              <Legend />
              <Line type="monotone" dataKey="averagePrice" stroke="#3f51b5" name="Avg Price ($)" />
              <Line type="monotone" dataKey="transactions" stroke="#4caf50" name="Records" />
            </LineChart>
          </ResponsiveContainer>
        </div>

        <div className="chart-container">
          <h2 className="text-xl font-semibold mb-4">Average Price by Room Type</h2>
          <ResponsiveContainer width="100%" height={300}>
            <BarChart data={flatTypeData}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="name" />
              <YAxis />
              <Tooltip />
              <Bar dataKey="averagePrice" fill="#3f51b5" />
            </BarChart>
          </ResponsiveContainer>
        </div>

        <div className="chart-container">
          <h2 className="text-xl font-semibold mb-4">Top 10 Towns by Average Price</h2>
          <ResponsiveContainer width="100%" height={300}>
            <BarChart data={townData} layout="vertical">
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis type="number" />
              <YAxis dataKey="name" type="category" width={80} />
              <Tooltip />
              <Bar dataKey="averagePrice" fill="#4caf50" />
            </BarChart>
          </ResponsiveContainer>
        </div>

        <div className="chart-container">
          <h2 className="text-xl font-semibold mb-4">Recent Price Ranges</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="bg-gray-50">
                  <th className="px-3 py-2 text-left">Town</th>
                  <th className="px-3 py-2 text-left">Room Type</th>
                  <th className="px-3 py-2 text-left">Min Price</th>
                  <th className="px-3 py-2 text-left">Max Price</th>
                  <th className="px-3 py-2 text-left">Year</th>
                </tr>
              </thead>
              <tbody>
                {recentTransactions && recentTransactions.length > 0 ? (
                  recentTransactions.slice(0, 5).map((t, i) => (
                    <tr key={i} className="border-t border-gray-100 hover:bg-gray-50">
                      <td className="px-3 py-2">{t.town || 'N/A'}</td>
                      <td className="px-3 py-2">{t.flatType || 'N/A'}</td>
                      <td className="px-3 py-2 font-medium text-green-600">
                        ${(t.minPrice || t.resalePrice || 0).toLocaleString()}
                      </td>
                      <td className="px-3 py-2 font-medium text-red-600">
                        ${(t.maxPrice || t.resalePrice || 0).toLocaleString()}
                      </td>
                      <td className="px-3 py-2">
                        {t.transactionDate ? new Date(t.transactionDate).getFullYear() : 'N/A'}
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td colSpan={5} className="text-center py-4 text-gray-500">
                      No recent transactions found
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  );
};