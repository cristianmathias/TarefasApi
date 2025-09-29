# 🚀 Guia Completo: Como Expor a API Tarefas no Azure API Management

## 📋 Pré-requisitos

1. **Conta Azure ativa** com permissões para criar recursos
2. **API funcionando** (localmente ou hospedada)
3. **Arquivo OpenAPI/Swagger JSON** (já gerado: `tarefas-api-openapi-final.json`)

## 🎯 Resumo da Resposta

**Sim, você precisa do arquivo OpenAPI JSON para importar no Azure APIM!** Já foi gerado:
- ✅ `tarefas-api-openapi-final.json` - Arquivo pronto para importação

## 📝 Passo a Passo Completo

### 1. 📦 Hospedar sua API primeiro

Antes de expor no APIM, sua API precisa estar acessível. Opções:

#### Opção A: Azure App Service (Recomendado)
```bash
# 1. Crie um App Service
az webapp create --resource-group myResourceGroup --plan myAppServicePlan --name tarefas-api-app --runtime "DOTNET|9.0"

# 2. Deploy da aplicação
dotnet publish -c Release -o ./publish
zip -r app.zip ./publish/*
az webapp deployment source config-zip --resource-group myResourceGroup --name tarefas-api-app --src app.zip
```

#### Opção B: Azure Container Instances
```bash
# Build da imagem Docker
docker build -t tarefas-api .
docker tag tarefas-api myregistry.azurecr.io/tarefas-api:latest
docker push myregistry.azurecr.io/tarefas-api:latest

# Deploy no ACI
az container create --resource-group myResourceGroup --name tarefas-api --image myregistry.azurecr.io/tarefas-api:latest --port 8080
```

### 2. 🏗️ Criar Azure API Management

#### Via Portal Azure:
1. Acesse **Portal Azure** → **Criar recurso**
2. Busque **API Management** → **Criar**
3. Configure:
   - **Nome**: `tarefas-apim`
   - **Tier**: Developer (para teste) ou Standard
   - **Região**: Mesma da sua API
   - **Organização**: Sua empresa
   - **E-mail do administrador**: Seu e-mail

#### Via Azure CLI:
```bash
# Criar API Management
az apim create \
  --name tarefas-apim \
  --resource-group myResourceGroup \
  --location eastus \
  --publisher-email admin@empresa.com \
  --publisher-name "Minha Empresa" \
  --sku-name Developer
```

### 3. 📤 Importar sua API usando OpenAPI

#### Via Portal Azure:
1. Acesse seu **API Management** → **APIs**
2. Clique **+ Add API** → **OpenAPI**
3. **OpenAPI specification**: Cole a URL do seu swagger OU upload do arquivo `tarefas-api-openapi-final.json`
4. Configure:
   - **Display name**: `Tarefas API`
   - **Name**: `tarefas-api`
   - **API URL suffix**: `tarefas`
   - **Base URL**: URL da sua API hospedada (ex: `https://tarefas-api-app.azurewebsites.net`)

#### Via Azure CLI:
```bash
# Importar API usando arquivo OpenAPI
az apim api import \
  --resource-group myResourceGroup \
  --service-name tarefas-apim \
  --api-id tarefas-api \
  --path tarefas \
  --display-name "Tarefas API" \
  --protocols https \
  --service-url https://tarefas-api-app.azurewebsites.net \
  --specification-format OpenApi \
  --specification-path tarefas-api-openapi-final.json
```

### 4. 🔧 Configurações Importantes no APIM

#### A. Políticas (Policies)
Adicione políticas para melhorar a API:

```xml
<!-- Política de CORS -->
<cors>
    <allowed-origins>
        <origin>*</origin>
    </allowed-origins>
    <allowed-methods>
        <method>GET</method>
        <method>POST</method>
        <method>PUT</method>
        <method>DELETE</method>
    </allowed-methods>
</cors>

<!-- Rate Limiting -->
<rate-limit calls="100" renewal-period="60" />

<!-- Transformação de response -->
<set-header name="X-Powered-By" exists-action="override">
    <value>Azure API Management</value>
</set-header>
```

