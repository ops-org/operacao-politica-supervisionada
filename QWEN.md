# Operação Política Supervisionada - Contexto do Projeto

## Visão Geral

**Operação Política Supervisionada (OPS)** é um sistema de auditoria pública de gastos com a Cota para Exercício da Atividade Parlamentar de Deputados Federais, Deputados Estaduais e Senadores. O projeto tem fins sociais e políticos, sem vínculo partidário ou lucrativo, permitindo que cidadãos acompanhem e fiscalizem o uso de recursos públicos.

## Stack Tecnológico

### Backend
- **.NET 10.0** com ASP.NET Core Web API
- **Entity Framework Core 10.0** para acesso a dados
- **PostgreSQL 15** como banco de dados
- **Dapper** para consultas otimizadas
- **Aspire** para orquestração de serviços

### Frontend
- **React 19** com TypeScript
- **Vite** como bundler
- **TailwindCSS** para estilização
- **shadcn/ui** e **Radix UI** para componentes
- **React Router** para navegação
- **TanStack Query** para gerenciamento de estado do servidor

### Infraestrutura
- **Docker** e **Docker Compose** para containerização
- **Playwright** para testes E2E
- **xunit** para testes unitários

## Estrutura do Projeto

```
operacao-politica-supervisionada/
├── OPS.API/                    # API RESTful principal
│   ├── Controllers/            # Endpoints da API
│   ├── Services/               # Serviços de negócio
│   ├── Configuration/          # Configurações
│   └── Program.cs              # Ponto de entrada
├── OPS.Core/                   # Camada de domínio
│   ├── DTOs/                   # Data Transfer Objects
│   └── Models/                 # Entidades de negócio
├── OPS.Infraestrutura/         # Acesso a dados (EF Core)
│   └── Entities/               # Entidades do banco
├── OPS.Site/                   # Frontend React/TypeScript
│   ├── src/
│   │   ├── components/         # Componentes React
│   │   ├── pages/              # Páginas da aplicação
│   │   └── hooks/              # Custom hooks
│   └── public/                 # Assets estáticos
├── OPS.Importador/             # Processos de importação de dados
├── OPS.AppHost/                # Host Aspire para orquestração
├── OPS.ServiceDefaults/        # Configurações padrão de serviços
├── OPS.API.Tests/              # Testes da API (xunit)
├── OPS.Core.Test/              # Testes do Core
└── Docs/
    └── BD/
        └── Schemas/            # Scripts SQL do banco
```

## Projetos .NET

| Projeto | Descrição |
|---------|-----------|
| `OPS.API` | Web API principal com controllers e serviços |
| `OPS.Core` | Regras de negócio, DTOs e modelos de domínio |
| `OPS.Infraestrutura` | Contexto EF Core e repositórios |
| `OPS.Site` | Frontend em React (esproj) |
| `OPS.Importador` | Importação de dados das fontes públicas |
| `OPS.AppHost` | Orquestrador Aspire |
| `OPS.ServiceDefaults` | Configurações compartilhadas |
| `OPS.API.Tests` | Testes da API com xunit v3 |

## Building e Running

### Pré-requisitos
- .NET 10.0 SDK
- Node.js 18+
- PostgreSQL 15
- Docker (opcional)

### Usando Docker Compose (Recomendado)

```bash
# Iniciar todos os serviços (banco, API, frontend)
docker-compose up -d
```

Serviços disponíveis:
- **PostgreSQL**: `localhost:5433`
- **API**: `http://localhost:5200`
- **Frontend**: `http://localhost:8080`

### Desenvolvimento Local

#### Backend

```bash
# Restaurar dependências
dotnet restore

# Rodar a API (projeto OPS.API)
cd OPS.API
dotnet run

# Rodar testes
dotnet test
```

A API estará disponível em `http://localhost:5200`

#### Frontend

```bash
cd OPS.Site

# Instalar dependências
npm install

# Rodar em modo desenvolvimento
npm run dev

# Build de produção
npm run build

# Rodar testes E2E
npm test
```

