namespace HDBResale.Application.DTOs;

public class PropertyListDto
{
    public int Id { get; set; }
    public string Block { get; set; } = "Unknown";
    public string Street { get; set; } = "Unknown";
    public string Town { get; set; } = "Unknown";
    public string MaxFloorLevel { get; set; } = "Unknown";
    public string YearCompleted { get; set; } = "Unknown";
    public string Residential { get; set; } = "N";
    public string Commercial { get; set; } = "N";
    public string MarketHawker { get; set; } = "N";
    public string Miscellaneous { get; set; } = "N";
    public string MultistoreyCarpark { get; set; } = "N";
    public string PrecinctPavilion { get; set; } = "N";
    public string TotalDwellingUnits { get; set; } = "0";
    public string OneRoomSold { get; set; } = "0";
    public string TwoRoomSold { get; set; } = "0";
    public string ThreeRoomSold { get; set; } = "0";
    public string FourRoomSold { get; set; } = "0";
    public string FiveRoomSold { get; set; } = "0";
    public string ExecSold { get; set; } = "0";
    public string StudioApartmentSold { get; set; } = "0";
}