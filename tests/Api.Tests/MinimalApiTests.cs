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
    public async Task Create_Returns_201_And_Location()
    {
        var create = await _client.PostAsJsonAsync(BaseRoute, new { title = "First item" });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        var location = create.Headers.Location;
        Assert.NotNull(location);

        var entity = await _client.GetFromJsonAsync<ExampleDto>(location);
        Assert.NotNull(entity);
        Assert.Equal("First item", entity!.Title);
        Assert.False(entity.Done);
    }

    [Fact]
    public async Task Create_Empty_Title_Returns_400()
    {
		// It is intercepted by the EndpointFilter and returns ValidationProblem (400)
		var create = await _client.PostAsJsonAsync(BaseRoute, new { title = "" });
        Assert.Equal(HttpStatusCode.BadRequest, create.StatusCode);
    }

    [Fact]
    public async Task Create_Duplicate_Title_Returns_409()
    {
        var first = await _client.PostAsJsonAsync(BaseRoute, new { title = "Duplicated" });
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var dup = await _client.PostAsJsonAsync(BaseRoute, new { title = "Duplicated" });
        Assert.Equal(HttpStatusCode.Conflict, dup.StatusCode);
    }

    [Fact]
    public async Task Get_NonExisting_Returns_404()
    {
        var res = await _client.GetAsync($"{BaseRoute}/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task Update_NonExisting_Returns_404()
    {
        var id = Guid.NewGuid();
        var update = await _client.PutAsJsonAsync($"{BaseRoute}/{id}", new { title = "X", done = true });
        Assert.Equal(HttpStatusCode.NotFound, update.StatusCode);
    }

    [Fact]
    public async Task Update_Empty_Title_Returns_400()
    {
		// It is intercepted by the EndpointFilter (ValidationProblem 400)
		var create = await _client.PostAsJsonAsync(BaseRoute, new { title = "To Update" });
        var location = create.Headers.Location!;
        var entity = await _client.GetFromJsonAsync<ExampleDto>(location);

        var update = await _client.PutAsJsonAsync($"{BaseRoute}/{entity!.Id}", new { title = "", done = true });
        Assert.Equal(HttpStatusCode.BadRequest, update.StatusCode);
    }

    [Fact]
    public async Task Update_Duplicate_Title_Returns_409()
    {
        var a = await _client.PostAsJsonAsync(BaseRoute, new { title = "A" });
        var b = await _client.PostAsJsonAsync(BaseRoute, new { title = "B" });

        var locationB = b.Headers.Location!;
        var entityB = await _client.GetFromJsonAsync<ExampleDto>(locationB);

		// Attempt to change B to “A” → 409
		var update = await _client.PutAsJsonAsync($"{BaseRoute}/{entityB!.Id}", new { title = "A", done = false });
        Assert.Equal(HttpStatusCode.Conflict, update.StatusCode);
    }

	[Fact]
	public async Task Delete_Flow_Creates_Deletes_Then_404()
	{
		var create = await _client.PostAsJsonAsync(BaseRoute, new { title = "To Delete" });
		Assert.Equal(HttpStatusCode.Created, create.StatusCode);

		// Get the entity to obtain the ID with certainty.
		var location = create.Headers.Location!;
		var entity = await _client.GetFromJsonAsync<ExampleDto>(location);
		Assert.NotNull(entity);

		// Build the DELETE URL explicitly
		var del = await _client.DeleteAsync($"{BaseRoute}/{entity!.Id}");
		Assert.Equal(HttpStatusCode.NoContent, del.StatusCode);

		var after = await _client.GetAsync($"{BaseRoute}/{entity.Id}");
		Assert.Equal(HttpStatusCode.NotFound, after.StatusCode);
	}

	private sealed record ExampleDto(Guid Id, string Title, bool Done);
}
