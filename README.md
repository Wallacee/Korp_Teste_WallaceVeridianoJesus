# 🧾 Korp Invoice

Sistema de emissão de notas fiscais desenvolvido como projeto técnico, com frontend em Angular e backend em C#/.NET, estruturado em microsserviços independentes para Estoque e Faturamento.

O projeto implementa os requisitos funcionais e arquiteturais propostos no desafio e adiciona recursos opcionais de concorrência, idempotência e inteligência artificial aplicada à previsão de consumo de estoque.

---

## 🚀 Como executar o projeto

> O fluxo abaixo considera o `docker-compose` final do projeto, responsável por subir os dois bancos PostgreSQL, os dois microsserviços e o frontend Angular. Caso você prefira executar os projetos localmente, há uma seção específica logo abaixo.

### Pré-requisitos

- Docker Desktop / Docker Engine
- Docker Compose
- .NET SDK compatível com a solução
- Node.js + npm, apenas para execução local do frontend

### 1. Clonar o repositório

```bash
git clone <URL_DO_REPOSITORIO>
cd Korp_Teste_WallaceVeridianoJesus
```

### 2. Subir a aplicação com Docker Compose

Na raiz do repositório:

```bash
docker compose up -d --build
```

O compose deverá subir os seguintes serviços:

| Serviço | Finalidade | Endereço local |
|---|---|---|
| `inventory-db` | PostgreSQL do microsserviço de Estoque | porta configurada no compose |
| `billing-db` | PostgreSQL do microsserviço de Faturamento | porta configurada no compose |
| `inventory-api` | API de Estoque | `http://localhost:5136` |
| `billing-api` | API de Faturamento | `http://localhost:5258` |
| `frontend` | Aplicação Angular | `http://localhost:4200` |

Para acompanhar os containers:

```bash
docker compose ps
```

Para visualizar logs:

```bash
docker compose logs -f
```

Para encerrar o ambiente:

```bash
docker compose down
```

Para remover também os volumes dos bancos:

```bash
docker compose down -v
```

### 3. Migrations

Caso o ambiente Docker não esteja configurado para aplicar as migrations automaticamente na inicialização, elas podem ser aplicadas manualmente a partir da raiz do repositório.

#### Inventory

```bash
dotnet ef database update --project src/backend/Inventory/Korp.Invoice.Inventory.Infrastructure --startup-project src/backend/Inventory/Korp.Invoice.Inventory.Api
```

#### Billing

```bash
dotnet ef database update --project src/backend/Billing/Korp.Invoice.Billing.Infrastructure --startup-project src/backend/Billing/Korp.Invoice.Billing.Api
```

---

## 💻 Execução local para desenvolvimento

Caso seja necessário executar cada aplicação separadamente, primeiro suba apenas os bancos com Docker Compose e depois execute os projetos .NET e Angular localmente.

### Inventory API

```bash
dotnet run --project src/backend/Inventory/Korp.Invoice.Inventory.Api
```

API disponível em:

```text
http://localhost:5136
```

### Billing API

```bash
dotnet run --project src/backend/Billing/Korp.Invoice.Billing.Api
```

API disponível em:

```text
http://localhost:5258
```

### Angular

Entre no diretório do frontend e instale as dependências:

```bash
npm install
```

Depois:

```bash
npm start
```

Aplicação disponível em:

```text
http://localhost:4200
```

---

# 🧭 Visão geral

A aplicação foi desenvolvida para representar um fluxo simplificado de emissão de notas fiscais com controle de estoque.

A solução é dividida em dois microsserviços principais:

```text
Angular
   |
   | HTTP/REST
   |
   +-----------------------+
   |                       |
   v                       v
Inventory API          Billing API
   |                       |
   |                       |
PostgreSQL             PostgreSQL
                           |
                           | HTTP
                           v
                      Inventory API
```

