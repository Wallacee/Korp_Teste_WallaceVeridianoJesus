# Korp Invoice

Sistema web para emissão e gerenciamento de notas fiscais, desenvolvido com Angular, ASP.NET Core, PostgreSQL e arquitetura de microsserviços.

## Arquitetura

A solução é composta inicialmente por dois microsserviços independentes:

- **Inventory Service** — responsável pelo cadastro de produtos e controle de estoque.
- **Billing Service** — responsável pela criação e processamento de notas fiscais.

Cada microsserviço segue os princípios de Clean Architecture, separando as responsabilidades entre:

- API
- Application
- Domain
- Infrastructure

A comunicação entre os microsserviços será realizada por HTTP/REST.

## Estrutura

```text
src/
├── backend/
│   ├── Inventory/
│   │   ├── Korp.Invoice.Inventory.Api/
│   │   ├── Korp.Invoice.Inventory.Application/
│   │   ├── Korp.Invoice.Inventory.Domain/
│   │   └── Korp.Invoice.Inventory.Infrastructure/
│   │
│   └── Billing/
│       ├── Korp.Invoice.Billing.Api/
│       ├── Korp.Invoice.Billing.Application/
│       ├── Korp.Invoice.Billing.Domain/
│       └── Korp.Invoice.Billing.Infrastructure/
│
└── frontend/

tests/
docs/

## Testes

O projeto possui testes unitários para as principais regras
de negócio e casos de uso.

Para executar:

dotnet test Korp.Invoice.slnx

### Cobertura

Os testes abrangem principalmente:

- Regras de domínio de produtos;
- Controle de saldo;
- Validação de produtos;
- Cadastro de produtos;
- Conflito de código duplicado;
- Consulta de produto inexistente.
