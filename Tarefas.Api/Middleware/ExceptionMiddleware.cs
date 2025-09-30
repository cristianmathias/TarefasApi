using Tarefas.Api.Models;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace Tarefas.Api.Middleware;

/// <summary>
/// Middleware global para tratamento de exceções
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exceção não tratada: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString();
        var traceId = context.TraceIdentifier;

        var (statusCode, response) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                new ErrorResponse
                {
                    Message = "Erro de validação",
                    Errors = new List<string> { validationEx.Message },
                    CorrelationId = correlationId,
                    TraceId = traceId
                }
            ),
            ArgumentException argumentEx => (
                HttpStatusCode.BadRequest,
                new ErrorResponse
                {
                    Message = "Parâmetro inválido",
                    Errors = new List<string> { argumentEx.Message },
                    CorrelationId = correlationId,
                    TraceId = traceId
                }
            ),
            KeyNotFoundException => (
                HttpStatusCode.NotFound,
                new ErrorResponse
                {
                    Message = "Recurso não encontrado",
                    CorrelationId = correlationId,
                    TraceId = traceId
                }
            ),
            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                new ErrorResponse
                {
                    Message = "Acesso não autorizado",
                    CorrelationId = correlationId,
                    TraceId = traceId
                }
            ),
            TimeoutException => (
                HttpStatusCode.RequestTimeout,
                new ErrorResponse
                {
                    Message = "Timeout na operação",
                    CorrelationId = correlationId,
                    TraceId = traceId
                }
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                new ErrorResponse
                {
                    Message = _env.IsDevelopment() ? exception.Message : "Erro interno do servidor",
                    Errors = _env.IsDevelopment() ? new List<string> { exception.StackTrace ?? "" } : null,
                    CorrelationId = correlationId,
                    TraceId = traceId
                }
            )
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _env.IsDevelopment()
        };

        var jsonResponse = JsonSerializer.Serialize(response, jsonOptions);
        await context.Response.WriteAsync(jsonResponse);
    }
}