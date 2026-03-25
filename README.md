
# 💈 Sistema de Agendamento para Barbearia

Este é o repositório do Projeto de Final de Curso para o Bacharelado em Sistemas de Informação, que consiste em um sistema web completo para agendamento em uma barbearia.

## Objetivo do Projeto

Desenvolver um sistema web que permita a clientes realizar o cadastro e login, escolher serviços e barbeiros, definir data e horário, e confirmar agendamentos. O sistema também possui um painel para que barbeiros e administradores possam gerenciar os serviços, horários e a agenda de forma centralizada.

## 🚀 Tecnologias Utilizadas

O projeto é estruturado como um monorepo, contendo duas aplicações principais:

  * **Backend:**

      * **Framework:** .NET 9 (C\#) com ASP.NET Core Web API
      * **Banco de Dados:** PostgreSQL
      * **ORM:** Entity Framework Core (Code-First)
      * **Autenticação:** ASP.NET Core Identity com Tokens JWT
      * **Documentação da API:** Swashbuckle (Swagger UI)

  * **Frontend:**

      * **Framework:** React.js
      * **Roteamento:** React Router DOM
      * **Cliente HTTP:** Axios
      * **Estilização:** CSS Modules

## ⚙️ Pré-requisitos

Antes de começar, garanta que você tenha as seguintes ferramentas instaladas na sua máquina:

  * [.NET SDK 9.0](https://dotnet.microsoft.com/en-us/download/dotnet/9.0) ou superior.
  * [PostgreSQL](https://www.postgresql.org/download/) (um servidor de banco de dados local ou na nuvem).
  * [Node.js (versão LTS)](https://nodejs.org/) que inclui o `npm`.
  * Um cliente Git (como o [Git for Windows](https://git-scm.com/download/win)).

-----

## 🔧 Guia de Instalação e Execução

Siga os passos abaixo para clonar e rodar o projeto localmente.

### 1\. Clonar o Repositório

Abra um terminal e clone o projeto para sua máquina:

```bash
git clone https://github.com/JoaoPedroAES/Agendamento-Barbearia.git
cd Agendamento-Barbearia
```

### 2\. Configurando o Backend (.NET API)

1.  **Navegue até a pasta do backend:**
    ```bash
    cd backend
    ```
2.  **Configure a Conexão com o Banco de Dados:**
      * Abra o arquivo `barbearia.api/appsettings.Development.json`.
      * Localize a seção `ConnectionStrings` e altere os valores (`Host`, `Database`, `Username`, `Password`) para corresponder à configuração do seu servidor PostgreSQL.
3.  **Restaure os Pacotes e Aplique as Migrations:**
      * Ainda no terminal, dentro da pasta `backend/`, execute os comandos para criar o banco de dados e as tabelas:
    <!-- end list -->
    ```bash
    # Restaura os pacotes do .NET
    dotnet restore

    # Aplica as migrations para criar o schema do banco
    dotnet ef database update --project barbearia.api
    ```
4.  **Execute o Backend:**
      * Para iniciar o servidor da API, execute:
    <!-- end list -->
    ```bash
    dotnet run --project barbearia.api
    ```
      * O backend estará rodando (geralmente em `https://localhost:7275`). Você pode acessar a documentação do Swagger nesta URL para verificar se os endpoints estão ativos.

### 3\. Configurando o Frontend (React App)

1.  **Abra um NOVO terminal.** Não feche o terminal do backend.
2.  **Navegue até a pasta do frontend:**
    ```bash
    # A partir da raiz do projeto
    cd frontend
    ```
3.  **Instale as Dependências:**
      * Execute o `npm` para baixar todas as bibliotecas do React:
    <!-- end list -->
    ```bash
    npm install
    ```
4.  **Verifique a URL da API:**
      * Abra o arquivo `frontend/src/services/api.js`.
      * Confira se a `baseURL` corresponde ao endereço onde seu backend está rodando. O padrão é `https://localhost:7275`.
5.  **Execute o Frontend:**
      * Para iniciar a aplicação React, execute:
    <!-- end list -->
    ```bash
    npm start
    ```
      * Uma nova aba será aberta no seu navegador no endereço `http://localhost:3000`.

### ✅ Pronto\!

Com os dois servidores (backend e frontend) rodando, acesse **`http://localhost:3000`** no seu navegador para utilizar a aplicação.
