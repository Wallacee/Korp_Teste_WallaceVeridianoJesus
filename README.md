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

## Comunicação entre Frontend e Microsserviços

O frontend Angular consome diretamente as APIs de Inventory e Billing,
mantendo cada serviço responsável pelos recursos pertencentes ao seu domínio.

Operações que envolvem mais de um microsserviço não são orquestradas pelo
frontend. No processamento de uma nota fiscal, por exemplo, o Angular aciona
o Billing Service, que coordena a operação de estoque junto ao Inventory Service.

Para o escopo atual, optou-se por não introduzir um API Gateway, evitando
complexidade adicional para apenas dois microsserviços.

Em uma evolução da arquitetura, com aumento do número de serviços e
necessidade de preocupações transversais, um API Gateway ou BFF poderia ser
introduzido como ponto único de entrada.


                    ┌─────────────────┐
                    │     Angular     │
                    └───────┬─────────┘
                            │
                 ┌──────────┴──────────┐
                 │                     │
                 ▼                     ▼
        ┌─────────────────┐   ┌─────────────────┐
        │ Inventory API   │   │   Billing API   │
        └─────────────────┘   └────────┬────────┘
                 ▲                     │
                 │                     │
                 └─────────────────────┘
                    processamento
                       de estoque
