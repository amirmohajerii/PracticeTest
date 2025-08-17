

// The collection definition ensures that the test fixture (and the database container)
// is only started once for all tests in this file.
using Application.Dtos;
using Application.Features.Users.Commands.Create;
using Application.Interfaces;
using Bogus;
using FluentAssertions;
using Infrastracture.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Testcontainers.MsSql;
using WebApi;

[CollectionDefinition("IntegrationTests")]
public class IntegrationTestCollection : ICollectionFixture<TestFixture>
{
    // This class is just a placeholder and doesn't need any code.
    // It links the test collection to the TestFixture.
}

/// <summary>
/// This is the core fixture for all integration tests. It manages the lifecycle of the
/// test host and the SQL Server container.
/// </summary>
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
            // First, remove the existing DbContext registration.
            services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));
            services.RemoveAll(typeof(ApplicationDbContext));

            // Add a new DbContext registration to use the Testcontainers SQL Server.
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(_msSqlContainer.GetConnectionString(), sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure();
                });
            });

            // Mock the caching service using NSubstitute to avoid external dependencies.
            // We'll use a simple in-memory cache for the test.
            var fakeRedisCache = Substitute.For<ICachingService>();
            services.RemoveAll(typeof(ICachingService));
            services.AddSingleton(fakeRedisCache);
            FakeRedisCache = fakeRedisCache;

            // Build the service provider to run migrations.
            var sp = services.BuildServiceProvider();
            using (var scope = sp.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var dbContext = scopedServices.GetRequiredService<ApplicationDbContext>();

                // Ensure the database is created and migrations are applied.
                dbContext.Database.Migrate();
            }
        });
    }
}

/// <summary>
/// A helper class to generate fake User data using Bogus.
/// </summary>
public class FakeUserGenerator
{
    private static readonly Faker<CreateUserCommand> _faker = new Faker<CreateUserCommand>()
        .RuleFor(u => u.Name, f => f.Person.FullName)
        .RuleFor(u => u.Email, f => f.Person.Email);

    public static CreateUserCommand Generate() => _faker.Generate();
}


/// <summary>
/// The main test class that contains all the integration tests for the UsersController.
/// </summary>
[Collection("IntegrationTests")]
public class UsersControllerTests
{
    private readonly HttpClient _httpClient;
    private readonly TestFixture _fixture;

    public UsersControllerTests(TestFixture fixture)
    {
        _fixture = fixture;
        _httpClient = _fixture.CreateClient();
    }

    [Fact]
    public async Task Post_CreatesUser_ReturnsCreated()
    {
        // Arrange
        var fakeUser = FakeUserGenerator.Generate();
        var content = new StringContent(JsonSerializer.Serialize(fakeUser), Encoding.UTF8, "application/json");

        // Act
        var response = await _httpClient.PostAsync("/api/Users", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var userDto = await response.Content.ReadFromJsonAsync<UserDto>();
        userDto.Should().NotBeNull();
        userDto.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task GetById_ReturnsUser_WhenUserExists()
    {
        // Arrange
        var fakeUser = FakeUserGenerator.Generate();
        var postContent = new StringContent(JsonSerializer.Serialize(fakeUser), Encoding.UTF8, "application/json");
        var postResponse = await _httpClient.PostAsync("/api/Users", postContent);
        var createdUser = await postResponse.Content.ReadFromJsonAsync<UserDto>();

        // Act
        var getResponse = await _httpClient.GetAsync($"/api/Users/{createdUser.Id}");

        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var userDto = await getResponse.Content.ReadFromJsonAsync<UserDto>();
        userDto.Should().NotBeNull();
        userDto.Id.Should().Be(createdUser.Id);
    }

    [Fact]
    public async Task Put_UpdatesUser_ReturnsOk()
    {
        // Arrange
        var fakeUser = FakeUserGenerator.Generate();
        var postContent = new StringContent(JsonSerializer.Serialize(fakeUser), Encoding.UTF8, "application/json");
        var postResponse = await _httpClient.PostAsync("/api/Users", postContent);
        var createdUser = await postResponse.Content.ReadFromJsonAsync<UserDto>();

        var updatedName = "Updated Name";
        var updatedEmail = "updated@test.com";
        var putContent = new StringContent(
            JsonSerializer.Serialize(new { name = updatedName, email = updatedEmail }),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var putResponse = await _httpClient.PutAsync($"/api/Users/{createdUser.Id}", putContent);

        // Assert
        putResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await _httpClient.GetAsync($"/api/Users/{createdUser.Id}");
        var updatedUser = await getResponse.Content.ReadFromJsonAsync<UserDto>();

        updatedUser.Name.Should().Be(updatedName);
        updatedUser.Email.Should().Be(updatedEmail);
    }

    [Fact]
    public async Task Delete_RemovesUser_ReturnsNoContent()
    {
        // Arrange
        var fakeUser = FakeUserGenerator.Generate();
        var postContent = new StringContent(JsonSerializer.Serialize(fakeUser), Encoding.UTF8, "application/json");
        var postResponse = await _httpClient.PostAsync("/api/Users", postContent);
        var createdUser = await postResponse.Content.ReadFromJsonAsync<UserDto>();

        // Act
        var deleteResponse = await _httpClient.DeleteAsync($"/api/Users/{createdUser.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the user is no longer there.
        var getResponse = await _httpClient.GetAsync($"/api/Users/{createdUser.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}