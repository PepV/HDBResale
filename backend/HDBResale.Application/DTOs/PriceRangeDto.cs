namespace HDBResale.Application.DTOs;

public class PriceRangeDto
{
    public int Id { get; set; }
    public int FinancialYear { get; set; }
    public string Town { get; set; } =" ";
    public string RoomType { get; set; } =" ";
    public decimal MinSellingPrice { get; set; }
    public decimal MaxSellingPrice { get; set; }
    public decimal MinSellingPriceLessAhgShg { get; set; }
    public decimal MaxSellingPriceLessAhgShg { get; set; }
}

public class PriceRangeStatisticsDto
{
    public List<PriceRangeDto> PriceRanges { get; set; } = new();
    public Dictionary<string, TownPriceRangeStats> TownStats { get; set; } = new();
    public Dictionary<string, RoomTypePriceRangeStats> RoomTypeStats { get; set; } = new();
    public List<YearlyPriceRangeStats> YearlyStats { get; set; } = new();
}

public class TownPriceRangeStats
{
    public string Town { get; set; } =" ";
    public int Count { get; set; }
    public decimal AverageMinPrice { get; set; }
    public decimal AverageMaxPrice { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
}

public class RoomTypePriceRangeStats
{
    public string RoomType { get; set; } =" ";
    public int Count { get; set; }
    public decimal AverageMinPrice { get; set; }
    public decimal AverageMaxPrice { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
}

public class YearlyPriceRangeStats
{
    public int Year { get; set; }
    public int Count { get; set; }
    public decimal AverageMinPrice { get; set; }
    public decimal AverageMaxPrice { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
}