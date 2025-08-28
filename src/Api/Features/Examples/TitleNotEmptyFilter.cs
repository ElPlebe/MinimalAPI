namespace Api.Features.Examples;

using Microsoft.AspNetCore.Http;

public sealed class TitleNotEmptyFilter : IEndpointFilter
{
	public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext ctx, EndpointFilterDelegate next)
	{
		// Find a bodysuit that is one of your entry DTOs.
		var body = ctx.Arguments.FirstOrDefault(a => a is CreateExampleRequest or UpdateExampleRequest);
		if (body is null) return await next(ctx);

		var title = body switch
		{
			CreateExampleRequest c => c.Title,
			UpdateExampleRequest u => u.Title,
			_ => null
		};

		if (string.IsNullOrWhiteSpace(title))
		{
			return Results.ValidationProblem(new Dictionary<string, string[]>
			{
				["title"] = new[] { "Title is required." }
			});
		}

		return await next(ctx);
	}
}