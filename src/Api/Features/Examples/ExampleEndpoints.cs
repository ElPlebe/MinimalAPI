namespace Api.Features.Examples;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

public static class ExampleEndpoints
{
	/// <summary>Maps Example feature endpoints into the provided route group (e.g., /api/v1).</summary>
	public static RouteGroupBuilder MapExampleEndpoints(this IEndpointRouteBuilder apiV1)
	{
		var examples = apiV1.MapGroup("/examples").WithTags("Examples");

		// GET /api/v1/examples
		examples.MapGet("/", async (IExampleRepository repo) =>
			Results.Ok(await repo.ListAsync()))
			.WithName("ListExamples")
			.Produces<Example[]>(StatusCodes.Status200OK);

		// GET /api/v1/examples/{id}
		examples.MapGet("/{id:guid}", async Task<Results<Ok<Example>, NotFound>> (Guid id, IExampleRepository repo) =>
		{
			var ex = await repo.GetAsync(id);
			return ex is null ? TypedResults.NotFound() : TypedResults.Ok(ex);
		})
		.WithName("GetExample")
		.Produces<Example>(StatusCodes.Status200OK)
		.ProducesProblem(StatusCodes.Status404NotFound);

		// POST /api/v1/examples
		examples.MapPost("/", async Task<Results<Created<Example>, ValidationProblem>> (
			CreateExampleRequest req, IExampleRepository repo) =>
		{
			var created = await repo.AddAsync(req.Title.Trim());
			return TypedResults.Created($"/api/v1/examples/{created.Id}", created);
		})
		.AddEndpointFilter(new TitleNotEmptyFilter())
		.WithName("CreateExample")
		.Produces<Example>(StatusCodes.Status201Created)
		.ProducesValidationProblem();

		// PUT /api/v1/examples/{id}
		examples.MapPut("/{id:guid}", async Task<Results<NoContent, NotFound, ValidationProblem>> (
			Guid id, UpdateExampleRequest req, IExampleRepository repo) =>
		{
			var updated = await repo.UpdateAsync(id, req.Title.Trim(), req.Done);
			return updated ? TypedResults.NoContent() : TypedResults.NotFound();
		})
		.AddEndpointFilter(new TitleNotEmptyFilter())
		.WithName("UpdateExample")
		.Produces(StatusCodes.Status204NoContent)
		.ProducesProblem(StatusCodes.Status404NotFound)
		.ProducesValidationProblem();

		// DELETE /api/v1/examples/{id}
		examples.MapDelete("/{id:guid}", async Task<Results<NoContent, NotFound>> (Guid id, IExampleRepository repo) =>
		{
			var removed = await repo.DeleteAsync(id);
			return removed ? TypedResults.NoContent() : TypedResults.NotFound();
		})
		.WithName("DeleteExample")
		.Produces(StatusCodes.Status204NoContent)
		.ProducesProblem(StatusCodes.Status404NotFound);

		return examples;
	}
}
