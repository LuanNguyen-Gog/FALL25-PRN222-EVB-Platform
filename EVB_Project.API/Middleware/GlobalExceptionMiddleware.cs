// Api/Middleware/GlobalExceptionMiddleware.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Services.Exceptions;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;

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
            if (ctx.Response.HasStarted) return;

            var traceId = ctx.TraceIdentifier;
            var (status, title, detail) = (StatusCodes.Status500InternalServerError,
                                           "Internal Server Error",
                                           _env.IsDevelopment() ? ex.Message : "An unexpected error occurred.");

            // --- Nhận diện lỗi tạm thời hạ tầng (DNS/Socket/Pool/Timeout) → 503 ---
            bool IsTransientInfra(Exception e)
            {
                // Socket / DNS
                if (e is SocketException se)
                    return se.SocketErrorCode is SocketError.TryAgain or SocketError.WouldBlock
                                                or SocketError.TimedOut or SocketError.HostNotFound
                                                or SocketError.NetworkDown or SocketError.NetworkUnreachable;

                // Npgsql (pool/timeout/network)
                if (e is NpgsqlException) return true;
                if (e is TimeoutException) return true;
                if (e.InnerException != null) return IsTransientInfra(e.InnerException);

                return false;
            }

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
                    break;

                case NotFoundException:
                    status = StatusCodes.Status404NotFound;
                    title = "Not Found";
                    break;

                case ConflictException:
                    status = StatusCodes.Status409Conflict;
                    title = "Conflict";
                    break;

                default:
                    if (IsTransientInfra(ex))
                    {
                        status = StatusCodes.Status503ServiceUnavailable;
                        title = "Service Unavailable";
                        // Gợi ý người dùng/FE retry sau vài giây
                    }
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

            problem.Extensions["traceId"] = traceId;

            ctx.Response.ContentType = "application/problem+json";
            ctx.Response.StatusCode = status;
            await ctx.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOpt));
        }
    }
}
