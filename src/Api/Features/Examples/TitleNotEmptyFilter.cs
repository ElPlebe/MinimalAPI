namespace Api.Features.Examples;

using Microsoft.AspNetCore.Http;

/// <summary>
/// Basic endpoint filter to reject empty titles consistently across endpoints.
/// </summary>
public sealed class TitleNotEmptyFilter : IEndpointFilter
{
	public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext ctx, EndpointFilterDelegate next)
	{
		if (ctx.Arguments.FirstOrDefault(a => a is CreateExampleRequest or UpdateExampleRequest) is { } body)
		{
			var title = body switch
			{
				CreateExampleRequest c => c.Title,
				UpdateExampleRequest u => u.Title,
				_ => ""
			};

			if (string.IsNullOrWhiteSpace(title))
			{
				return Results.ValidationProblem(new Dictionary<string, string[]>
				{
					["title"] = new[] { "Title is required." }
				});
			}
		}

		return await next(ctx);
	}
}
