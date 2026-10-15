using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RaceDay.Data;

namespace RaceDay.Api.Tests
{
    public class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _dbName = "TestDb_" + Guid.NewGuid();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Remove everything EF Core registered for the original DbContext.
                // In EF Core 9+, AddDbContext also registers
                // IDbContextOptionsConfiguration<TContext>, which must be removed too.
                services.RemoveAll<DbContextOptions<RaceDayDbContext>>();
                services.RemoveAll<DbContextOptions>();
                services.RemoveAll<IDbContextOptionsConfiguration<RaceDayDbContext>>();
                services.RemoveAll<RaceDayDbContext>();

                // Re-add the context backed by an isolated in-memory database
                services.AddDbContext<RaceDayDbContext>(options =>
                    options.UseInMemoryDatabase(_dbName));
            });
        }
    }
}