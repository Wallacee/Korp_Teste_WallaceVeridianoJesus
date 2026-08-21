# 🧾 Korp Invoice

Sistema de emissão e gerenciamento de notas fiscais desenvolvido como projeto técnico, utilizando **Angular e .NET**, com arquitetura baseada em **microsserviços**, persistência em **PostgreSQL** e comunicação HTTP entre os serviços.

A solução foi estruturada priorizando separação de responsabilidades, regras de domínio, tratamento de falhas, testabilidade e uma experiência simples para execução e avaliação do projeto.

## 🏗️ Visão geral da solução

A aplicação é composta por dois microsserviços principais:

- **Inventory Service** — responsável pelo cadastro de produtos, controle de estoque e processamento das movimentações.
- **Billing Service** — responsável pelo ciclo de vida das notas fiscais, seus itens, numeração sequencial e processamento.

O frontend foi desenvolvido em **Angular** e consome as APIs responsáveis pelos respectivos domínios.

Cada microsserviço possui seu próprio banco lógico PostgreSQL:

```text
                         ┌─────────────────────┐
                         │     Angular Web     │
                         └──────────┬──────────┘
                                    │
                     ┌──────────────┴──────────────┐
                     │                             │
                     ▼                             ▼
          ┌────────────────────┐        ┌────────────────────┐
          │ Inventory Service  │◄───────│  Billing Service   │
          │       .NET         │  HTTP  │       .NET         │
          └─────────┬──────────┘        └─────────┬──────────┘
                    │                             │
                    ▼                             ▼
          ┌────────────────────┐        ┌────────────────────┐
          │   korp_inventory   │        │    korp_billing    │
          │     PostgreSQL     │        │     PostgreSQL     │
          └────────────────────┘        └────────────────────┘
```

Além dos requisitos funcionais propostos no desafio, a solução contempla recursos adicionais como tratamento de concorrência, idempotência, resiliência entre microsserviços, testes automatizados, dashboard analítico e previsão inteligente de consumo.

---


## 📁 Estrutura do projeto

A solução foi organizada em dois microsserviços independentes, um projeto compartilhado, frontend e projetos de testes:

```text
Korp_Teste_WallaceVeridianoJesus/
│
├── docker/
│   └── postgres/
│       └── init-multiple-databases.sql
│
├── src/
│   ├── backend/
│   │   ├── Inventory/
│   │   │   ├── Korp.Invoice.Inventory.Api/
│   │   │   ├── Korp.Invoice.Inventory.Application/
│   │   │   ├── Korp.Invoice.Inventory.Domain/
│   │   │   └── Korp.Invoice.Inventory.Infrastructure/
│   │   ├── Billing/
│   │   │   ├── Korp.Invoice.Billing.Api/
│   │   │   ├── Korp.Invoice.Billing.Application/
│   │   │   ├── Korp.Invoice.Billing.Domain/
│   │   │   └── Korp.Invoice.Billing.Infrastructure/
│   │   └── shared/
│   │       └── Korp.Invoice.Shared/
│   └── frontend/
│       └── Korp.Invoice.Web/
│
├── tests/
│   ├── Korp.Invoice.Inventory.UnitTests/
│   └── Korp.Invoice.Billing.UnitTests/
│
├── Directory.Build.props
├── docker-compose.yml
├── Korp.Invoice.slnx
└── README.md
```

| Projeto | Responsabilidade |
|---|---|
| **Api** | Endpoints REST, configuração e tratamento global |
| **Application** | Casos de uso, DTOs, validações e orquestração |
| **Domain** | Entidades, invariantes, regras e exceções |
| **Infrastructure** | EF Core, PostgreSQL, repositories, migrations e integrações |

O `Korp.Invoice.Shared` concentra contratos compartilhados necessários à comunicação entre os serviços, sem compartilhar diretamente seus modelos de persistência.

---

# 🚀 Como executar o projeto

A solução pode ser executada de **duas formas**, dependendo apenas de como o PostgreSQL será disponibilizado.

