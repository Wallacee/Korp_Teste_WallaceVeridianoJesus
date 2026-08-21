export interface ConsumptionForecast {
  hasEnoughData: boolean;
  historicalDays: number;
  forecastDays: number;
  dailyAverage: number;
  estimatedConsumption: number;
  trend: string;
  forecast: ConsumptionForecastPoint[];
}

export interface ConsumptionForecastPoint {
  date: string;
  estimatedQuantity: number;
}