- **Inventory**: responsável pelos produtos, saldos e movimentações de estoque.
- **Billing**: responsável pelas notas fiscais, itens, numeração, status e processamento da emissão.
- **Angular**: interface responsável pela experiência do usuário e integração com os dois microsserviços.

Os bancos são independentes, preservando o isolamento entre os microsserviços.

---

# ✨ Funcionalidades implementadas

## Produtos

O módulo de produtos permite:

- cadastro de produto;
- edição;
- exclusão;
- consulta paginada server-side;
- pesquisa por código ou descrição;
- ordenação server-side;
- controle de saldo;
- validação de campos no frontend e backend;
- bloqueio de exclusão quando o produto já está associado a uma nota fiscal.

Campos principais:

- Código;
- Descrição;
- Saldo.

A exclusão de um produto que já foi utilizado em uma nota é bloqueada para preservar a integridade histórica do sistema. Como Inventory e Billing possuem bancos independentes, essa verificação é realizada por comunicação HTTP entre os microsserviços.

---

## Notas fiscais

O módulo de faturamento permite:

- criação de nota fiscal;
- numeração sequencial;
- status inicial `Aberta`;
- inclusão de múltiplos produtos;
- definição da quantidade de cada produto;
- consulta paginada e ordenada;
- visualização de detalhes;
- edição enquanto a nota estiver aberta;
- exclusão enquanto a nota estiver aberta;
- bloqueio de alteração de notas fechadas.

A edição e exclusão de notas abertas foram adicionadas como extensão de usabilidade ao escopo original.

---

## Impressão de notas fiscais

A impressão segue o fluxo solicitado no desafio.

Uma nota somente pode ser impressa enquanto estiver com status `Aberta`.

Fluxo:

```text
Nota Aberta
    |
    v
Usuário solicita impressão
    |
    v
Indicador de processamento
    |
    v
Billing solicita débito ao Inventory
    |
    v
Estoque atualizado
    |
    v
Nota alterada para Fechada
    |
    v
Documento preparado para impressão
```

Após o processamento:

- o estoque dos produtos é atualizado;
- a nota passa para `Fechada`;
- o usuário recebe feedback visual;
- a impressão utiliza um layout próprio para A4;
- notas fechadas não podem executar novamente a operação de processamento.

Exemplo:

```text
Saldo anterior: 10
Quantidade utilizada na nota: 2
Saldo final: 8
```

---

# 📊 Dashboard

A aplicação possui uma visão geral operacional consolidando dados dos dois microsserviços.

Indicadores exibidos:

- total de produtos cadastrados;
- saldo total em estoque;
- produtos com estoque baixo;
- notas abertas;
- notas fechadas;
- unidades processadas;
- consumo de estoque ao longo do tempo;
- produtos mais utilizados;
- previsão inteligente de consumo.

Os dados são obtidos diretamente das APIs de Inventory e Billing e combinados no frontend utilizando RxJS.

Os gráficos são renderizados com **Chart.js**.

---

# 🤖 Inteligência Artificial

Como requisito opcional, foi implementada uma funcionalidade de previsão inteligente de consumo de estoque.

A solução utiliza:

- **ML.NET**;
- **Microsoft.ML.TimeSeries**;
- modelo de séries temporais baseado em **SSA — Singular Spectrum Analysis**.

O modelo utiliza o histórico real de consumo gerado pelas notas fiscais fechadas e projeta o consumo esperado para os próximos dias.

Fluxo simplificado:

```text
Notas fechadas
    |
    v
Histórico diário de consumo
    |
    v
ML.NET / SSA
    |
    v
Modelo de série temporal
    |
    v
Previsão inteligente de consumo
```

A interface diferencia visualmente:

- consumo realizado;
- previsão inteligente;
- consumo estimado para os próximos dias;
- tendência de comportamento.

Quando não há histórico suficiente, a aplicação informa que ainda não existem dados suficientes para gerar uma previsão confiável.

---

# ⚡ Tratamento de concorrência