| Opção | Banco de dados | APIs e frontend |
|---|---|---|
| 💻 **Execução totalmente local** | PostgreSQL 18 instalado localmente | Executados localmente |
| 🐳 **PostgreSQL via Docker** | PostgreSQL 18 em container | Executados localmente |

Em ambas as opções, o **Inventory Service**, o **Billing Service** e o **frontend Angular** são executados diretamente na máquina.

A arquitetura, migrations e funcionalidades são exatamente as mesmas nas duas modalidades.

---

## 📋 1. Pré-requisitos

A solução foi desenvolvida e validada utilizando:

| Tecnologia | Versão |
|---|---:|
| .NET SDK | `10.0.400` |
| PostgreSQL | `18` |
| Node.js | `24.19.0` |
| npm | `11.11.1` |
| Angular | `22.1.x` |
| Angular CLI | `22.1.4` |
| Angular Material | `22.1.2` |
| TypeScript | `6.0.2` |
| RxJS | `7.8.x` |

Para utilizar a opção com PostgreSQL em container também são necessários:

- Docker
- Docker Compose

As principais versões instaladas podem ser verificadas com:

```bash
dotnet --version
node --version
npm --version
docker --version
docker compose version
```

Caso o Entity Framework Core CLI ainda não esteja instalado:

```bash
dotnet tool install --global dotnet-ef
```

---

## 📥 2. Clonar o repositório

Clone o projeto:

```bash
git clone https://github.com/Wallacee/Korp_Teste_WallaceVeridianoJesus.git
```

> Os comandos das próximas etapas consideram a **raiz do repositório** como diretório atual, salvo quando indicado o contrário.

---

## ⚠️ 3. Verificar as portas

Antes de iniciar os componentes, verifique se as portas configuradas para PostgreSQL, APIs e frontend estão disponíveis.

Por exemplo, para verificar a porta padrão do PostgreSQL no Windows:

```powershell
netstat -ano | findstr :5432
```

Para identificar o processo utilizando uma porta:

```powershell
tasklist /FI "PID eq <PID>"
```

Faça a mesma verificação para as portas configuradas nas APIs e para a porta `4200` utilizada pelo Angular.

> ⚠️ **Importante:** conflitos de porta podem impedir a inicialização do PostgreSQL, das APIs ou do frontend. Caso alguma porta esteja ocupada, libere-a ou ajuste a configuração correspondente antes de continuar.

---

# 🗄️ 4. Disponibilizar o PostgreSQL

Escolha **uma das duas opções abaixo**.

---

## 💻 Opção 1 — PostgreSQL 18 local

Tenha uma instância do **PostgreSQL 18** instalada e em execução.

Crie os bancos utilizados pelos dois microsserviços:

```sql
CREATE DATABASE korp_inventory;
CREATE DATABASE korp_billing;
```

Cada microsserviço possui seu próprio banco:

```text
Inventory Service → korp_inventory
Billing Service   → korp_billing
```

Configure as connection strings das APIs de acordo com as credenciais da instalação local.

Exemplo:

```text
Host=localhost;Port=5432;Database=korp_inventory;Username=postgres;Password=SUA_SENHA
Host=localhost;Port=5432;Database=korp_billing;Username=postgres;Password=SUA_SENHA
```

Com os dois bancos disponíveis, prossiga para o **Passo 5 — Preparar o backend**.

---

## 🐳 Opção 2 — PostgreSQL 18 via Docker

Nesta modalidade, o Docker é utilizado **exclusivamente para disponibilizar o PostgreSQL 18**.

As APIs .NET e o frontend Angular continuam sendo executados localmente.

O `docker-compose.yml` utiliza a imagem:

```yaml
image: postgres:18
```

### 🐘 Subir o PostgreSQL

Na raiz do repositório:

```bash
docker compose up -d postgres
```

Verifique o estado:

```bash
docker compose ps
```

Aguarde até o container `korp-postgres` apresentar:

