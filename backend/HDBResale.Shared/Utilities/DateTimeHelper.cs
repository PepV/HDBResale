
using System.Globalization;


namespace HDBResale.Shared.Utilities;

public static class DateTimeHelper
{
    public static DateTime ParseTransactionDate(string dateString)
    {
        if (DateTime.TryParse(dateString, out var date))
            return date;
       
        if (DateTime.TryParseExact(dateString, "MM-yyyy", 
            CultureInfo.InvariantCulture, 
            DateTimeStyles.None, out var parsedDate))
        {
            return parsedDate;
        }
        
        return DateTime.MinValue;
    }
}
