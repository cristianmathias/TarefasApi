# ADR-001: Integração com Azure API Management

## Status
- **Data**: 2024-12-29
- **Status**: ✅ Aceito
- **Decisores**: Cristian Mathias
- **Tags**: azure, apim, api-gateway, openapi

## Contexto
A API de Tarefas precisa ser exposta publicamente através de um API Gateway para:
- Controle de acesso e autenticação
- Rate limiting e throttling  
- Monitoramento e analytics
- Documentação centralizada
- Versionamento de API

## Decisão
**Utilizar Azure API Management (APIM) como gateway da API**, integrando através de:
1. **Especificação OpenAPI/Swagger** gerada automaticamente
2. **Arquivo JSON** para importação no APIM
3. **Documentação completa** com metadados e validações
4. **Headers customizados** (X-Correlation-ID)

### Implementação:
- ✅ Melhorada documentação OpenAPI com anotações ProducesResponseType
- ✅ Gerados arquivos swagger JSON para importação
- ✅ Adicionadas validações nos DTOs (Required, StringLength)
- ✅ Criado guia completo de deployment
- ✅ Configurado Swagger com metadados profissionais

## Consequências

### Positivas
- ✅ **Gateway Profissional**: Infraestrutura enterprise-ready
- ✅ **Documentação Automática**: OpenAPI spec completa e padronizada
- ✅ **Importação Simples**: JSON pronto para deploy no APIM
- ✅ **Rastreabilidade**: Correlation IDs para debugging
- ✅ **Flexibilidade**: API independente do gateway
- ✅ **Observabilidade**: Métricas e logs centralizados no Azure
- ✅ **Segurança**: Rate limiting, authentication, CORS automáticos

### Negativas
- ❌ **Vendor Lock-in**: Dependência do Azure
- ❌ **Custo**: APIM tem cobrança (Developer: ~$50/mês)
- ❌ **Complexidade**: Configuração adicional necessária
- ❌ **Latência**: Hop adicional no request

## Alternativas Consideradas

### 1. **Nginx + Kong**
- ✅ Open source e flexível
- ❌ Maior complexidade de configuração
- ❌ Menos integração com Azure

### 2. **AWS API Gateway**  
- ✅ Maduro e estável
- ❌ Vendor lock-in diferente
- ❌ Menos integração se já estamos no Azure

### 3. **Sem Gateway (API direta)**
- ✅ Simplicidade máxima
- ❌ Sem rate limiting
- ❌ Sem analytics centralizados  
- ❌ Sem versionamento
- ❌ Não escala para enterprise

### 4. **Ocelot (.NET)**
- ✅ Nativo .NET
- ❌ Mais código para manter
- ❌ Menos features que APIM

## Detalhes da Implementação

### Arquivos Gerados:
- `tarefas-api-openapi.json` - Especificação OpenAPI completa
- `Azure-APIM-Guide.md` - Guia passo-a-passo de deployment

### Melhorias na API:
```csharp
// Anotações ProducesResponseType
[ProducesResponseType(typeof(Tarefa), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]

// Validações nos DTOs
[Required(ErrorMessage = "O título é obrigatório")]
[StringLength(100, ErrorMessage = "O título deve ter no máximo 100 caracteres")]
```

### OpenAPI Spec:
```json
{
  "openapi": "3.0.4",
  "info": {
    "title": "Tarefas API",
    "description": "API REST para gerenciamento de tarefas...",
    "contact": { "name": "Desenvolvedor", "email": "dev@tarefas.com" }
  }
}
```

## Referencias
- [Azure APIM Documentation](https://docs.microsoft.com/en-us/azure/api-management/)
- [OpenAPI Specification](https://swagger.io/specification/)
- [ASP.NET Core OpenAPI](https://docs.microsoft.com/en-us/aspnet/core/tutorials/web-api-help-pages-using-swagger)