```text
healthy
```

> ⚠️ **Importante:** `Started` significa apenas que o container foi iniciado. Aguarde o healthcheck retornar `healthy` antes de continuar.

Caso o container seja encerrado durante a inicialização:

```bash
docker compose ps -a
docker compose logs postgres
```

### 🗃️ Criação automática dos bancos

Na primeira inicialização de um volume novo, são criados:

```text
korp_inventory
korp_billing
```

O `korp_inventory` é criado pela configuração do container.

O `korp_billing` é criado através do script versionado em:

```text
docker/postgres/init-multiple-databases.sql
```

Para verificar os bancos:

```bash
docker compose exec postgres psql -U postgres -l
```

O resultado deverá conter:

```text
korp_inventory
korp_billing
```

### ⚠️ Importante — volumes PostgreSQL

Os scripts disponíveis em:

```text
/docker-entrypoint-initdb.d/
```

são executados pelo PostgreSQL **somente durante a primeira inicialização de um volume vazio**.

Portanto, alterações posteriores no script de inicialização não são executadas novamente enquanto o volume existente estiver sendo reutilizado.

Para recriar completamente o ambiente:

```bash
docker compose down -v
docker compose up -d postgres
```

Depois:

```bash
docker compose ps
```

e aguarde novamente o estado:

```text
healthy
```

> ⚠️ **Atenção:** `docker compose down -v` remove permanentemente todos os dados armazenados no volume PostgreSQL. Utilize-o somente quando desejar recriar o ambiente do zero.

---

# 🔧 5. Preparar o backend

Com o PostgreSQL disponível, restaure as dependências NuGet da solução.

Na raiz do repositório:

```bash
dotnet restore
```

Compile a solução:

```bash
dotnet build --no-restore
```

Essa etapa garante que todas as dependências foram restauradas e que os projetos estão compilando corretamente antes da preparação dos bancos.

---

# 🗃️ 6. Aplicar as migrations

Com:

```text
korp_inventory ✅
korp_billing   ✅
```

disponíveis, aplique as migrations de cada microsserviço.

### 📦 Inventory Service

```bash
dotnet ef database update --project src/backend/Inventory/Korp.Invoice.Inventory.Infrastructure --startup-project src/backend/Inventory/Korp.Invoice.Inventory.Api
```

### 🧾 Billing Service

```bash
dotnet ef database update --project src/backend/Billing/Korp.Invoice.Billing.Infrastructure --startup-project src/backend/Billing/Korp.Invoice.Billing.Api
```

Cada microsserviço mantém suas próprias migrations e persistência:

```text
Inventory Service ──► korp_inventory
Billing Service   ──► korp_billing
```

### 🔎 Verificação opcional

Para confirmar que o modelo atual está sincronizado com as migrations:

**Inventory:**

```bash
dotnet ef migrations has-pending-model-changes --project src/backend/Inventory/Korp.Invoice.Inventory.Infrastructure --startup-project src/backend/Inventory/Korp.Invoice.Inventory.Api
```

**Billing:**

```bash
dotnet ef migrations has-pending-model-changes --project src/backend/Billing/Korp.Invoice.Billing.Infrastructure --startup-project src/backend/Billing/Korp.Invoice.Billing.Api
```

---

# 🅰️ 7. Preparar o frontend

Abra outro terminal e acesse o projeto Angular:

```bash
cd src/frontend/Korp.Invoice.Web
```

Instale as dependências exatamente conforme o `package-lock.json`:

```bash
npm ci
```

Valide a compilação:

```bash
npm run build
```

Com o build concluído, o frontend está pronto para execução.

---

# ▶️ 8. Iniciar a aplicação

A solução possui **três processos locais independentes**.

Utilize um terminal para cada processo.

---

### 📦 Terminal 1 — Inventory Service

A partir da raiz do repositório:

```bash
dotnet run --no-build --project src/backend/Inventory/Korp.Invoice.Inventory.Api
```