O frontend estará disponível em `http://localhost:8080`

### Comandos NPM Disponíveis

| Comando | Descrição |
|---------|-----------|
| `npm run dev` | Inicia servidor de desenvolvimento Vite |
| `npm run build` | Build de produção |
| `npm run build:dev` | Build em modo development |
| `npm run lint` | Executa ESLint |
| `npm run preview` | Preview do build |
| `npm test` | Testes E2E com Playwright |
| `npm run test:ui` | Testes com interface UI |
| `npm run test:debug` | Testes em modo debug |

## Banco de Dados

### Schemas Disponíveis

Os scripts de schema estão em `Docs/BD/Schemas/`:

| Arquivo | Descrição |
|---------|-----------|
| `CamaraFederal.sql` | Tabelas da Câmara dos Deputados (prefixo `cf_`) |
| `SenadoFederal.sql` | Tabelas do Senado Federal (prefixo `sf_`) |
| `AssembleiasLegislativas.sql` | Tabelas das Assembleias Estaduais (prefixo `cl_`) |
| `Fornecedor.sql` | Dados de fornecedores/CNPJ |
| `Comum.sql` | Tabelas compartilhadas |
| `Temp.sql` | Tabelas temporárias |
| `TribunalSuperiorEleitoral.sql` | Dados do TSE |

### Configuração do PostgreSQL

Via Docker Compose:
- Host: `localhost`
- Porta: `5433`
- Database/User/Password: Definidos no arquivo `.env`

## Fontes de Dados

O sistema importa dados de:

### Dados Parlamentares
- **CEAP** - Câmara dos Deputados
- **CEAPS** - Senado Federal
- **Assembleias Legislativas Estaduais**

### Dados de Fornecedores
- **Receita Federal** - Cadastro de CNPJ
- **Minha Receita** - API de consulta

## Convenções de Desenvolvimento

### C# / .NET

- **Indentação**: 4 espaços
- **End of line**: CRLF
- **Naming**: PascalCase para classes, métodos, propriedades
- **Interfaces**: Prefixo `I` (ex: `IService`)
- **Usings**: Fora do namespace
- **Pattern matching**: Preferido sobre `as` com null check
- **Expressões-bodied**: Para membros simples
- **Nullable reference types**: Habilitados

Configurações no `.editorconfig` com regras StyleCop e SonarLint.

### TypeScript / React

- **Componentes**: Functional components com hooks
- **Estilização**: TailwindCSS com `clsx` e `tailwind-merge`
- **Gerenciamento de estado**: TanStack Query para dados do servidor
- **Formulários**: React Hook Form
- **Roteamento**: React Router DOM v7
- **Linting**: ESLint com configs do `eslint-plugin-react-hooks`

### Testes

- **Backend**: xunit v3 com Shouldly para asserções
- **Frontend**: Playwright para testes E2E
- **Cobertura**: coverlet.collector

## Integração Contínua

- **SonarLint**: Análise estática de código C#
- **StyleCop**: Padrões de código .NET
- **ESLint**: Qualidade de código TypeScript/React

## Contribuição

1. Clone o repositório
2. Configure o banco PostgreSQL e importe os schemas
3. Backend: Abra a solução no Visual Studio, defina `OPS.API` como startup
4. Frontend: Execute `npm install` e `npm run dev`
5. Consulte `CONTRIBUTING.md` para diretrizes detalhadas

## Contatos e Links

- **Homepage**: http://ops.net.br
- **Email principal**: luciobig@ops.net.br
- **Suporte técnico**: suporte@ops.net.br
- **Facebook**: [Operação Política Supervisionada](https://www.facebook.com/operacaopoliticasupervisionada)
- **Telegram**: [Robops](https://t.me/joinchat/ByZCHlJ3VPEc8guzFbaybQ)

## Licença

O projeto é aberto e sem fins lucrativos, focado em transparência pública e controle social.
