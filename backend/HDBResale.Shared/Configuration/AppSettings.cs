namespace HDBResale.Shared.Configuration;

public class JwtSettings
{
    public string Key { get; set; } = "YourSuperSecretKeyThatIsAtLeast32CharactersLongAndSecure!";
    public string Issuer { get; set; } = "HDBResaleAPI";
    public string Audience { get; set; } = "HDBResaleClient";
    public int ExpiryInMinutes { get; set; } = 60;
}

public class DataGovApiSettings
{
    public string BaseUrl { get; set; } = "https://data.gov.sg/api/action";
    public DataGovResourceIds ResourceIds { get; set; } = new();
    public int TimeoutSeconds { get; set; } = 60;
}

public class DataGovResourceIds
{
    public string ResaleFlatPrices { get; set; } = "d_ebc5ab87086db484f88045b47411ebc5";
    public string ResaleFlatPrices2015 { get; set; } = "d_ea9ed51da2787afaf8e51f827c304208";
    public string ResaleFlatPrices2017 { get; set; } = "d_8b84c4ee58e3cfc0ece0d773c8ca6abc";
    public string PropertyInformation { get; set; } = "d_17f5382f26140b1fdae0ba2ef6239d2f";
    public string PriceRangeOffered { get; set; } = "d_2d493bdcc1d9a44828b6e71cb095b88d";
    public string CEASalesperson { get; set; } = "d_07c63be0f37e6e59c07a4ddc2fd87fcb";
}

public class CorsSettings
{
    public string[] AllowedOrigins { get; set; } = new[] { 
        "http://localhost:5173", 
        "http://localhost:3000", 
        "http://localhost:5000" 
    };
}