Mantenha o processo em execução.

---

### 🧾 Terminal 2 — Billing Service

A partir da raiz do repositório:

```bash
dotnet run --no-build --project src/backend/Billing/Korp.Invoice.Billing.Api
```

Mantenha o processo em execução.

> ℹ️ O **Billing Service se comunica com o Inventory Service via HTTP** durante o processamento das notas fiscais. Ambos devem estar disponíveis para o funcionamento completo do fluxo de faturamento.

---

### 🅰️ Terminal 3 — Angular

Acesse o frontend:

```bash
cd src/frontend/Korp.Invoice.Web
```

Execute:

```bash
npm start
```

A aplicação estará disponível em:

```text
http://localhost:4200
```

---

# ✅ Ambiente pronto

Após concluir as etapas anteriores, a solução estará executando:

```text
                       ┌────────────────────────┐
                       │       Angular 22       │
                       │    localhost:4200      │
                       └───────────┬────────────┘
                                   │
                    ┌──────────────┴──────────────┐
                    │                             │
                    ▼                             ▼
        ┌─────────────────────┐       ┌─────────────────────┐
        │  Inventory Service  │◄─HTTP─│   Billing Service   │
        │      .NET 10        │       │      .NET 10        │
        └──────────┬──────────┘       └──────────┬──────────┘
                   │                             │
                   ▼                             ▼
        ┌─────────────────────┐       ┌─────────────────────┐
        │   korp_inventory    │       │    korp_billing     │
        │    PostgreSQL 18    │       │    PostgreSQL 18    │
        └─────────────────────┘       └─────────────────────┘
```

### 🧭 Resumo da primeira execução

```text
Clone do repositório
        │
        ▼
Verificação dos pré-requisitos
        │
        ▼
Verificação das portas
        │
        ▼
Disponibilizar PostgreSQL 18
        │
        ├── PostgreSQL local
        │
        └── PostgreSQL via Docker
        │
        ▼
dotnet restore
        │
        ▼
dotnet build
        │
        ▼
Migrations Inventory + Billing
        │
        ▼
npm ci
        │
        ▼
npm run build
        │
        ▼
Inventory API + Billing API + Angular
        │
        ▼
   ✅ Aplicação pronta
```

Na **Opção 1**, o PostgreSQL 18 é executado diretamente na máquina.

Na **Opção 2**, somente o PostgreSQL 18 é executado em container; **Inventory API, Billing API e Angular continuam sendo executados localmente**.

Independentemente da opção escolhida, a aplicação utiliza **a mesma arquitetura, os mesmos bancos lógicos, as mesmas migrations e as mesmas funcionalidades**.
---

# 🧪 Testes automatizados e relatórios

A solução possui testes automatizados no backend e no frontend, com foco em regras de negócio, comportamento dos componentes e cenários críticos.

## ⚙️ Backend — xUnit

Na raiz do repositório:

```bash
dotnet test
```

Também é possível executar os projetos separadamente:

```bash
dotnet test tests/Korp.Invoice.Inventory.UnitTests/Korp.Invoice.Inventory.UnitTests.csproj
```

```bash
dotnet test tests/Korp.Invoice.Billing.UnitTests/Korp.Invoice.Billing.UnitTests.csproj
```

Os testes cobrem, entre outros cenários:

- validações de produtos e quantidades;
- débito de estoque e saldo insuficiente;
- regras de notas abertas e fechadas;
- criação e processamento de notas;
- falhas na integração com o Inventory;
- idempotência;
- comportamento esperado em cenários de concorrência.

### 📊 Cobertura e relatório HTML do backend

Gere a cobertura:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

Instale o ReportGenerator:

```bash
dotnet tool install --global dotnet-reportgenerator-globaltool
```

Gere o relatório consolidado:

```bash
reportgenerator "-reports:**/TestResults/**/coverage.cobertura.xml" "-targetdir:coverage/backend" "-reporttypes:Html"
```

Abra:

