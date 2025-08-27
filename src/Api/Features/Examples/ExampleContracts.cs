namespace Api.Features.Examples;

using Microsoft.AspNetCore.Mvc;

public sealed record Example(Guid Id, string Title, bool Done);
public sealed record CreateExampleRequest(string Title);
public sealed record UpdateExampleRequest(string Title, bool Done);
//Change only for create a PR....
