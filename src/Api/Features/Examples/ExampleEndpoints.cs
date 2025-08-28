namespace Api.Features.Examples;

using Microsoft.AspNetCore.Http.HttpResults;

public static class ExampleEndpoints
{
	public static RouteGroupBuilder MapExampleEndpoints(this IEndpointRouteBuilder apiV1)
	{
		var examples = apiV1.MapGroup("/examples")
							.WithTags("Examples")
							.AddEndpointFilter(new TitleNotEmptyFilter()); // valida Create/Update

		// GET /api/v1/examples
		examples.MapGet("/", async (IExampleRepository repo) =>
				TypedResults.Ok(await repo.ListAsync()))
			.WithName("ListExamples")
			.Produces<Example[]>(StatusCodes.Status200OK);

		// GET /api/v1/examples/{id}
		examples.MapGet("/{id:guid}", async Task<Results<Ok<Example>, NotFound>> (Guid id, IExampleRepository repo) =>
		{
			var ex = await repo.GetAsync(id);

			return ex is not null ? TypedResults.Ok(ex) : TypedResults.NotFound();
		})
		.WithName("GetExampleById")
		.Produces<Example>(StatusCodes.Status200OK)
		.ProducesProblem(StatusCodes.Status404NotFound);

		// POST /api/v1/examples
		examples.MapPost("/", async Task<Results<Created<Example>, ValidationProblem>> (
			CreateExampleRequest req, IExampleRepository repo, LinkGenerator links, HttpContext http) =>
		{
			// The filter has already validated Title, here normal business flow
			try
			{
				var created = await repo.AddAsync(req.Title);
				var location = links.GetPathByName(http, "GetExampleById", new { id = created.Id });

				return TypedResults.Created(location!, created);
			}
			catch (InvalidOperationException ex)
			{
				return TypedResults.ValidationProblem(new Dictionary<string, string[]>
				{
					["title"] = new[] { ex.Message }
				});
			}
		})
		.WithName("CreateExample")
		.Accepts<CreateExampleRequest>("application/json")
		.Produces<Example>(StatusCodes.Status201Created)
		.ProducesValidationProblem();

		// PUT /api/v1/examples/{id}
		examples.MapPut("/{id:guid}", async Task<Results<NoContent, NotFound, ValidationProblem>> (
			Guid id, UpdateExampleRequest req, IExampleRepository repo) =>
		{
			try
			{
				var ok = await repo.UpdateAsync(id, req.Title, req.Done);

				return ok ? TypedResults.NoContent() : TypedResults.NotFound();
			}
			catch (InvalidOperationException ex)
			{
				return TypedResults.ValidationProblem(new Dictionary<string, string[]>
				{
					["title"] = new[] { ex.Message }
				});
			}
		})
		.WithName("UpdateExample")
		.Accepts<UpdateExampleRequest>("application/json")
		.Produces(StatusCodes.Status204NoContent)
		.ProducesProblem(StatusCodes.Status404NotFound)
		.ProducesValidationProblem();

		// DELETE /api/v1/examples/{id}
		examples.MapDelete("/{id:guid}", async Task<Results<NoContent, NotFound>> (
			Guid id, IExampleRepository repo) =>
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