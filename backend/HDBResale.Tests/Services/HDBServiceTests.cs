using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;
using FluentAssertions;
using HDBResale.Application.Services;
using HDBResale.Shared.Configuration;

namespace HDBResale.Tests.Services;

public class HDBServiceTests
{
    private readonly Mock<ILogger<HDBService>> _loggerMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly IOptions<DataGovApiSettings> _apiSettings;

    public HDBServiceTests()
    {
        _loggerMock = new Mock<ILogger<HDBService>>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        
        var settings = new DataGovApiSettings
        {
            BaseUrl = "https://data.gov.sg/api/action",
            ResourceIds = new DataGovResourceIds
            {
                ResaleFlatPrices = "d_ebc5ab87086db484f88045b47411ebc5",
                PriceRangeOffered = "d_2d493bdcc1d9a44828b6e71cb095b88d",
                PropertyInformation = "d_17f5382f26140b1fdae0ba2ef6239d2f"
            },
            TimeoutSeconds = 30
        };
        _apiSettings = Options.Create(settings);
    }

    private HDBService CreateServiceWithMockHttpClient(string jsonResponse)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        return new HDBService(
            _httpClientFactoryMock.Object,
            new MemoryCache(new MemoryCacheOptions()),
            _loggerMock.Object,
            _apiSettings);
    }

    private string GetMockPriceRangeResponse()
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
                    },
                    {
                        ""_id"": 2,
                        ""financial_year"": ""2023"",
                        ""town"": ""BEDOK"",
                        ""room_type"": ""3 ROOM"",
                        ""min_selling_price"": ""280000"",
                        ""max_selling_price"": ""450000""
                    }
                ],
                ""total"": 2
            }
        }";
    }

    [Fact]
    public async Task GetResalePricesAsync_ShouldReturnData_WhenApiCallSucceeds()
    {
        // Arrange
        var service = CreateServiceWithMockHttpClient(GetMockPriceRangeResponse());

        // Act
        var result = await service.GetResalePricesAsync(limit: 10);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetResalePricesAsync_ShouldReturnEmpty_WhenApiReturnsNoRecords()
    {
        // Arrange
        var emptyResponse = @"{""success"": true, ""result"": { ""records"": [] } }";
        var service = CreateServiceWithMockHttpClient(emptyResponse);

        // Act
        var result = await service.GetResalePricesAsync(limit: 10);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetResalePricesAsync_ShouldFilterByTown()
    {
        // Arrange
        var service = CreateServiceWithMockHttpClient(GetMockPriceRangeResponse());

        // Act
        var result = await service.GetResalePricesAsync(town: "ANG MO KIO", limit: 10);

        // Assert
        result.Should().NotBeEmpty();
        result.All(r => r.Town.Contains("ANG MO KIO")).Should().BeTrue();
    }

    [Fact]
    public async Task GetStatisticsAsync_ShouldReturnStatistics_WhenDataExists()
    {
        // Arrange
        var service = CreateServiceWithMockHttpClient(GetMockPriceRangeResponse());

        // Act
        var result = await service.GetStatisticsAsync();

        // Assert
        result.Should().NotBeNull();
        result.TotalTransactions.Should().BeGreaterThan(0);
        result.AveragePrice.Should().BeGreaterThan(0);
        result.MinPrice.Should().BeGreaterThan(0);
        result.MaxPrice.Should().BeGreaterThan(0);
    }


    [Fact]
    public async Task GetPriceRangesAsync_ShouldReturnData_WhenApiCallSucceeds()
    {
        // Arrange
        var service = CreateServiceWithMockHttpClient(GetMockPriceRangeResponse());

        // Act
        var result = await service.GetPriceRangesAsync(limit: 10);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetPriceRangesAsync_ShouldFilterByTown()
    {
        // Arrange
        var service = CreateServiceWithMockHttpClient(GetMockPriceRangeResponse());

        // Act
        var result = await service.GetPriceRangesAsync(town: "ANG MO KIO", limit: 10);

        // Assert
        result.Should().NotBeEmpty();
        result.All(r => r.Town.Contains("ANG MO KIO")).Should().BeTrue();
    }

    [Fact]
    public async Task GetPriceRangeStatisticsAsync_ShouldReturnStatistics()
    {
        // Arrange
        var service = CreateServiceWithMockHttpClient(GetMockPriceRangeResponse());

        // Act
        var result = await service.GetPriceRangeStatisticsAsync();

        // Assert
        result.Should().NotBeNull();
        result.PriceRanges.Should().NotBeEmpty();
        result.TownStats.Should().NotBeEmpty();
        result.RoomTypeStats.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetTownsAsync_ShouldReturnDistinctTowns()
    {
        // Arrange
        var service = CreateServiceWithMockHttpClient(GetMockPriceRangeResponse());

        // Act
        var result = await service.GetTownsAsync();

        // Assert
        result.Should().NotBeEmpty();
        result.Should().Contain("ANG MO KIO");
        result.Should().Contain("BEDOK");
    }

    [Fact]
    public async Task GetFlatTypesAsync_ShouldReturnDistinctFlatTypes()
    {
        // Arrange
        var service = CreateServiceWithMockHttpClient(GetMockPriceRangeResponse());

        // Act
        var result = await service.GetFlatTypesAsync();

        // Assert
        result.Should().NotBeEmpty();
        result.Should().Contain("4 ROOM");
        result.Should().Contain("3 ROOM");
    }
}