using Application.Dtos;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace UserProject.IntegrationTest.UserController
{
    [Collection("IntegrationTests")]
    public class UpdateUserControllerTests
    {
        private readonly HttpClient _httpClient;
        private readonly TestFixture _fixture;

        public UpdateUserControllerTests(TestFixture fixture)
        {
            _fixture = fixture;
            _httpClient = _fixture.CreateClient();
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

    }
}