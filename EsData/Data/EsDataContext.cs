using Microsoft.EntityFrameworkCore;
using EsData.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace EsData.Data
{
    public class EsDataContext : IdentityDbContext<ApplicationUser>
    {
        public EsDataContext(DbContextOptions<EsDataContext> options)
            : base(options)
        {
        }

        public DbSet<EsData.Models.Brands> Brands { get; set; }

        public DbSet<EsData.Models.ImageFile> ImageFile { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //modelBuilder.Seed();
        }


    }

}
