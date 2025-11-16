namespace Wtrfll.Server.Application.Common;

public sealed record ErrorResponse(string Error, string? Details = null);
