using Application.Dtos;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace UserProject.IntegrationTest.UserController
{
    [Collection("IntegrationTests")]
    public class CreateUserControllerTests
    {
        private readonly HttpClient _httpClient;
        private readonly TestFixture _fixture;

        public CreateUserControllerTests(TestFixture fixture)
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
    }
}