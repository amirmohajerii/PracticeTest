using Application.Dtos;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace UserProject.IntegrationTest.UserController
{
    [Collection("IntegrationTests")]
    public class GetAllUserControllerTests
    {
        private readonly HttpClient _httpClient;
        private readonly TestFixture _fixture;

        public GetAllUserControllerTests(TestFixture fixture)
        {
            _fixture = fixture;
            _httpClient = _fixture.CreateClient();
        }

        [Fact]
        public async Task GetAll_ReturnsAllUsers_WhenUsersExist()
        {
            // Arrange
            await _httpClient.PostAsync("/api/Users", new StringContent(JsonSerializer.Serialize(FakeUserGenerator.Generate()), Encoding.UTF8, "application/json"));
            await _httpClient.PostAsync("/api/Users", new StringContent(JsonSerializer.Serialize(FakeUserGenerator.Generate()), Encoding.UTF8, "application/json"));
            await _httpClient.PostAsync("/api/Users", new StringContent(JsonSerializer.Serialize(FakeUserGenerator.Generate()), Encoding.UTF8, "application/json"));

            // Act
            var getResponse = await _httpClient.GetAsync("/api/Users");

            // Assert
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var users = await getResponse.Content.ReadFromJsonAsync<IEnumerable<UserDto>>();
            users.Should().NotBeNullOrEmpty();
            users.Should().HaveCount(3);
        }
    }
}