O projeto contempla o cenário opcional de concorrência no estoque.

Exemplo:

```text
Produto com saldo = 1

Nota A ----+
           +--> tentam consumir simultaneamente
Nota B ----+
```

A aplicação utiliza controle de concorrência no armazenamento do produto para impedir que duas operações concorrentes consumam o mesmo saldo de forma inconsistente.

O objetivo é garantir que:

- apenas uma operação possa confirmar o último item disponível;
- a outra operação receba uma falha de concorrência/regra de negócio;
- o saldo nunca fique negativo.

---

# 🔁 Idempotência

O processamento de estoque também possui proteção contra repetição da mesma operação.

Cada processamento recebe um `OperationId` único.

O Inventory mantém o registro da operação em `StockOperations`, cujo `OperationId` possui índice único no banco.

Fluxo:

```text
OperationId ABC
     |
     v
Primeira execução
     |
     +--> estoque debitado
     +--> operação registrada

OperationId ABC novamente
     |
     +--> operação reconhecida
     +--> nenhum novo débito
```

Isso evita efeitos colaterais indesejados em cenários de retry, timeout ou repetição de requisição.

---

# 🛡️ Tratamento de falhas entre microsserviços

O tratamento de falhas foi implementado como parte central da solução.

A comunicação HTTP entre Billing e Inventory utiliza políticas de resiliência para lidar com:

- timeout;
- indisponibilidade do microsserviço;
- falhas transitórias;
- conflitos de negócio.

A solução diferencia falhas técnicas de erros de negócio.

Exemplo de saldo insuficiente:

```text
Product.DebitStock
        |
        v
InsufficientStockException
        |
        v
Inventory API -> HTTP 409
        |
        v
Billing preserva o erro de negócio
        |
        v
Billing API -> ProblemDetails
        |
        v
Angular apresenta a mensagem ao usuário
```

Um conflito de estoque não é tratado como indisponibilidade do microsserviço.

Da mesma forma, quando Inventory está indisponível, Billing informa a indisponibilidade corretamente sem fechar a nota nem considerar a operação concluída.

---

# 🧱 Backend

Os dois microsserviços foram desenvolvidos em C#/.NET utilizando separação por responsabilidades inspirada em Clean Architecture.

Estrutura principal:

```text
Api
Application
Domain
Infrastructure
```

## Domain

Contém:

- entidades;
- regras de negócio;
- exceções de domínio;
- contratos independentes de infraestrutura.

Exemplos de regras protegidas no domínio:

- saldo nunca pode ficar negativo;
- quantidade de débito deve ser válida;
- uma nota fechada não pode ser alterada;
- uma nota não pode ser fechada sem itens;
- o mesmo produto não pode ser incluído mais de uma vez na mesma nota.

## Application

Contém:

- casos de uso;
- AppServices;
- requests e responses;
- validações;
- interfaces de serviços externos.

## Infrastructure

Contém:

- Entity Framework Core;
- repositories;
- DbContexts;
- migrations;
- PostgreSQL;
- clientes HTTP entre microsserviços;
- implementação da previsão com ML.NET.

## Api

Responsável por:

- controllers;
- endpoints REST;
- configuração da aplicação;
- injeção de dependência;
- tratamento global de exceções.

---

# 🗄️ Persistência

A aplicação utiliza PostgreSQL com Entity Framework Core.

Cada microsserviço possui seu próprio banco de dados e seu próprio `DbContext`.

Isso evita compartilhamento direto de tabelas entre serviços e mantém o isolamento arquitetural.

O acesso a dados utiliza repositories e operações assíncronas.

Paginação, filtros, ordenação e agregações são executados no banco de dados sempre que possível.

---

# 🔎 LINQ

LINQ é utilizado extensivamente no backend.

Exemplos:

```csharp
.Where(...)
.Select(...)
.SelectMany(...)
.GroupBy(...)
.OrderBy(...)
.OrderByDescending(...)
.Skip(...)
.Take(...)
.CountAsync(...)
.SumAsync(...)
.AnyAsync(...)
```

