<div align="center">

# 📚 Letrify API

<br/>

![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)
![.NET](https://img.shields.io/badge/.NET_9-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white)
![Entity Framework](https://img.shields.io/badge/Entity_Framework_Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![SignalR](https://img.shields.io/badge/SignalR-WebSockets-0078D4?style=for-the-badge&logo=microsoft&logoColor=white)
![JWT](https://img.shields.io/badge/JWT-Auth-000000?style=for-the-badge&logo=jsonwebtokens&logoColor=white)
![Gemini](https://img.shields.io/badge/Google_Gemini-AI-4285F4?style=for-the-badge&logo=google&logoColor=white)

<br/>

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square)](https://opensource.org/licenses/MIT)
![Status](https://img.shields.io/badge/Status-Em%20Desenvolvimento-orange?style=flat-square)
![API](https://img.shields.io/badge/Tipo-REST%20API-blue?style=flat-square)

</div>

---

## 🏗️ Arquitetura e Tecnologias

### Stack Principal

| Camada | Tecnologia |
|---|---|
| **Linguagem** | C# 12 |
| **Framework** | ASP.NET Core (.NET 9) |
| **ORM** | Entity Framework Core |
| **Banco de Dados** | SQL Server (Azure) |
| **Autenticação** | JWT Bearer Tokens + BCrypt |
| **Tempo Real** | ASP.NET Core SignalR (WebSockets) |
| **IA / Embeddings** | Google Gemini API |
| **Busca Vetorial** | Qdrant (Vector Database) |
| **Catálogo de Livros** | Open Library API (externa) |

---

### ⚙️ Decisões Arquiteturais Avançadas

#### 🔴 Comunicação em Tempo Real com WebSockets (SignalR)
O chat global da plataforma é alimentado por um **Hub SignalR** (`/hubs/chat`). Quando um usuário envia ou deleta uma mensagem via API REST, o controller notifica proativamente **todos os clientes conectados** em tempo real através dos eventos `ReceberNovaMensagem` e `MensagemDeletada`. Isso elimina a necessidade de *polling* no front-end e garante uma experiência de chat instantânea e reativa.

#### 🛡️ Proteção Anti-Spam com Rate Limiting Nativo
A rota de envio de mensagens (`POST /api/chat/enviar`) está protegida pela política **`ChatAntiSpam`**, implementada com o Rate Limiter nativo do ASP.NET Core. A política usa uma janela fixa de **5 requisições por minuto**, particionada por `userId` do JWT ou por IP como fallback. Usuários que excedem o limite recebem um `HTTP 429 Too Many Requests` imediatamente, sem enfileiramento.

#### ⏱️ Processamento em Background (Worker Service)
O `LimpezaBancoChat` é um **`BackgroundService`** registrado como `HostedService` que roda de forma completamente assíncrona e isolada do ciclo de vida das requisições HTTP. Utilizando um `PeriodicTimer`, ele acorda a cada **24 horas** para varrer e remover mensagens de chat com mais de **5 dias**, mantendo o banco de dados saudável e performático. O acesso ao `DbContext` é feito de forma segura via `IServiceScopeFactory`, respeitando o ciclo de vida `Scoped` do EF Core.

#### 🌳 Estrutura de Dados em Árvore (Auto-Relacionamento)
O sistema de respostas do chat foi implementado com um **auto-relacionamento** na entidade `MensagemChat`. Cada mensagem possui um `MensagemPaiId` opcional que aponta para outra mensagem da mesma tabela. Isso cria uma estrutura de árvore (thread) no banco de dados. A **deleção em cascata** é tratada manualmente no código: ao deletar uma mensagem pai, o sistema primeiro remove todas as suas `Respostas` antes de remover a mensagem principal, garantindo a integridade referencial.

#### 📄 Paginação de Dados
O endpoint de listagem do chat (`GET /api/chat/listar`) suporta **paginação server-side** com os parâmetros `pagina` e `tamanhoPagina` (máximo de 100 itens). A query é otimizada com `.AsNoTracking()` para leituras somente de dados, `.Skip()` e `.Take()` para o controle eficiente do offset, e projeções via `.Select()` para evitar o carregamento desnecessário de colunas.

#### 🔐 Autenticação via Token JWT
Toda a segurança da API é baseada em **JSON Web Tokens (JWT)**. Ao fazer login, o usuário recebe um token com validade de 8 horas contendo seus claims (`Id`, `Email`, `Nome`). O token é validado em cada requisição protegida, verificando assinatura, issuer, audience e tempo de expiração com `ClockSkew = TimeSpan.Zero` para máxima precisão.

#### 🤖 Matchmaking com IA e Busca Vetorial
O `MatchController` orquestra um fluxo de IA sofisticado: ele constrói um **perfil semântico textual** do leitor (baseado em temas preferidos, autores favoritos e livros lidos), converte esse perfil em um **vetor de embeddings** via Google Gemini, persiste o vetor no **Qdrant** (banco de dados vetorial) e realiza uma **busca por similaridade cosseno** para encontrar os usuários com perfis literários mais parecidos.

---

## ✨ Funcionalidades

- **Autenticação Segura:** Registro com validação de e-mail e senha forte (regex), hash BCrypt e geração de JWT.
- **Gestão de Perfil:** Edição de dados pessoais (nome, cidade, descrição, foto de perfil via upload de arquivo).
- **Estante Virtual:** Adicionar livros com status `Lendo`, `Lido` ou `Quero Ler`, remover livros e visualizar a estante de qualquer usuário.
- **Livro Favorito:** Definir e atualizar um único livro favorito destacado no perfil.
- **Sistema de Seguidores:** Seguir e deixar de seguir outros leitores com um único endpoint (toggle), além de listar seguidores e seguidos.
- **Chat Global em Tempo Real:** Envio de mensagens e respostas (threads) com broadcast via SignalR, paginação e auto-limpeza automática de mensagens antigas.
- **Exploração de Livros:** Busca no catálogo da Open Library por tema, título ou autor com paginação.
- **Matchmaking com IA:** Descoberta de outros leitores com gostos compatíveis usando embeddings do Google Gemini e busca vetorial no Qdrant.
- **Estatísticas de Perfil:** Visualização de dados agregados: total de livros, situações, top temas e top autores preferidos.

---

## 🛣️ Endpoints Principais (Documentação da API)

### 🔐 Auth — `/api/auth`

| Método | Rota | 🔒 Token | Descrição |
|:---:|---|:---:|---|
| `POST` | `/api/auth/register` | ❌ | Registra um novo usuário com validação de e-mail e senha forte. |
| `POST` | `/api/auth/login` | ❌ | Autentica o usuário e retorna um token JWT (válido por 8h). |

---

### 👤 Usuário — `/api/usuario`

| Método | Rota | 🔒 Token | Descrição |
|:---:|---|:---:|---|
| `GET` | `/api/usuario/{id}` | ❌ | Retorna o perfil público de um usuário pelo ID. |
| `PUT` | `/api/usuario/editar` | ✅ | Edita o perfil do usuário logado (multipart/form-data, suporta upload de foto). |
| `GET` | `/api/usuario/informacoes/{id?}` | ✅ | Retorna estatísticas detalhadas de leitura (top temas, autores, contagens por status). |
| `DELETE` | `/api/usuario/deletar` | ✅ | Deleta a conta do usuário logado e remove a foto do servidor. |
| `POST` | `/api/usuario/meus-livros` | ✅ | Adiciona ou atualiza um livro na estante do usuário com um status. |
| `GET` | `/api/usuario/{id}/livros` | ❌ | Retorna a estante completa de um usuário (Lendo, Lido, Quero Ler). |
| `DELETE` | `/api/usuario/meus-livros/{livroId}` | ✅ | Remove um livro da estante do usuário logado. |

---

### 💬 Chat — `/api/chat`

| Método | Rota | 🔒 Token | Descrição |
|:---:|---|:---:|---|
| `POST` | `/api/chat/enviar` | ✅ | Envia uma mensagem (ou resposta) no chat global. **Rate limited:** 5 req/min. Dispara evento SignalR. |
| `GET` | `/api/chat/listar` | ❌ | Lista as mensagens do chat com paginação (`?pagina=1&tamanhoPagina=50`). Inclui threads de respostas. |
| `DELETE` | `/api/chat/deletar/{id}` | ✅ | Deleta uma mensagem (apenas o autor pode). Apaga em cascata e dispara evento SignalR. |

---

### ❤️ Favoritos — `/api/favoritos`

| Método | Rota | 🔒 Token | Descrição |
|:---:|---|:---:|---|
| `POST` | `/api/favoritos/add` | ✅ | Define ou atualiza o livro favorito do usuário. Cria o livro localmente se não existir. |
| `DELETE` | `/api/favoritos/excluir` | ✅ | Remove o livro favorito do usuário logado. |

---

### 👥 Seguidores — `/api/seguidores`

| Método | Rota | 🔒 Token | Descrição |
|:---:|---|:---:|---|
| `POST` | `/api/seguidores/seguir/{idSeguido}` | ✅ | Toggle: segue ou deixa de seguir um usuário com uma única chamada. |
| `GET` | `/api/seguidores/seguidores/{id?}` | ✅ | Lista os seguidores de um usuário (ou do próprio usuário logado se `id` omitido). |
| `GET` | `/api/seguidores/seguindo/{id?}` | ✅ | Lista os usuários que alguém está seguindo. |

---

### 🤖 Match (IA) — `/api/match`

| Método | Rota | 🔒 Token | Descrição |
|:---:|---|:---:|---|
| `POST` | `/api/match` | ✅ | Gera/atualiza o vetor do usuário no Qdrant e retorna os leitores mais compatíveis por similaridade. |
| `POST` | `/api/match/migrar-todos-vetores` | ❌ | Endpoint utilitário para reprocessar os vetores de todos os usuários no Qdrant. |

---

### 📖 Livros (Open Library) — `/api/livro`

| Método | Rota | 🔒 Token | Descrição |
|:---:|---|:---:|---|
| `GET` | `/api/livro/livrostema` | ❌ | Busca livros por tema na Open Library. Params: `tema`, `pagina`, `quantidade`. |
| `GET` | `/api/livro/livrostitulo` | ❌ | Busca livros por título na Open Library. Params: `titulo`, `pagina`, `quantidade`. |
| `GET` | `/api/livro/livrosautor` | ❌ | Busca livros por autor na Open Library. Params: `autor`, `pagina`, `quantidade`. |

---

### 📡 SignalR Hub

| Protocolo | Endereço | Evento (Server → Client) | Descrição |
|:---:|---|---|---|
| `WebSocket` | `/hubs/chat` | `ReceberNovaMensagem` | Disparado ao enviar uma mensagem. Transmite os dados completos da mensagem para todos os clientes. |
| `WebSocket` | `/hubs/chat` | `MensagemDeletada` | Disparado ao deletar uma mensagem. Transmite o `id` da mensagem removida. |

---

## 🚀 Como Rodar o Projeto

### Pré-requisitos

Antes de começar, certifique-se de ter instalado:

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server](https://www.microsoft.com/pt-br/sql-server/sql-server-downloads) (ou SQL Server Express)
- [Git](https://git-scm.com/)

---

### Passo a Passo

**1. Clone o repositório**
```bash
git clone https://github.com/seu-usuario/letrify-api.git
cd letrify-api
```

**2. Configure o `appsettings.json`**

Abra o arquivo `appsettings.json` e preencha as seguintes seções com suas credenciais:

```json
{
  "ConnectionStrings": {
    "Azure": "Server=SEU_SERVIDOR;Database=LetrifyDb;User Id=SEU_USUARIO;Password=SUA_SENHA;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "SUA_CHAVE_SECRETA_MUITO_LONGA_E_SEGURA_AQUI",
    "Issuer": "LetrifyAPI",
    "Audience": "LetrifyClientes"
  }
}
```

> ⚠️ **Segurança:** Nunca comite o `appsettings.json` com credenciais reais. Utilize `User Secrets` ou variáveis de ambiente em produção.

**3. Aplique as Migrations do banco de dados**

Com o Package Manager Console (Visual Studio):
```powershell
Update-Database
```

Ou via CLI do .NET:
```bash
dotnet ef database update
```

Isso irá criar todas as tabelas necessárias no SQL Server automaticamente.

**4. Execute a API**

```bash
dotnet run
```

A API estará disponível em `https://localhost:7XXX` (a porta exata será exibida no terminal).

**5. Acesse a documentação OpenAPI (Swagger)**

Em ambiente de desenvolvimento, navegue até:

```
https://localhost:7XXX/openapi
```

---

### Variáveis de Ambiente Opcionais

Para integração com IA e busca vetorial, configure também:

```json
{
  "Gemini": {
    "ApiKey": "SUA_CHAVE_GEMINI"
  },
  "Qdrant": {
    "Host": "localhost",
    "Port": 6333
  }
}
```

> 💡 O sistema funciona normalmente sem essas configurações, apenas o endpoint `/api/match` ficará indisponível.

---
