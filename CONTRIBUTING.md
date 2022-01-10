Esta documentação ainda está sendo desenvolvida

Dados específicos serão disponibilizados em [Docs](https://github.com/ops-org/operacao-politica-supervisionada/tree/Documenta%C3%A7%C3%A3o/Docs)

- [Contribuindo com o Projeto](#contribuindo-com-o-projeto)
  - [Como começar?](#como-começar)
  - [Estrutura](#estrutura)
    - [Banco de dados](#banco-de-dados)
      - [Tabelas Específicas](#tabelas-específicas)
    - [O Backend](#o-backend)
    - [O FrontEnd](#o-frontend)
  - [Padrões e Estilos](#padrões-e-estilos)

# Contribuindo com o Projeto

Há várias formas de contribuir com o projeto, aqui será abordado os passos para programdores contribuirem com o site e as ferramentas

Grupo de discussão: [Robops no Telegram](https://t.me/joinchat/ByZCHlJ3VPEc8guzFbaybQ)

## Como começar?

1. Baixe (clone) o repositório
2. [BD] Subir um banco dedados MySql
3. [BD] Importar os arquivos de Schema
4. [Backend] Com o SDK do .Net6 instalado, abra a solução com o VisualStudio
5. [Backend] Definir o Projeto OPS.API como principal (`Set as Startup Project`)
6. [Backend]Iniciar o backend com F5
7. [FrontEnd] No VSCode, executar os comandos do NPM:
   1. `npm install`
   2. `npm run serve`

## Estrutura

### Banco de dados

O banco de dados do projeto é o MySql

Um novo banco deve ser criado usando os scripts de criação disponibilizados

As tabelas são separadas em duas categorias:
* Específicas
* Gerais

As específicas tem informações para órgãos e casas legislativas

As gerais dizem sobre informações sobre entidades, empresas e indivíduos

#### Tabelas Específicas

Hoje as tabelas espcíficas são denominadas com o seguinte prefixo:

* `cf_`: Câmara Federal
* `cl_`: Casas Estaduais
* `sf_`: Senado

### O Backend

O backend é escrito em C# e composto por:

* OPS.Core: Classes, models, DTOs e DAOs
* OPS.API: WebAPI para acesso aos dados
* OPS.Importador: Aplicação de importação de dados (executado manualmente)

### O FrontEnd

O FrontEnd é uma aplicação VUE, na solução, é o projeto `OPS.Site`

## Padrões e Estilos

Em progresso


