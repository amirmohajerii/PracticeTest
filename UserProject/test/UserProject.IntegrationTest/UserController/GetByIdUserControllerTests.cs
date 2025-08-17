using Application.Dtos;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace UserProject.IntegrationTest.UserController
{
    [Collection("IntegrationTests")]
    public class GetByIdUserControllerTests
    {
        private readonly HttpClient _httpClient;
        private readonly TestFixture _fixture;

        public GetByIdUserControllerTests(TestFixture fixture)
        {
            _fixture = fixture;
            _httpClient = _fixture.CreateClient();
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
    }
}

