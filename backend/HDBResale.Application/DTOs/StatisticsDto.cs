namespace HDBResale.Application.DTOs;

public class StatisticsDto
{
    public int TotalTransactions { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public double AverageFloorArea { get; set; }
    public Dictionary<string, FlatTypeStatistics> PriceByFlatType { get; set; } = new();
    public Dictionary<string, TownStatistics> PriceByTown { get; set; } = new();
    public List<YearlyTrend> PriceTrend { get; set; } = new();
}

public class FlatTypeStatistics
{
    public int Count { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
}

public class TownStatistics
{
    public int Count { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
}

public class YearlyTrend
{
    public int Year { get; set; }
    public decimal AveragePrice { get; set; }
    public int TransactionCount { get; set; }
}