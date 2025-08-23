namespace Api.Features.Examples;

using Microsoft.AspNetCore.Mvc;

public sealed record Example(Guid Id, string Title, bool Done);
public sealed record CreateExampleRequest([property: FromBody] string Title);
public sealed record UpdateExampleRequest([property: FromBody] string Title, [property: FromBody] bool Done);

//Change only for create a PR....
