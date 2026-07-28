export interface ResaleFlat {
  town: string;
  flatType: string;
  block: string;
  streetName: string;
  storeyRange: number;
  floorAreaSqm: number;
  resalePrice: number;
  leaseRemainYear: number;
  transactionDate: string;
  minPrice?: number;
  maxPrice?: number;
}

export interface FlatTypeStats {
  count: number;
  averagePrice: number;
  minPrice: number;
  maxPrice: number;
}

export interface TownStats {
  count: number;
  averagePrice: number;
  minPrice: number;
  maxPrice: number;
}

export interface YearlyTrend {
  year: number;
  averagePrice: number;
  transactionCount: number;
}

export interface Statistics {
  totalTransactions: number;
  averagePrice: number;
  minPrice: number;
  maxPrice: number;
  averageFloorArea: number;
  priceByFlatType: Record<string, FlatTypeStats>;
  priceByTown: Record<string, TownStats>;
  priceTrend: YearlyTrend[];
}

export interface PropertyInfo {
  block: string;
  streetName: string;
  town: string;
  postalCode: string;
  maxFloorLevel: string;
  yearCompleted: number | null;
  totalDwellingUnits: number | null;
  hasResidential: boolean | null;
  hasCommercial: boolean | null;
  hasMarketHawker: boolean | null;
  hasMiscellaneous: boolean | null;
  hasMultistoreyCarpark: boolean | null;
  hasPrecinctPavilion: boolean | null;
  oneRoomSold: number | null;
  twoRoomSold: number | null;
  threeRoomSold: number | null;
  fourRoomSold: number | null;
  fiveRoomSold: number | null;
  execSold: number | null;
  studioApartmentSold: number | null;
}