```text
coverage/backend/index.html
```

## 🅰️ Frontend — Vitest

Acesse:

```bash
cd src/frontend/Korp.Invoice.Web
```

Execute os testes:

```bash
npm test -- --watch=false
```

Os testes do frontend validam comportamentos como carregamento, busca com debounce, paginação, ordenação, exclusão, conflitos de negócio e feedback ao usuário.

### 📈 Relatório web de cobertura do frontend

Execute:

```bash
npm test -- --coverage --watch=false
```

O relatório HTML é gerado no diretório de cobertura do projeto e apresenta métricas de **Statements, Branches, Functions e Lines**.

---

# ✨ Funcionalidades implementadas

## 📦 Produtos

- cadastro, edição e exclusão;
- código, descrição e saldo;
- pesquisa por código ou descrição;
- paginação e ordenação server-side;
- validações no frontend e backend;
- bloqueio de exclusão quando o produto está associado a nota fiscal.

Como Inventory e Billing possuem bancos independentes, a verificação de vínculo é feita via comunicação entre os microsserviços.

## 🧾 Notas fiscais

- numeração sequencial;
- status `Aberta` e `Fechada`;
- múltiplos produtos e quantidades;
- consulta e detalhamento;
- edição e exclusão enquanto abertas;
- bloqueio de alterações após fechamento.

## 🖨️ Impressão e fechamento

```text
Nota Aberta
    │
    ▼
Usuário solicita impressão
    │
    ▼
Indicador de processamento
    │
    ▼
Billing solicita débito ao Inventory
    │
    ▼
Inventory valida e atualiza o estoque
    │
    ▼
Billing fecha a nota
    │
    ▼
Documento é enviado para impressão
```

O fluxo garante atualização do estoque, fechamento da nota somente após sucesso e impedimento de novo processamento de uma nota já fechada.

---

# 🏗️ Arquitetura e decisões técnicas

A solução utiliza dois microsserviços com responsabilidades e persistências independentes.

### Inventory Service

Responsável por produtos, saldo, movimentações, processamento de estoque, idempotência e concorrência.

### Billing Service

Responsável por notas fiscais, numeração, itens, status, processamento e coordenação da integração com Inventory.

A comunicação é feita por **HTTP/REST**. Nenhum serviço acessa diretamente as tabelas do outro contexto.

## 🧱 Backend

O backend foi implementado em **C# / .NET 10 com ASP.NET Core Web API**.

A separação em `Api`, `Application`, `Domain` e `Infrastructure` mantém regras de negócio fora dos controllers e infraestrutura desacoplada do domínio.

Exemplos de invariantes:

- saldo não pode ser debitado além do disponível;
- quantidade deve ser válida;
- nota fechada não pode ser alterada;
- nota precisa possuir itens para ser processada;
- o mesmo produto não pode ser incluído duas vezes na mesma nota.

## 🗄️ Persistência

A solução utiliza **PostgreSQL 18**, Entity Framework Core, migrations, repositories e Unit of Work.

```text
InventoryDbContext → korp_inventory
BillingDbContext   → korp_billing
```

Filtros, ordenação, paginação e agregações são executados no banco sempre que possível.

---

# 🔎 Uso de LINQ

LINQ é utilizado para filtros, projeções, paginação, ordenação, consultas de existência e agregações.

Exemplos:

```text
Where
Select
SelectMany
GroupBy
OrderBy / OrderByDescending
Skip / Take
Any / AnyAsync
Count / CountAsync
Sum / SumAsync
```

Consultas baseadas em `IQueryable` são traduzidas pelo Entity Framework Core para SQL e executadas pelo PostgreSQL.

---

# ✅ Validação, erros e feedback

## FluentValidation

Requests da Application utilizam **FluentValidation**. Invariantes permanecem protegidas pelo domínio.

## ProblemDetails e tratamento global

As APIs utilizam tratamento global de exceções com respostas padronizadas em `ProblemDetails`.

