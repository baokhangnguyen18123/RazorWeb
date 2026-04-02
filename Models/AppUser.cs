using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace CS58.Models
{
    public class AppUser: IdentityUser
    {
        [Column(TypeName ="nvarchar")]
        [StringLength(400)]
        
        public string? HomeAdress {get;set;}

    }
}
//dotnet aspnet-codegenerator identity -dc cs58.models.MyBlogContext