Entre os usos principais estão:

- filtros de produtos;
- paginação server-side;
- ordenação dinâmica;
- busca de notas fiscais;
- agregação dos dados do dashboard;
- cálculo de consumo diário;
- ranking de produtos mais utilizados;
- preparação das séries utilizadas pelo modelo de previsão.

As consultas são construídas sobre `IQueryable`, permitindo que o Entity Framework traduza as expressões para SQL e execute o processamento diretamente no PostgreSQL, evitando carregar grandes volumes de dados em memória.

---

# ✅ Validação

O backend utiliza **FluentValidation** para validação dos requests.

As regras de domínio permanecem nas entidades e exceções específicas de domínio, enquanto validações de entrada são realizadas na Application.

Essa separação permite diferenciar:

- dados inválidos;
- recurso inexistente;
- conflito de negócio;
- falha de infraestrutura.

---

# 🚨 Tratamento de erros e exceções

As APIs utilizam tratamento global de exceções com respostas padronizadas em `ProblemDetails`.

Principais respostas:

| Status | Uso |
|---|---|
| `400 Bad Request` | erro de validação ou entrada inválida |
| `404 Not Found` | recurso inexistente |
| `409 Conflict` | conflito de regra de negócio, concorrência ou estoque insuficiente |
| `503 Service Unavailable` | microsserviço externo indisponível / timeout |
| `500 Internal Server Error` | erro inesperado |

Erros originados no Inventory são interpretados pelo Billing antes de serem devolvidos ao frontend. Dessa forma, uma mensagem útil de domínio não é substituída por um erro genérico.

---

# 🔄 Resiliência HTTP

A comunicação entre os microsserviços utiliza `HttpClient` e políticas de resiliência baseadas no ecossistema Polly / Microsoft.Extensions.Http.Resilience.

Foram considerados cenários como:

- timeout;
- retry de falhas transitórias;
- serviço indisponível;
- propagação correta de conflitos de negócio.

Erros `409`, por exemplo, não são tratados como falhas transitórias a serem repetidas indefinidamente.

---

# 🅰️ Frontend Angular

O frontend foi desenvolvido em Angular utilizando componentes standalone e uma organização orientada a features.

Estrutura simplificada:

```text
src/app/
├── features/
│   ├── dashboard/
│   ├── products/
│   └── invoices/
├── layout/
└── shared/
```

A aplicação utiliza um shell compartilhado, estilos enterprise reutilizáveis e serviços centralizados para integração com as APIs e feedback ao usuário.

---

# ♻️ Ciclos de vida do Angular

Foram utilizados ciclos de vida do Angular conforme a necessidade dos componentes.

## `OnInit`

Utilizado em telas que dependem de parâmetros de rota ou precisam carregar dados imediatamente após a inicialização.

Exemplos:

- carregamento dos detalhes de uma nota fiscal;
- identificação do modo criação/edição;
- carregamento de registros existentes em formulários.

## `OnDestroy`

Utilizado no dashboard para liberar as instâncias de gráficos do Chart.js ao sair da página, evitando referências pendentes e vazamento de recursos.

Além dos hooks, o dashboard utiliza `ViewChild` para trabalhar com os elementos `canvas` criados dinamicamente pelo template e sincronizar corretamente a criação das instâncias do Chart.js com o ciclo de renderização da view.

---

# 🔀 RxJS

RxJS é utilizado em diferentes pontos da aplicação.

Principais operadores utilizados:

- `forkJoin`;
- `switchMap`;
- `debounceTime`;
- `distinctUntilChanged`;
- `finalize`;
- `map`;
- `of`;
- `startWith`;
- `valueChanges` de Reactive Forms.

## Busca server-side de produtos

No cadastro de notas fiscais, o autocomplete não carrega todo o catálogo de produtos.

Fluxo:

