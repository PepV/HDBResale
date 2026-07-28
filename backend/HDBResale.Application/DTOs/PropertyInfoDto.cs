namespace HDBResale.Application.DTOs;

public class PropertyInfoDto
{
    public string Block { get; set; } = "Unknown";
    public string StreetName { get; set; } = "Unknown";
    public string Town { get; set; } = "Unknown";
    public string PostalCode { get; set; } = "N/A";
    public string MaxFloorLevel { get; set; } = "Unknown";
    public int? YearCompleted { get; set; }
    public int? TotalDwellingUnits { get; set; }
    public bool? HasResidential { get; set; }
    public bool? HasCommercial { get; set; }
    public bool? HasMarketHawker { get; set; }
    public bool? HasMiscellaneous { get; set; }
    public bool? HasMultistoreyCarpark { get; set; }
    public bool? HasPrecinctPavilion { get; set; }
    public int? OneRoomSold { get; set; }
    public int? TwoRoomSold { get; set; }
    public int? ThreeRoomSold { get; set; }
    public int? FourRoomSold { get; set; }
    public int? FiveRoomSold { get; set; }
    public int? ExecSold { get; set; }
    public int? StudioApartmentSold { get; set; }
}