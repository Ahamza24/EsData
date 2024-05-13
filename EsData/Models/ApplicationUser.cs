using Microsoft.AspNetCore.Identity;

namespace EsData.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Firstname { get; set; }
        public string Surename { get; set; }
    }
}
