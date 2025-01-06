using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UserManagement.Domain.Entities;

namespace UserManagement.Infrastructure.Databases
{
    public class UserDbContext : IdentityDbContext<User>
    {
        /*
         * I used this command for docker to host db on docker
         * docker run -e 'ACCEPT_EULA=Y' -e 'MSSQL_SA_PASSWORD=P@ssword123' -p 1400:1433 --name sqlserver-container2 -d mcr.microsoft.com/mssql/server:2022-latest

         * 
         * https://www.youtube.com/watch?v=UT9l_UfhexE
         * 
         */
        public UserDbContext(DbContextOptions<UserDbContext> options):base(options)
        {
            
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>()
                .Property(u => u.IsDeleted)
                .HasDefaultValue(false); // Database default value of IsDeleted is false
        
        }

    }
}
