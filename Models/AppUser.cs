using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Bogus.DataSets;
using Microsoft.AspNetCore.Identity;

namespace App.Models
{
    public class AppUser: IdentityUser
    {
        [Column(TypeName ="nvarchar")]
        [StringLength(400)]
        public string? HomeAdress {get;set;}
        [DataType(DataType.Date)]
        public DateTime? BirthDate {get;set;}

    }
}
//dotnet aspnet-codegenerator identity -dc cs58.models.MyBlogContext