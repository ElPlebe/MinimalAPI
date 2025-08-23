// tests/Api.Tests/MinimalApiTests.cs
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Api.Tests;

public class MinimalApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
	private const string BaseRoute = "/api/v1/examples";

	public MinimalApiTests(WebApplicationFactory<Program> factory)
        => _client = factory.CreateClient();

    [Fact]
    public async Task Create_Then_Get_Todo_Succeeds()
    {
        var create = await _client.PostAsJsonAsync(BaseRoute, new { title = "Buy milk" });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        var location = create.Headers.Location!;
        var todo = await _client.GetFromJsonAsync<Exampl>(location);
        Assert.NotNull(todo);
        Assert.Equal("Buy milk", todo!.Title);
        Assert.False(todo.Done);
    }

	[Fact]
	public async Task Create_With_Empty_Title_Returns_400()
	{
		var create = await _client.PostAsJsonAsync(BaseRoute, new { title = "" });
		Assert.Equal(HttpStatusCode.BadRequest, create.StatusCode);
	}

	[Fact]
	public async Task Update_NonExisting_Returns_404()
	{
		var nonExistingId = Guid.NewGuid();
		var update = await _client.PutAsJsonAsync($"{BaseRoute}/{nonExistingId}", new { title = "X", done = true });
		Assert.Equal(HttpStatusCode.NotFound, update.StatusCode);
	}

	[Fact]
	public async Task Delete_Flow_Works()
	{
		var create = await _client.PostAsJsonAsync(BaseRoute, new { title = "To Delete" });
		var location = create.Headers.Location!;
		var del = await _client.DeleteAsync(location);
		Assert.Equal(HttpStatusCode.NoContent, del.StatusCode);

		var getAfter = await _client.GetAsync(location);
		Assert.Equal(HttpStatusCode.NotFound, getAfter.StatusCode);
	}

	private sealed record EntityDto(Guid Id, string Title, bool Done);
}
