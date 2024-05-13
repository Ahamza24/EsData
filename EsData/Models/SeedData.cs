using EsData.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EsData.Models
{
    public static class SeedData
    {
        public static void Initialise(IServiceProvider serviceProvider)
        {
            using (var context = new EsDataContext(
                serviceProvider.GetRequiredService<
                    DbContextOptions<EsDataContext>>()))
            {
                if (!context.Brands.Any())
                {
                    context.Brands.AddRange(

                        new Brands
                        {
                            BrandName = "Fanta",
                            EthicalScore = 2,
                            Link1 = "https://www.coca-colacompany.com/company",
                            Link2 = "https://www.ethicalconsumer.org/food-drink/shopping-guide/soft-drinks"
                        },

                        new Brands
                        {
                            BrandName = "Sharp",
                            EthicalScore = 6,
                            Link1 = "https://guide.ethical.org.au/company/?company=4137",
                            Link2 = "https://global.sharp/corporate/eco/csr_management/"
                        },

                        new Brands
                        {
                            BrandName = "Apple",
                            EthicalScore = 3,
                            Link1 = "https://www.apple.com/compliance/",
                            Link2 = "https://www.ethicalconsumer.org/company-profile/apple-inc"
                        }
                    );                   
                }
                context.SaveChanges();
            }
        }
    }
}