#### B. Autenticação e Autorização
```xml
<!-- Validação de subscription key -->
<validate-subscription-key />

<!-- JWT Token validation (se necessário) -->
<validate-jwt header-name="Authorization" failed-validation-httpcode="401">
    <openid-config url="https://login.microsoftonline.com/{tenant}/v2.0/.well-known/openid_configuration" />
</validate-jwt>
```

### 5. 📊 Configurar Produtos e Subscriptions

#### Criar Produto:
```bash
# Criar produto
az apim product create \
  --resource-group myResourceGroup \
  --service-name tarefas-apim \
  --product-id tarefas-product \
  --product-name "Tarefas Product" \
  --description "Produto para API de Tarefas" \
  --subscription-required true \
  --approval-required false \
  --state published

# Associar API ao produto
az apim product api add \
  --resource-group myResourceGroup \
  --service-name tarefas-apim \
  --product-id tarefas-product \
  --api-id tarefas-api
```

### 6. 🌐 URLs Resultantes

Após a configuração:

- **Gateway URL**: `https://tarefas-apim.azure-api.net`
- **API Endpoints**: 
  - `GET https://tarefas-apim.azure-api.net/tarefas/Tarefas`
  - `POST https://tarefas-apim.azure-api.net/tarefas/Tarefas`
  - `GET https://tarefas-apim.azure-api.net/tarefas/Tarefas/{id}`
  - `PUT https://tarefas-apim.azure-api.net/tarefas/Tarefas/{id}`
  - `DELETE https://tarefas-apim.azure-api.net/tarefas/Tarefas/{id}`
- **Developer Portal**: `https://tarefas-apim.developer.azure-api.net`

### 7. 🧪 Testar a API

#### Via Portal APIM:
1. Acesse **APIs** → **Tarefas API** → **GET /Tarefas**
2. Clique **Test** → **Send**

#### Via cURL:
```bash
# Obter subscription key primeiro
SUBSCRIPTION_KEY="sua-subscription-key"

# Testar endpoint
curl -H "Ocp-Apim-Subscription-Key: $SUBSCRIPTION_KEY" \
     https://tarefas-apim.azure-api.net/tarefas/Tarefas

# Criar nova tarefa
curl -X POST \
  -H "Content-Type: application/json" \
  -H "Ocp-Apim-Subscription-Key: $SUBSCRIPTION_KEY" \
  -d '{"titulo":"Nova Tarefa","descricao":"Descrição da tarefa"}' \
  https://tarefas-apim.azure-api.net/tarefas/Tarefas
```

### 8. 📈 Monitoramento e Analytics

O Azure APIM fornece automaticamente:
- **Métricas de uso** (requests, latência, erros)
- **Logs detalhados**
- **Analytics do Developer Portal**
- **Alertas personalizados**

## ✅ Checklist Final

- [ ] API hospedada e funcionando
- [ ] Azure APIM criado
- [ ] API importada usando OpenAPI JSON
- [ ] Políticas configuradas
- [ ] Produto e subscription criados
- [ ] Testes realizados
- [ ] Monitoramento configurado

## 🔒 Considerações de Segurança

1. **Sempre use HTTPS**
2. **Configure rate limiting**
3. **Implemente autenticação adequada**
4. **Monitore o uso da API**
5. **Mantenha logs de auditoria**

## 💰 Custos

- **Developer Tier**: ~$50/mês (para desenvolvimento/teste)
- **Basic Tier**: ~$150/mês (produção pequena)
- **Standard Tier**: ~$650/mês (produção média)
- **Premium Tier**: ~$2800/mês (produção enterprise)

## 📚 Próximos Passos

1. **Versionamento**: Configure versionamento da API
2. **Developer Portal**: Customize o portal para desenvolvedores
3. **CI/CD**: Integre com Azure DevOps para deploy automatizado
4. **Backup**: Configure backup das configurações do APIM