namespace HDBResale.Application.DTOs;

public class CEASalespersonDto
{
    public int Id { get; set; }
    public string? SalespersonName { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? EstateAgentName { get; set; }
    public string? EstateAgentLicenseNo { get; set; }
    public string? RegistrationStartDate { get; set; }
    public string? RegistrationEndDate { get; set; }
    public string? Status { get; set; }  // Calculated based on dates
}

public class CEASalespersonStatisticsDto
{
    public int TotalCount { get; set; }
    public int ActiveCount { get; set; }
    public Dictionary<string, int> AgencyDistribution { get; set; } = new();
}