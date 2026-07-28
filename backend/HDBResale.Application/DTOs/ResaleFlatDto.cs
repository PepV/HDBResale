namespace HDBResale.Application.DTOs;

public class ResaleFlatDto
{
    public string Town { get; set; } = "Unknown";
    public string FlatType { get; set; } = "Unknown";
    public string Block { get; set; } = "Unknown";
    public string StreetName { get; set; } = "Unknown";
    public int StoreyRange { get; set; }
    public int FloorAreaSqm { get; set; }
    public decimal ResalePrice { get; set; }
    public int LeaseRemainYear { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal MinPrice { get; set; }  // <-- Make sure this exists
    public decimal MaxPrice { get; set; }  // <-- Make sure this exists
}