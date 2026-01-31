
![Operação Política Supervisionada](https://static.ops.org.br/logo.png)

## Operação Política Supervisionada

**Sistema para auditoria pública de gastos com a Cota para Exercício da Atividade Parlamentar dos Deputados Federais, Deputados Estaduais e Senadores.**

[![Crates.io](https://img.shields.io/crates/l/rustc-serialize.svg?maxAge=2592000)]()
[![.NET](https://img.shields.io/badge/.NET-512BD4?logo=dotnet&logoColor=fff)](#)
[![Postgres](https://img.shields.io/badge/Postgres-%23316192.svg?logo=postgresql&logoColor=white)](#)
[![React](https://img.shields.io/badge/React-%2361DAFB.svg?logo=react&logoColor=white)](#)
[![TypeScript](https://img.shields.io/badge/TypeScript-%233178C6.svg?logo=typescript&logoColor=white)](#)

## Tecnologias

- **Backend**: .NET 10.0 com ASP.NET Core Web API
- **Frontend**: React 18 com TypeScript, Vite, TailwindCSS e shadcn/ui
- **Banco de Dados**: PostgreSQL 15
- **Importação de Dados**: Processos automatizados para importação de dados da Câmara Federal, Senado e Assembleias Legislativas Estaduais

## Estrutura do Projeto

- `OPS.API/` - API RESTful em .NET para acesso aos dados
- `OPS.Site/` - Frontend em React/TypeScript com interface moderna e responsiva
- `OPS.Core/` - Camada de domínio com DTOs e regras de negócio
- `OPS.Infraestrutura/` - Camada de acesso a dados com Entity Framework Core
- `OPS.Importador/` - Utilitários para importação e processamento de dados públicos

## Desenvolvimento

### Pré-requisitos

- .NET 10.0 SDK
- Node.js 18+ 
- PostgreSQL 15
- Docker (opcional, para container do PostgreSQL)

### Configuração do Ambiente

1. **Banco de Dados PostgreSQL**:

2. **Backend (.NET API)**:
   ```bash
   cd OPS.API
   dotnet restore
   dotnet run
   ```
   A API estará disponível em `http://localhost:5200`

3. **Frontend (React)**:
   ```bash
   cd OPS.Site
   npm install
   npm run dev
   ```
   O frontend estará disponível em `http://localhost:8080`

### Estrutura dos Dados

O sistema importa dados de:
- Câmara dos Deputados (CEAP)
- Senado Federal (CEAPS) 
- Assembleias Legislativas Estaduais
- Receita Federal (dados de fornecedores)

## Quem Somos?

A **Operação Política Supervisionada** conta com a ajuda de seus colaboradores voluntários, espalhados pelo Brasil, para a leitura e interpretação de dados dos portais públicos da transparência brasileira e exibição simplificada dessas informações.

Além disso, qualquer um pode ser um fiscal dos gastos públicos e este site oferece dados suficientes para isso.

Este projeto é político e social, mas não possui vínculo a partido político e não possui ligação privada ou fim lucrativo.

## Fale Conosco
* [Homepage](http://ops.net.br) - Homepage
* [Facebook](https://www.facebook.com/operacaopoliticasupervisionada) - Pagina no Facebook
* luciobig@ops.net.br - Contato principal
* suporte@ops.net.br - Contato técnico

## Fontes de Dados

### Dados Parlamentares
- **[CEAP - Câmara dos Deputados](http://www2.camara.leg.br/transparencia/cota-para-exercicio-da-atividade-parlamentar/dados-abertos-cota-parlamentar)** - Cota Para Exercício da Atividade Parlamentar - Deputados Federais
- **[CEAPS - Senado Federal](https://www12.senado.leg.br/transparencia/dados-abertos/dados-abertos-ceaps)** - Cota Para Exercício da Atividade Parlamentar - Senadores
- **Assembleias Legislativas Estaduais** - Dados de despesas dos deputados estaduais (disponibilizados através de portais de transparência estaduais)

### Dados de Fornecedores
- **[Receita Federal](http://www.receita.fazenda.gov.br/PessoaJuridica/CNPJ/cnpjreva/Cnpjreva_Solicitacao.asp)** - Informações cadastrais de fornecedores (Pessoa Jurídica)
- **[Minha Receita](https://minhareceita.org.br)** - API para consulta de dados de CNPJ

### Atualização dos Dados
Os dados são atualizados periodicamente através de processos automatizados de importação, garantindo que as informações estejam sempre atualizadas com os dados públicos mais recentes.


**O controle social é indispensável para combatermos a corrupção em nosso país.**
