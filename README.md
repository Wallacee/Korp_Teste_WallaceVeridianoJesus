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

# 🚀 Como executar o projeto

Para facilitar tanto a análise técnica quanto a execução da solução, o projeto oferece **duas formas principais de inicialização**.

| Opção | Descrição | Indicada para |
|---|---|---|
| 💻 **Execução local** | APIs e frontend executados diretamente na máquina | Desenvolvimento, debugging e análise do código |
| 🐳 **Docker Compose** | Toda a infraestrutura e aplicações são construídas e inicializadas em containers | Avaliação rápida e ambiente reproduzível |

As duas modalidades executam a **mesma arquitetura e as mesmas funcionalidades**.

O uso de Docker não é obrigatório para executar o projeto. A modalidade local permite executar toda a solução utilizando .NET, Node.js e PostgreSQL instalados diretamente na máquina.

Para maior conveniência, a modalidade Docker Compose automatiza a criação da infraestrutura necessária e a inicialização dos componentes.

---

## 💻 Opção 1 — Execução local

Esta modalidade executa as APIs e o frontend diretamente na máquina, sem depender da containerização das aplicações.

É a alternativa recomendada para quem deseja:

- executar e depurar cada microsserviço individualmente;
- analisar o código e o fluxo entre as camadas;
- acompanhar diretamente as requisições entre Billing e Inventory;
- trabalhar com as migrations do Entity Framework;
- executar o Angular em modo de desenvolvimento.

### 📋 Pré-requisitos

Para executar a solução completamente sem Docker, são necessários:

- **.NET SDK** compatível com a solução;
- **PostgreSQL**;
- **Node.js**;
- **npm**;
- **Entity Framework Core CLI**.

Verifique as instalações:

```bash
dotnet --version
node --version
npm --version
```

Caso o Entity Framework Core CLI ainda não esteja instalado:

```bash
dotnet tool install --global dotnet-ef
```

Valide:

```bash
dotnet ef --version
```

### 🗄️ Criando os bancos de dados

A solução mantém bancos independentes para os dois microsserviços.

Conecte-se à sua instância PostgreSQL e crie:

```sql
CREATE DATABASE korp_inventory;
CREATE DATABASE korp_billing;
```

A arquitetura resultante será:

```text
Inventory Service ─────► korp_inventory
Billing Service   ─────► korp_billing
```

Os dois bancos podem existir na mesma instância PostgreSQL. A separação lógica evita que um microsserviço acesse diretamente as tabelas pertencentes ao outro.

### 🔐 Configurando as conexões

Configure o Inventory para utilizar o banco:

```text
Host=localhost;Port=5432;Database=korp_inventory;Username=postgres;Password=SUA_SENHA
```

E o Billing:

```text
Host=localhost;Port=5432;Database=korp_billing;Username=postgres;Password=SUA_SENHA
```

As credenciais podem ser definidas nos arquivos de configuração de desenvolvimento ou através de variáveis de ambiente.

> Para ambientes reais, recomenda-se não versionar credenciais ou outros dados sensíveis no repositório.

### 📦 Restaurando e compilando o backend

A partir da raiz do repositório:

```bash
dotnet restore
dotnet build
```

### 🗃️ Aplicando as migrations do Inventory

Na raiz do repositório:

```bash
dotnet ef database update --project src/backend/Inventory/Korp.Invoice.Inventory.Infrastructure --startup-project src/backend/Inventory/Korp.Invoice.Inventory.Api
```

Esse comando aplica as migrations existentes ao banco:

```text
korp_inventory
```

### 🗃️ Aplicando as migrations do Billing

Execute:

```bash
dotnet ef database update --project src/backend/Billing/Korp.Invoice.Billing.Infrastructure --startup-project src/backend/Billing/Korp.Invoice.Billing.Api
```

As migrations serão aplicadas ao banco:

```text
korp_billing
```

### 📦 Executando o Inventory Service

Abra um terminal na raiz do projeto:

```bash
dotnet run --project src/backend/Inventory/Korp.Invoice.Inventory.Api
```

O Inventory Service ficará responsável por produtos, disponibilidade e operações de estoque.

Mantenha esse terminal em execução.

### 🧾 Executando o Billing Service

Em outro terminal:

```bash
dotnet run --project src/backend/Billing/Korp.Invoice.Billing.Api
```

O Billing Service ficará responsável pelo gerenciamento e processamento das notas fiscais.

Durante o processamento de uma nota, o Billing realiza a comunicação com o Inventory através de HTTP:

```text
Billing Service ───── HTTP ─────► Inventory Service
```

Por isso, a URL local do Inventory configurada no Billing deve corresponder ao endereço em que a API foi inicializada.

### 🅰️ Instalando o frontend

Abra outro terminal e acesse:

```bash
cd src/frontend/Korp.Invoice.Web
```

Instale as dependências utilizando o lockfile do projeto:

```bash
npm ci
```

### 🌐 Executando o Angular

Execute:

```bash
npm start
```

A aplicação poderá então ser acessada em:

```text
http://localhost:4200
```

Com todos os componentes inicializados, o ambiente local estará estruturado da seguinte forma:

```text
PostgreSQL
   │
   ├── korp_inventory ◄──── Inventory API
   │                              ▲
   │                              │ HTTP
   │                              │
   └── korp_billing   ◄──── Billing API
                                  ▲
                                  │
                              Angular Web
```

> Caso não queira instalar PostgreSQL localmente, também é possível utilizar apenas o banco em container e continuar executando as APIs e o Angular localmente.

---

## 🐳 Opção 2 — Docker Compose

A segunda modalidade permite executar a solução completa utilizando **Docker Compose**.

Diferentemente da execução local, não é necessário iniciar manualmente cada API, preparar individualmente os bancos ou executar o frontend em um terminal separado.

O Docker Compose será responsável por orquestrar os componentes necessários para execução da solução.

### 📋 Pré-requisitos

Para esta modalidade são necessários:

- **Git**
- **Docker**
- **Docker Compose**

Verifique:

```bash
docker --version
docker compose version
```

### 🏗️ Componentes do ambiente

A execução containerizada será composta por:

```text
Docker Compose
│
├── PostgreSQL
│   ├── korp_inventory
│   └── korp_billing
│
├── Inventory API
│
├── Billing API
│
└── Angular Web
```

As imagens das aplicações são construídas diretamente a partir dos **Dockerfiles e do código-fonte presentes neste repositório**.

Isso significa que esta modalidade não depende de imagens previamente preparadas para executar a aplicação.

### 🚀 Inicialização

Após clonar o repositório:

```bash
git clone <URL_DO_REPOSITORIO>
cd Korp_Teste_WallaceVeridianoJesus
```

A execução completa será realizada através de:

```bash
docker compose up -d --build
```

> As próximas subseções detalham a criação das imagens, inicialização dos bancos, aplicação das migrations, health checks, comunicação entre os containers e formas de inspecionar ou reinicializar o ambiente.
