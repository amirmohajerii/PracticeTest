

// The collection definition ensures that the test fixture (and the database container)
// is only started once for all tests in this file.
using Application.Interfaces;
using Infrastracture.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Testcontainers.MsSql;
using WebApi;

public class TestFixture : WebApplicationFactory<IApiMarker>, IAsyncLifetime
{
    private readonly MsSqlContainer _msSqlContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("YourStrong@Password!")
        .Build();

    public ICachingService FakeRedisCache { get; private set; }

    public Task InitializeAsync()
    {
        return _msSqlContainer.StartAsync();
    }

    public new Task DisposeAsync()
    {
        return _msSqlContainer.DisposeAsync().AsTask();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));
            services.RemoveAll(typeof(ApplicationDbContext));

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(_msSqlContainer.GetConnectionString(), sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure();
                });
            });

            var fakeRedisCache = Substitute.For<ICachingService>();
            services.RemoveAll(typeof(ICachingService));
            services.AddSingleton(fakeRedisCache);
            FakeRedisCache = fakeRedisCache;

            var sp = services.BuildServiceProvider();
            using (var scope = sp.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var dbContext = scopedServices.GetRequiredService<ApplicationDbContext>();

                dbContext.Database.Migrate();
            }
        });
    }
}
