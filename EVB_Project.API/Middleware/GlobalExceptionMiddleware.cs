// Api/Middleware/GlobalExceptionMiddleware.cs
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Services.Exceptions;

namespace EVB_Project.API.Middleware
{
    public sealed class GlobalExceptionMiddleware : IMiddleware
    {
        private readonly IHostEnvironment _env;
        private static readonly JsonSerializerOptions JsonOpt = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public GlobalExceptionMiddleware(IHostEnvironment env) => _env = env;

        public async Task InvokeAsync(HttpContext ctx, RequestDelegate next)
        {
            try
            {
                await next(ctx);
            }
            catch (Exception ex)
            {
                await HandleAsync(ctx, ex);
            }
        }

        private async Task HandleAsync(HttpContext ctx, Exception ex)
        {
            if (ctx.Response.HasStarted)
                return;

            var traceId = ctx.TraceIdentifier;
            var status = StatusCodes.Status500InternalServerError;
            var title = "Internal Server Error";
            string? detail = _env.IsDevelopment() ? ex.Message : "An unexpected error occurred.";
            object? extensions = new { traceId };

            switch (ex)
            {
                case UnauthorizedAccessException:
                case SecurityTokenException:
                    status = StatusCodes.Status401Unauthorized;
                    title = "Unauthorized";
                    detail = _env.IsDevelopment() ? ex.Message : "Unauthorized";
                    break;
                case ValidationAppException vex:
                    status = StatusCodes.Status422UnprocessableEntity;
                    title = "Validation Failed";
                    detail = vex.Message;
                    extensions = new { traceId, errors = vex.Errors };
                    break;
                case NotFoundException:
                    status = StatusCodes.Status404NotFound;
                    title = "Not Found";
                    break;
                case ConflictException:
                    status = StatusCodes.Status409Conflict;
                    title = "Conflict";
                    break;
            }

            var problem = new ProblemDetails
            {
                Title = title,
                Status = status,
                Detail = detail,
                Type = "about:blank",
                Instance = ctx.Request.Path
            };

            var extJson = JsonSerializer.Serialize(extensions, JsonOpt);
            var extDict = JsonSerializer.Deserialize<Dictionary<string, object>>(extJson, JsonOpt)!;
            foreach (var kv in extDict) problem.Extensions[kv.Key] = kv.Value;

            ctx.Response.ContentType = "application/problem+json";
            ctx.Response.StatusCode = status;
            await ctx.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOpt));
        }
    }
}
