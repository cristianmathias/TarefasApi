using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Tarefas.Api.Filters;

/// <summary>
/// Filtro para adicionar header Correlation-ID na documentação Swagger
/// </summary>
public class CorrelationIdHeaderFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Correlation-ID",
            In = ParameterLocation.Header,
            Required = false,
            Schema = new OpenApiSchema
            {
                Type = "string"
            },
            Description = "ID de correlação para rastreamento da requisição (gerado automaticamente se não fornecido)"
        });

        // Adicionar exemplos de response headers
        operation.Responses ??= new OpenApiResponses();
        
        foreach (var response in operation.Responses.Values)
        {
            response.Headers ??= new Dictionary<string, OpenApiHeader>();
            
            if (!response.Headers.ContainsKey("X-Correlation-ID"))
            {
                response.Headers.Add("X-Correlation-ID", new OpenApiHeader
                {
                    Description = "ID de correlação da requisição",
                    Schema = new OpenApiSchema { Type = "string" }
                });
            }
        }
    }
}