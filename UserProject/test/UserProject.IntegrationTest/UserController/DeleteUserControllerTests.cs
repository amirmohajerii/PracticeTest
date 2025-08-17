using Application.Dtos;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace UserProject.IntegrationTest.UserController
{
    [Collection("IntegrationTests")]
    public class DeleteUserControllerTests
    {
        private readonly HttpClient _httpClient;
        private readonly TestFixture _fixture;

        public DeleteUserControllerTests(TestFixture fixture)
        {
            _fixture = fixture;
            _httpClient = _fixture.CreateClient();
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
}