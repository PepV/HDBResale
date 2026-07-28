using HDBResale.Application.DTOs;
using HDBResale.Domain.Entities;

namespace HDBResale.Tests.Helpers;

public static class TestDataHelper
{
    public static List<PriceRangeDto> GetSamplePriceRanges()
    {
        return new List<PriceRangeDto>
        {
            new PriceRangeDto
            {
                Id = 1,
                FinancialYear = 2023,
                Town = "ANG MO KIO",
                RoomType = "4 ROOM",
                MinSellingPrice = 350000,
                MaxSellingPrice = 650000
            },
            new PriceRangeDto
            {
                Id = 2,
                FinancialYear = 2023,
                Town = "BEDOK",
                RoomType = "3 ROOM",
                MinSellingPrice = 280000,
                MaxSellingPrice = 450000
            },
            new PriceRangeDto
            {
                Id = 3,
                FinancialYear = 2023,
                Town = "TAMPINES",
                RoomType = "5 ROOM",
                MinSellingPrice = 450000,
                MaxSellingPrice = 750000
            }
        };
    }

    public static List<ResaleFlatDto> GetSampleResaleFlats()
    {
        return new List<ResaleFlatDto>
        {
            new ResaleFlatDto
            {
                Town = "ANG MO KIO",
                FlatType = "4 ROOM",
                Block = "123",
                StreetName = "ANG MO KIO AVE 3",
                ResalePrice = 480000,
                TransactionDate = new DateTime(2023, 1, 15),
                MinPrice = 350000,
                MaxPrice = 650000
            },
            new ResaleFlatDto
            {
                Town = "BEDOK",
                FlatType = "3 ROOM",
                Block = "456",
                StreetName = "BEDOK NORTH ST 1",
                ResalePrice = 350000,
                TransactionDate = new DateTime(2023, 2, 20),
                MinPrice = 280000,
                MaxPrice = 450000
            }
        };
    }

    public static string GetMockApiResponse()
    {
        return @"{
            ""success"": true,
            ""result"": {
                ""records"": [
                    {
                        ""_id"": 1,
                        ""financial_year"": ""2023"",
                        ""town"": ""ANG MO KIO"",
                        ""room_type"": ""4 ROOM"",
                        ""min_selling_price"": ""350000"",
                        ""max_selling_price"": ""650000""
                    }
                ],
                ""total"": 1
            }
        }";
    }
}