```text
Digitação
   |
   v
valueChanges
   |
   v
debounceTime
   |
   v
distinctUntilChanged
   |
   v
switchMap
   |
   v
Inventory API
```

Isso reduz chamadas desnecessárias e permite trabalhar com grande volume de produtos.

## 📊 Dashboard

O dashboard utiliza `forkJoin` para consultar Billing e Inventory em paralelo e consolidar os indicadores na mesma tela.

---

# 🎨 Componentes visuais

A interface utiliza **Angular Material** como principal biblioteca visual.

Foram utilizados, entre outros:

- buttons;
- form fields;
- inputs;
- autocomplete;
- tables;
- paginator;
- sorting;
- icons;
- progress spinner;
- dialog;
- snackbar.

Os gráficos utilizam **Chart.js**.

Também foi criado um padrão visual compartilhado para:

- breadcrumbs;
- cabeçalhos de página;
- cards;
- formulários;
- tabelas;
- paginação;
- botões;
- loading states;
- empty states;
- notificações.

Os feedbacks possuem diferenciação visual para sucesso, alerta, erro e informação.

---

# ⚙️ Paginação e performance

As listagens principais utilizam paginação server-side.

O Angular envia:

- página;
- tamanho da página;
- filtro;
- coluna de ordenação;
- direção.

O backend aplica essas informações sobre `IQueryable` antes da execução da consulta.

Isso evita carregar tabelas completas para a memória do frontend ou backend.

O autocomplete de produtos também utiliza busca server-side com quantidade limitada de resultados.

---

# 🧰 Tecnologias e bibliotecas

## Frontend

- Angular
- TypeScript
- Angular Material
- RxJS
- Chart.js
- SCSS

## 🧱 Backend

- C# / .NET
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- FluentValidation
- HttpClient
- Polly / Microsoft.Extensions.Http.Resilience
- ML.NET
- Microsoft.ML.TimeSeries

## Testes

- xUnit
- Moq

## Infraestrutura

- Docker
- Docker Compose
- PostgreSQL

---

# 🧪 Testes automatizados

A solução possui testes automatizados para regras relevantes do domínio e da Application.

Entre os cenários considerados estão:

- débito de estoque com saldo suficiente;
- tentativa de débito acima do saldo disponível;
- quantidades inválidas;
- criação e processamento de notas;
- notas sem itens;
- notas já fechadas;
- validações;
- falhas na comunicação com Inventory;
- comportamento de idempotência;
- cenários de concorrência.

Os testes de regras de negócio são separados de cenários que dependem de comportamento real de persistência e concorrência.

---

# 🏗️ Decisões arquiteturais

## Comunicação entre microsserviços

Foi escolhida comunicação HTTP síncrona para manter o escopo do desafio simples e explícito.

Billing não acessa diretamente as tabelas do Inventory, e Inventory não acessa diretamente o banco do Billing.

Quando um serviço precisa de informação pertencente ao outro, essa informação é obtida por contrato HTTP.

## Bancos separados

Cada microsserviço é responsável pelo próprio banco, evitando acoplamento por persistência compartilhada.

## Repository + Unit of Work

Repositories encapsulam o acesso às entidades e consultas.

O `UnitOfWork` controla o momento de persistência das alterações e permite manter as operações de escrita consistentes.

## Regras no domínio

Regras que representam invariantes do negócio permanecem nas entidades, evitando que controllers ou frontend sejam responsáveis por garantir consistência.

---

# 🎬 Cenários importantes para demonstração

## Fluxo de sucesso

```text
Criar produto
    |
Criar nota com múltiplos produtos
    |
Imprimir nota
    |
Inventory debita estoque
    |
Billing fecha nota
    |
Usuário recebe feedback
```

## Estoque insuficiente

```text
Nota solicita quantidade maior que o saldo
    |
Inventory detecta conflito
    |
HTTP 409
    |
Billing preserva mensagem
    |
Frontend informa o usuário
    |
Nota permanece aberta
```

