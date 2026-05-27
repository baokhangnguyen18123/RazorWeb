using Microsoft.AspNetCore.Authorization;

namespace App.Security.Requirements;
public class ArticleUpdateRequirement : IAuthorizationRequirement
{


    public int Year { get; set; }
    public int Month { get; set; }
    public int Day { get; set; }
    public ArticleUpdateRequirement(int year = 2021, int month = 6, int day = 1)
    {
        Year = year;
        Month = month;
        Day = day;
    }

} 