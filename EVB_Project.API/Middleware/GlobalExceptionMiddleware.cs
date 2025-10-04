// Api/Middleware/GlobalExceptionMiddleware.cs
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Services.Exceptions;
using Npgsql;

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
            var traceId = ctx.TraceIdentifier;

            var status = StatusCodes.Status500InternalServerError;
            var title = "Internal Server Error";
            string? detail = _env.IsDevelopment() ? ex.Message : "An unexpected error occurred.";
            object? extensions = new { traceId };

            switch (ex)
            {
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

                case DbUpdateException dbex when IsUniqueViolation(dbex):
                    status = StatusCodes.Status409Conflict;
                    title = "Conflict";
                    detail = "A resource with the same unique value already exists.";
                    break;

                // có thể bổ sung Unauthorized/Forbidden… nếu cần
                //case DbUpdateException dbex when dbex.InnerException is PostgresException pex:
                //    // Tham khảo: https://www.postgresql.org/docs/current/errcodes-appendix.html
                //    switch (pex.SqlState)
                //    {
                //        case PostgresErrorCodes.UniqueViolation:           // 23505
                //            status = StatusCodes.Status409Conflict;
                //            title = "Conflict";
                //            detail = "A resource with the same unique value already exists.";
                //            break;

                //        case PostgresErrorCodes.ForeignKeyViolation:       // 23503
                //            status = StatusCodes.Status409Conflict;        // hoặc 400 tuỳ policy
                //            title = "Foreign key violation";
                //            detail = "Related resource does not exist or is referenced.";
                //            break;

                //        case PostgresErrorCodes.NotNullViolation:          // 23502
                //            status = StatusCodes.Status400BadRequest;
                //            title = "Null constraint violation";
                //            detail = $"Column '{pex.ColumnName}' cannot be null.";
                //            break;

                //        case PostgresErrorCodes.CheckViolation:            // 23514
                //            status = StatusCodes.Status400BadRequest;
                //            title = "Check constraint violation";
                //            detail = pex.MessageText; // hoặc message thân thiện hơn
                //            break;
                //    }
                //    break;
            }

            var problem = new ProblemDetails
            {
                Title = title,
                Status = status,
                Detail = detail,
                Type = "about:blank",
                Instance = ctx.Request.Path
            };

            // add extensions (traceId, errors…)
            var extDict = JsonSerializer.Deserialize<Dictionary<string, object>>(
                JsonSerializer.Serialize(extensions, JsonOpt), JsonOpt)!;
            foreach (var kv in extDict) problem.Extensions[kv.Key] = kv.Value;

            ctx.Response.ContentType = "application/problem+json";
            ctx.Response.StatusCode = status;
            await ctx.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOpt));
        }

        private static bool IsUniqueViolation(DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pex)
            {
                return pex.SqlState == PostgresErrorCodes.UniqueViolation;
            }
            return false;
        }
        //private static bool IsUniqueViolation(DbUpdateException ex)
        //{
        //    // SQL Server (2601/2627)
        //    if (ex.InnerException is Microsoft.Data.SqlClient.SqlException mssql)
        //        return mssql.Number is 2601 or 2627;

        //    // PostgreSQL (23505)
        //    if (ex.InnerException?.GetType().FullName == "Npgsql.PostgresException")
        //    {
        //        var sqlState = ex.InnerException.GetType().GetProperty("SqlState")?.GetValue(ex.InnerException)?.ToString();
        //        return sqlState == "23505";
        //    }

        //    return false;
        //}

        // Cách cũ, dùng reflection để check đa DB
        //private static bool IsUniqueViolation(DbUpdateException ex)
        //{
        //    var inner = ex.InnerException;
        //    if (inner == null) return false;

        //    var t = inner.GetType();
        //    var ns = t.Namespace ?? "";
        //    var full = t.FullName ?? "";

        //    // --- SQL Server: Microsoft.Data.SqlClient.SqlException, Number = 2601/2627 ---
        //    if (ns == "Microsoft.Data.SqlClient" && t.Name == "SqlException")
        //    {
        //        var numberProp = t.GetProperty("Number");
        //        if (numberProp?.GetValue(inner) is int number && (number == 2601 || number == 2627))
        //            return true;
        //    }

        //    // --- PostgreSQL: Npgsql.PostgresException, SqlState = 23505 ---
        //    if (full == "Npgsql.PostgresException")
        //    {
        //        var sqlState = t.GetProperty("SqlState")?.GetValue(inner)?.ToString();
        //        if (sqlState == "23505") return true;
        //    }

        //    // --- MySQL (Pomelo/Oracle): code = 1062 (Duplicate entry) ---
        //    // Một số provider có Number, một số là ErrorCode
        //    if (full.Contains("MySql"))
        //    {
        //        var numProp = t.GetProperty("Number") ?? t.GetProperty("ErrorCode");
        //        if (numProp?.GetValue(inner) is int myNum && myNum == 1062)
        //            return true;
        //    }

        //    return false;
        //}

    }

}
