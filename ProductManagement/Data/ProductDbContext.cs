using Microsoft.EntityFrameworkCore;
using ProductManagement.Models;

namespace ProductManagement.Data
{
    public class ProductDbContext : DbContext
    {
        //docker terminal command to host this db
        // docker run -e 'ACCEPT_EULA=Y' -e 'MSSQL_SA_PASSWORD=P@ssword123' -p 1401:1433 --name sqlserver-containerForProducts -d mcr.microsoft.com/mssql/server:2022-latest
        public ProductDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<Product> Products { get; set; }
    }
}