| Status | Situação |
|---|---|
| `400 Bad Request` | entrada ou validação inválida |
| `404 Not Found` | recurso não encontrado |
| `409 Conflict` | conflito de negócio, estoque insuficiente ou concorrência |
| `503 Service Unavailable` | serviço externo indisponível ou timeout |
| `500 Internal Server Error` | erro inesperado |

Erros do Inventory são interpretados pelo Billing antes de chegar ao frontend, preservando mensagens de negócio e evitando feedback genérico.

---

# 🛡️ Resiliência entre microsserviços

A integração Billing → Inventory utiliza `HttpClient` com recursos de **Microsoft.Extensions.Http.Resilience / Polly**.

Foram tratados:

- timeout;
- falhas transitórias;
- indisponibilidade;
- retry quando aplicável;
- diferenciação entre falha técnica e conflito de negócio.

### Cenário obrigatório de falha

```text
Billing
   │
   ▼
Inventory indisponível / timeout
   │
   ▼
Política de resiliência
   │
   ▼
Billing retorna indisponibilidade
   │
   ▼
Angular apresenta feedback
```

A nota não é fechada como se a operação tivesse sido concluída.

---

# ⚡ Concorrência

O requisito opcional de concorrência foi implementado no estoque por meio de controle otimista de versão com PostgreSQL/EF Core.

```text
Saldo = 1

Nota A ───┐
          ├── tentam consumir simultaneamente
Nota B ───┘
```

Apenas uma operação deve confirmar o último saldo disponível, mantendo o estoque consistente.

---

# 🔁 Idempotência

Cada processamento de estoque possui um `OperationId`.

O Inventory registra operações em `StockOperations`, com unicidade para esse identificador.

```text
OperationId ABC
   │
   ├── 1ª execução: debita e registra
   └── nova execução: não debita novamente
```

Isso evita efeitos colaterais duplicados em retry, timeout ou reenvio da mesma requisição.

---

# 📊 Dashboard operacional

A visão geral consolida dados de Inventory e Billing, incluindo:

- produtos cadastrados;
- saldo total;
- produtos com estoque baixo;
- notas abertas;
- notas fechadas;
- unidades processadas;
- histórico de consumo;
- previsão inteligente de consumo.

Os gráficos são renderizados com **Chart.js**.

---

# 🤖 Inteligência Artificial — previsão de consumo

Como requisito opcional, foi implementada previsão de consumo utilizando **ML.NET** e **Microsoft.ML.TimeSeries**.

O histórico de movimentação é utilizado como série temporal para estimar comportamento futuro:

```text
Histórico de consumo
        │
        ▼
Série temporal
        │
        ▼
ML.NET
        │
        ▼
Previsão
        │
        ▼
Dashboard
```

Quando não existe histórico suficiente, a interface informa que ainda não há dados adequados para gerar uma previsão confiável.

---

# 🅰️ Frontend Angular

O frontend foi desenvolvido em **Angular 22**, com componentes standalone e organização por features.

```text
src/app/
├── features/
│   ├── dashboard/
│   ├── products/
│   └── invoices/
├── layout/
└── shared/
```

Principais recursos:

- Angular Material;
- Reactive Forms;
- `FormArray`;
- Angular Signals;
- RxJS;
- Chart.js;
- SCSS;
- componentes e estilos compartilhados.

## ♻️ Ciclos de vida do Angular

- `OnInit`: carregamento inicial, parâmetros de rota e inicialização de telas;
- `OnDestroy`: liberação de recursos como instâncias do Chart.js;
- `DestroyRef` + `takeUntilDestroyed`: encerramento seguro de subscriptions;
- `ViewChild`: acesso aos elementos `canvas` utilizados pelos gráficos.

## 🔀 RxJS

Principais recursos utilizados:

- `debounceTime`;
- `distinctUntilChanged`;
- `switchMap`;
- `forkJoin`;
- `finalize`;
- `map`;
- `of`;
- `startWith`;
- `valueChanges`.

