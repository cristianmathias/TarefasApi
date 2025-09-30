# 📋 Architecture Decision Records (ADRs)

## 🎯 Propósito
Este diretório contém os **Architecture Decision Records** do projeto Tarefas API, documentando decisões arquiteturais importantes tomadas durante o desenvolvimento.

## 📚 Formato dos ADRs
Seguimos o padrão de nomenclatura: `{número}-{titulo-da-decisao}.md`

## 📑 Índice de ADRs

| ADR | Título | Data | Status |
|-----|---------|------|---------|
| [ADR-001](./001-azure-apim-integration.md) | Integração com Azure API Management | 2024-12-29 | ✅ Aceito |
| [ADR-002](./002-ef-vs-custom-paging.md) | PagedResult vs Entity Framework Paging | 2024-12-29 | ✅ Aceito |
| [ADR-003](./003-melhorias-planejadas.md) | Roadmap de Melhorias Arquiteturais | 2024-12-29 | 🔄 Em Progresso |
| [ADR-004](./004-limitacoes-testes-inmemory.md) | Limitações dos Testes InMemory | 2024-12-30 | ✅ Aceito |
| [ADR-005](./005-fase-3-observabilidade.md) | FASE 3 - Observabilidade e Monitoramento | 2024-12-30 | ✅ Aceito |

## 🏗️ Template de ADR
Para novos ADRs, use a estrutura:

```markdown
# ADR-XXX: [Título da Decisão]

## Status
- **Data**: YYYY-MM-DD  
- **Status**: [Proposto/Aceito/Rejeitado/Supersedido]
- **Decisores**: [Nome dos decisores]

## Contexto
[Descrever o contexto e problema que motivou a decisão]

## Decisão
[Descrever a decisão tomada]

## Consequências
### Positivas
- [Benefício 1]
- [Benefício 2]

### Negativas  
- [Trade-off 1]
- [Trade-off 2]

## Alternativas Consideradas
- [Alternativa 1]: [Motivo da rejeição]
- [Alternativa 2]: [Motivo da rejeição]
```

## 🔄 Processo de ADRs
1. **Propor**: Criar ADR com status "Proposto"
2. **Discutir**: Review com equipe técnica
3. **Decidir**: Aceitar, rejeitar ou modificar
4. **Implementar**: Executar a decisão
5. **Atualizar**: Marcar como "Aceito" ou "Rejeitado"

## 📖 Referências
- [ADR GitHub Template](https://github.com/joelparkerhenderson/architecture-decision-record)
- [Thoughtworks ADR Tools](https://github.com/npryce/adr-tools)