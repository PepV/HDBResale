import { useState, useEffect } from 'react';
import { PieChart, Pie, Cell, ResponsiveContainer, Tooltip, Legend } from 'recharts';
import { getStatistics, getTowns } from '../services/api';
import type { Statistics } from '../types';

const COLORS: string[] = ['#3f51b5', '#4caf50', '#ff9800', '#f44336', '#9c27b0', '#00bcd4', '#ffc658'];

interface PieData {
  name: string;
  value: number;
  avgPrice: number;
}

export const StatisticsPage = () => {
  const [statistics, setStatistics] = useState<Statistics | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [selectedTown, setSelectedTown] = useState<string>('');
  const [towns, setTowns] = useState<string[]>([]);

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        const [stats, townsList] = await Promise.all([
          getStatistics(selectedTown || undefined),
          getTowns(),
        ]);
        setStatistics(stats);
        setTowns(townsList);
      } catch (error) {
        console.error('Error fetching statistics:', error);
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, [selectedTown]);

  if (loading) {
    return <div className="loading-spinner">Loading statistics...</div>;
  }

  if (!statistics) {
    return <div className="text-center py-12 text-red-600">Failed to load statistics</div>;
  }

  const pieData: PieData[] = Object.entries(statistics.priceByFlatType).map(([name, data]) => ({
    name: name,
    value: data.count,
    avgPrice: data.averagePrice,
  }));

  const renderLabel = (entry: any) => {
    return entry.name + ': ' + (entry.percent * 100).toFixed(1) + '%';
  };

  return (
    <div>
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-6">
        <h1 className="text-3xl font-bold text-primary-900">Detailed Statistics</h1>
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
          <p className="stat-label">Total Transactions</p>
          <p className="stat-value">{statistics.totalTransactions.toLocaleString()}</p>
        </div>
        <div className="stat-card">
          <p className="stat-label">Average Price</p>
          <p className="stat-value">${(statistics.averagePrice || 0).toLocaleString()}</p>
        </div>
        {/* <div className="stat-card">
          <p className="stat-label">Price Range</p>
          <p className="stat-value text-lg">
             {statistics.averagePrice}
          </p>
        </div> */}
        {/* <div className="stat-card">
          <p className="stat-label">Average Area</p>
          <p className="stat-value">{statistics.averageFloorArea.toFixed(1)} m²</p>
        </div> */}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="chart-container">
          <h2 className="text-xl font-semibold mb-4">Distribution by Flat Type</h2>
          <ResponsiveContainer width="100%" height={400}>
            <PieChart>
              <Pie
                data={pieData}
                cx="50%"
                cy="50%"
                labelLine={false}
                outerRadius={150}
                dataKey="value"
                label={renderLabel}
              >
                {pieData.map((entry, index) => (
                  <Cell key={'cell-' + index} fill={COLORS[index % COLORS.length]} />
                ))}
              </Pie>
              <Tooltip />
              <Legend />
            </PieChart>
          </ResponsiveContainer>
        </div>

        <div className="chart-container">
          <h2 className="text-xl font-semibold mb-4">Price Statistics by Flat Type</h2>
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="bg-gray-50">
                  <th className="px-3 py-2 text-left">Flat Type</th>
                  <th className="px-3 py-2 text-left">Count</th>
                  <th className="px-3 py-2 text-left">Avg Price</th>
                </tr>
              </thead>
              <tbody>
                {Object.entries(statistics.priceByFlatType).slice(0, 8).map(([type, data]) => (
                  <tr key={type} className="border-t border-gray-100 hover:bg-gray-50">
                    <td className="px-3 py-2 font-medium">{type}</td>
                    <td className="px-3 py-2">{data.count}</td>
                    <td className="px-3 py-2">${(data.averagePrice || 0).toLocaleString()}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  );
};

export default StatisticsPage;