No autocomplete de produtos, a pesquisa é server-side e usa debounce para reduzir chamadas desnecessárias.

---

# 🎨 Tecnologias e bibliotecas

| Camada | Tecnologia | Finalidade |
|---|---|---|
| Frontend | Angular 22 | SPA e componentes |
| Frontend | Angular Material | componentes visuais |
| Frontend | RxJS | fluxos reativos |
| Frontend | Angular Signals | estado local |
| Frontend | Reactive Forms | formulários e validação |
| Frontend | Chart.js | gráficos |
| Frontend | Vitest | testes |
| Backend | ASP.NET Core Web API / .NET 10 | APIs |
| Backend | Entity Framework Core | ORM e persistência |
| Backend | FluentValidation | validação |
| Backend | HttpClient | comunicação HTTP |
| Backend | Microsoft.Extensions.Http.Resilience / Polly | resiliência |
| Backend | ML.NET / Microsoft.ML.TimeSeries | previsão |
| Banco | PostgreSQL 18 | persistência |
| Testes | xUnit | testes backend |
| Testes | Moq | mocks |
| Infraestrutura | Docker Compose | provisionamento opcional do PostgreSQL |

> O item de gerenciamento de dependências em **Golang** não se aplica, pois o backend foi implementado integralmente em C#/.NET.

---

# 📋 Atendimento ao desafio

## Requisitos obrigatórios

| Requisito | Status |
|---|:---:|
| Cadastro de produtos: código, descrição e saldo | ✅ |
| Persistência real em banco | ✅ |
| Cadastro de notas fiscais | ✅ |
| Numeração sequencial | ✅ |
| Status Aberta / Fechada | ✅ |
| Múltiplos produtos e quantidades | ✅ |
| Botão de impressão | ✅ |
| Indicador durante processamento | ✅ |
| Fechamento após processamento | ✅ |
| Bloqueio de novo processamento de nota fechada | ✅ |
| Atualização do estoque | ✅ |
| Inventory Service | ✅ |
| Billing Service | ✅ |
| Comunicação entre microsserviços | ✅ |
| Cenário de falha entre serviços | ✅ |
| Feedback apropriado ao usuário | ✅ |

## Requisitos opcionais

| Requisito | Status | Implementação |
|---|:---:|---|
| Concorrência | ✅ | controle otimista no estoque |
| Idempotência | ✅ | `OperationId` + `StockOperations` |
| Inteligência Artificial | ✅ | previsão de consumo com ML.NET |

---

# 📚 Detalhamento técnico solicitado

| Item solicitado | Implementação |
|---|---|
| Ciclos de vida Angular | `OnInit`, `OnDestroy`, `DestroyRef`, `ViewChild` |
| Uso de RxJS | debounce, autocomplete, composição de chamadas e loading |
| Outras bibliotecas | Angular Material, Chart.js, FluentValidation, EF Core, Polly, ML.NET |
| Biblioteca visual | Angular Material |
| Gerenciamento de dependências Golang | Não aplicável |
| Framework C# | ASP.NET Core Web API / .NET 10 |
| Tratamento de erros | exceções específicas, handler global, `ProblemDetails`, HTTP semântico |
| LINQ | filtros, projeções, paginação, ordenação e agregações |

---


# 🏁 Considerações finais

O projeto foi mantido aderente ao domínio solicitado, evitando ampliar artificialmente o escopo fiscal ou tributário.

Além dos requisitos obrigatórios, foram incorporadas decisões relevantes para aplicações distribuídas: microsserviços e bancos independentes, resiliência, idempotência, concorrência, testes automatizados, dashboard e análise preditiva.

O objetivo foi entregar uma solução cuja arquitetura, comportamento e decisões técnicas possam ser explicados, reproduzidos e testados.

---

## 👨‍💻 Autor

**Wallace Veridiano de Jesus**

Projeto desenvolvido para o desafio técnico **Korp — Sistema de emissão de Notas Fiscais**.
