import axios from 'axios';
import type { ResaleFlat, Statistics, PropertyInfo } from '../types';

const API_BASE_URL = '/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = 'Bearer ' + token;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

// HDB endpoints
export const getResalePrices = async (params?: {
  town?: string;
  flatType?: string;
  minPrice?: number;
  maxPrice?: number;
  year?: number;
  limit?: number;
}): Promise<ResaleFlat[]> => {
  const response = await api.get('/hdb/resale-prices', { params });
  return response.data.data;
};

export const getStatistics = async (town?: string): Promise<Statistics> => {
  const response = await api.get('/hdb/statistics', { params: { town } });
  return response.data.data;
};

export const getPriceRanges = async (params?: {
  town?: string;
  roomType?: string;
  year?: number;
  limit?: number;
}) => {
  const response = await api.get('/hdb/price-ranges', { params });
  return response.data.data;
};

export const getPriceRangeStatistics = async (town?: string) => {
  const response = await api.get('/hdb/price-range-statistics', { params: { town } });
  return response.data.data;
};

export const getPropertyInfo = async (block: string): Promise<PropertyInfo> => {
  const response = await api.get('/hdb/property-info/' + block);
  return response.data.data;
};

export const getPropertyList = async (params?: {
  search?: string;
  town?: string;
  limit?: number;
  offset?: number;
}) => {
  const response = await api.get('/property/list', { params });
  return response.data;
};

export const getPropertyTowns = async (): Promise<string[]> => {
  const response = await api.get('/property/towns');
  return response.data.data;
};

export const getTowns = async (): Promise<string[]> => {
  const response = await api.get('/hdb/towns');
  return response.data.data;
};

export const getFlatTypes = async (): Promise<string[]> => {
  const response = await api.get('/hdb/flat-types');
  return response.data.data;
};

// CEA Salesperson endpoints
export const getCEASalespersons = async (params?: {
  search?: string;
  status?: string;
  agency?: string;
  limit?: number;
  offset?: number;
}) => {
  const response = await api.get('/ceasalesperson/list', { params });
  return response.data;
};

export const getCEAStatistics = async () => {
  const response = await api.get('/ceasalesperson/statistics');
  return response.data.data;
};

export const getCEAAgencies = async (): Promise<string[]> => {
  const response = await api.get('/ceasalesperson/agencies');
  return response.data.data;
};

// Auth endpoints
export const login = async (username: string, password: string): Promise<string> => {
  const response = await api.post('/auth/login', { username, password });
  const token = response.data.data.token;
  localStorage.setItem('token', token);
  return token;
};

export const logout = (): void => {
  localStorage.removeItem('token');
  window.location.href = '/login';
};

export default {
  getResalePrices,
  getStatistics,
  getPriceRanges,
  getPriceRangeStatistics,
  getPropertyInfo,
  getPropertyList,
  getPropertyTowns,
  getTowns,
  getFlatTypes,
  getCEASalespersons,
  getCEAStatistics,
  getCEAAgencies,
  login,
  logout,
  get: api.get,
  post: api.post,
  put: api.put,
  delete: api.delete,
};