## Inventory indisponível

```text
Billing tenta processar nota
    |
Inventory indisponível / timeout
    |
Política de resiliência atua
    |
Billing retorna indisponibilidade
    |
Frontend apresenta feedback
    |
Nota não é fechada
```

## 🔁 Idempotência

```text
Mesma operação enviada novamente
    |
OperationId já processado
    |
Nenhum novo débito de estoque
```

## Concorrência

```text
Saldo = 1
    |
Duas notas tentam consumir simultaneamente
    |
Apenas uma operação confirma
    |
Saldo final permanece consistente
```

---

# 📋 Requisitos do desafio atendidos

| Requisito | Implementação |
|---|---|
| Cadastro de Produtos | Sim |
| Código, descrição e saldo | Sim |
| Cadastro de Notas Fiscais | Sim |
| Numeração sequencial | Sim |
| Status Aberta / Fechada | Sim |
| Múltiplos produtos e quantidades | Sim |
| Impressão | Sim |
| Indicador de processamento | Sim |
| Fechamento após impressão | Sim |
| Atualização de estoque | Sim |
| Bloqueio de impressão fora do estado Aberta | Sim |
| Dois microsserviços | Sim |
| Banco real | Sim |
| Tratamento de falha entre serviços | Sim |
| Tratamento de concorrência | Sim — opcional |
| Idempotência | Sim — opcional |
| Inteligência Artificial | Sim — opcional, ML.NET SSA |

---

# 📚 Detalhamento técnico solicitado

## Ciclos de vida Angular utilizados

- `OnInit`: carregamento inicial e tratamento de parâmetros de rota.
- `OnDestroy`: destruição das instâncias do Chart.js no dashboard.
- `ViewChild`: sincronização com os elementos `canvas` renderizados dinamicamente.

## Uso de RxJS

Sim.

Utilizado para:

- debounce de pesquisa;
- autocomplete server-side;
- cancelamento lógico de pesquisas anteriores com `switchMap`;
- composição de chamadas paralelas com `forkJoin`;
- transformação de respostas;
- controle de estado de loading com `finalize`.

## Outras bibliotecas

- Angular Material: componentes visuais.
- Chart.js: gráficos analíticos.
- FluentValidation: validação do backend.
- Entity Framework Core: persistência e LINQ para SQL.
- Polly / Microsoft.Extensions.Http.Resilience: resiliência HTTP.
- ML.NET / Microsoft.ML.TimeSeries: previsão de consumo com séries temporais.
- xUnit e Moq: testes automatizados.

## Framework utilizado no backend

ASP.NET Core Web API em C#/.NET.

## 🚨 Tratamento de erros e exceções

Global Exception Handlers convertem exceções de validação, domínio e infraestrutura em respostas `ProblemDetails` com códigos HTTP apropriados.

Falhas originadas em um microsserviço são interpretadas pelo serviço consumidor para preservar o contexto do erro.

## Uso de LINQ

Sim.

LINQ é utilizado para filtros, ordenação, paginação, agregações, consultas de existência, cálculo de consumo diário, ranking de produtos e preparação de dados do dashboard e modelo preditivo.

---

# 🏁 Considerações finais

O objetivo da solução foi ir além de um CRUD simples e demonstrar decisões normalmente encontradas em aplicações distribuídas reais:

- separação entre microsserviços;
- bancos independentes;
- comunicação HTTP;
- resiliência;
- tratamento semântico de erros;
- concorrência;
- idempotência;
- consultas server-side;
- frontend consistente;
- análise operacional;
- previsão de consumo com Machine Learning.

Ao mesmo tempo, o domínio principal foi mantido aderente ao escopo proposto, evitando adicionar regras fiscais ou tributárias que não faziam parte do desafio.

---

## 👨‍💻 Autor

**Wallace Veridiano de Jesus**

Projeto desenvolvido para o desafio técnico da Korp.
