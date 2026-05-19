<div align="center">

# 📚 Letrify API

### *A rede social para quem vive entre páginas*

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
A plataforma possui três Hubs SignalR independentes: `ChatHub` para broadcast do chat global, `NotificacaoHub` para notificações individuais (cada usuário entra no próprio grupo `usuario-{id}`) e `GrupoHub` para chat isolado por sala (`grupo-{id}`). Isso garante que mensagens cheguem apenas aos destinatários corretos sem vazamento entre contextos.

#### 🛡️ Proteção Anti-Spam com Rate Limiting Nativo
A rota de envio de mensagens (`POST /api/chat/enviar`) está protegida pela política **`ChatAntiSpam`**, implementada com o Rate Limiter nativo do ASP.NET Core. A política usa uma janela fixa de **5 requisições por minuto**, particionada por `userId` do JWT ou por IP como fallback. Usuários que excedem o limite recebem `HTTP 429 Too Many Requests` imediatamente, sem enfileiramento.

#### ⏱️ Processamento em Background (Worker Service)
O `LimpezaBancoChat` é um **`BackgroundService`** que roda de forma completamente assíncrona e isolada do ciclo de vida das requisições HTTP. Utilizando um `PeriodicTimer`, ele acorda a cada **24 horas** para remover mensagens de chat com mais de **5 dias**. O acesso ao `DbContext` é feito via `IServiceScopeFactory`, respeitando o ciclo de vida `Scoped` do EF Core.

#### 🌳 Estrutura de Dados em Árvore (Auto-Relacionamento)
O sistema de respostas do chat e dos posts de grupo foi implementado com **auto-relacionamento** nas entidades `MensagemChat` e `PostGrupo`. Cada registro possui um `PaiId` opcional que aponta para outro registro da mesma tabela, criando uma estrutura de thread. A deleção em cascata é tratada manualmente no código para garantir integridade referencial.

#### 📄 Paginação de Dados
Todos os endpoints de listagem suportam **paginação server-side** com os parâmetros `pagina` e `tamanhoPagina`. As queries são otimizadas com `.AsNoTracking()`, `.Skip()`, `.Take()` e projeções via `.Select()` para evitar o carregamento desnecessário de dados.

#### 🔐 Autenticação via Token JWT
Toda a segurança da API é baseada em **JSON Web Tokens (JWT)**. Ao fazer login, o usuário recebe um token com validade de 8 horas contendo seus claims (`Id`, `Email`, `Nome`). O token também é aceito via query string (`?access_token=`) para autenticar conexões WebSocket do SignalR.

#### 🔔 Sistema de Notificações em Tempo Real
O `NotificacaoService` centraliza a criação e o disparo de notificações: persiste no banco e entrega via SignalR ao grupo do usuário destino simultaneamente. É disparado automaticamente por `SeguidoresController` (novo seguidor), `MatchController` (match com score ≥ 80%), `ChatController` (resposta a comentário) e `GruposController` (solicitação e resposta de acesso a grupo).

#### 🤖 Matchmaking com IA e Busca Vetorial
O `MatchController` constrói um **perfil semântico textual** do leitor, converte em **vetor de embeddings** via Google Gemini, persiste no **Qdrant** e realiza **busca por similaridade cosseno**. O resultado inclui perfil literário completo de cada match (livros lidos, autores e temas preferidos) obtido em uma única query batch, evitando o problema N+1.

---

## ✨ Funcionalidades

- **Autenticação Segura:** Registro com validação de e-mail e senha forte, hash BCrypt e JWT.
- **Gestão de Perfil:** Edição de dados pessoais com upload de foto, estatísticas de leitura e busca de usuários por nome com paginação.
- **Estante Virtual:** Gerenciamento de livros com status `Lendo`, `Lido` ou `Quero Ler`.
- **Livro Favorito:** Definir e atualizar um livro favorito destacado no perfil.
- **Sistema de Seguidores:** Toggle de seguir/deixar de seguir com notificação automática ao seguido.
- **Chat Global em Tempo Real:** Mensagens, respostas em thread, curtidas ❤️ (toggle) e posts de recrutamento para grupos.
- **Notificações Push:** Entrega em tempo real via SignalR para eventos de seguidor, match e menção/resposta.
- **Grupos de Leitura:** CRUD completo, sistema de roles (Líder/Admin/Membro), solicitações de acesso para grupos fechados, posts internos com threads e chat em tempo real isolado por sala.
- **Exploração de Livros:** Busca na Open Library por tema, título ou autor com paginação.
- **Matchmaking com IA:** Descoberta de leitores compatíveis com perfil literário detalhado e score de similaridade percentual.

---

## 🛣️ Endpoints (Documentação da API)

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
| `GET` | `/api/usuario/usuariosPorNome` | ❌ | Busca usuários pelo nome com paginação. Params: `nome`, `pagina`, `tamanhoPagina`. Retorna `temMais` para controle do botão "Carregar mais". |
| `PUT` | `/api/usuario/editar` | ✅ | Edita o perfil do usuário logado (multipart/form-data, suporta upload de foto). |
| `GET` | `/api/usuario/informacoes/{id?}` | ✅ | Retorna estatísticas detalhadas: total de livros, top temas, top autores e livro favorito. |
| `DELETE` | `/api/usuario/deletar` | ✅ | Deleta a conta do usuário logado e remove a foto do servidor. |
| `POST` | `/api/usuario/meus-livros` | ✅ | Adiciona ou atualiza um livro na estante com um status. |
| `GET` | `/api/usuario/{id}/livros` | ❌ | Retorna a estante completa de um usuário (Lendo, Lido, Quero Ler). |
| `DELETE` | `/api/usuario/meus-livros/{livroId}` | ✅ | Remove um livro da estante do usuário logado. |

---

### 💬 Chat — `/api/chat`

| Método | Rota | 🔒 Token | Descrição |
|:---:|---|:---:|---|
| `POST` | `/api/chat/enviar` | ✅ | Envia mensagem ou resposta no chat global. Campo `GrupoId` opcional transforma o post em convite de recrutamento. **Rate limited:** 5 req/min. Dispara evento SignalR e notificação de menção ao autor do comentário respondido. |
| `GET` | `/api/chat/listar` | ❌ | Lista mensagens com paginação. Retorna `TotalCurtidas`, `EuCurti` e objeto `Grupo` (id, nome, fotoCapa, status) para posts de recrutamento. |
| `DELETE` | `/api/chat/deletar/{id}` | ✅ | Deleta mensagem e respostas em cascata. Dispara evento SignalR `MensagemDeletada`. |
| `POST` | `/api/chat/curtir/{mensagemId}` | ✅ | Toggle ❤️: curte ou descurte uma mensagem. Dispara evento SignalR `AtualizarCurtidas` com total atualizado e estado `curtiu`. |

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
| `POST` | `/api/seguidores/seguir/{idSeguido}` | ✅ | Toggle: segue ou deixa de seguir. Envia notificação `Seguidor` ao usuário seguido. |
| `GET` | `/api/seguidores/seguidores/{id?}` | ✅ | Lista os seguidores de um usuário (ou do logado se `id` omitido). |
| `GET` | `/api/seguidores/seguindo/{id?}` | ✅ | Lista os usuários que alguém está seguindo. |

---

### 🔔 Notificações — `/api/notificacoes`

| Método | Rota | 🔒 Token | Descrição |
|:---:|---|:---:|---|
| `GET` | `/api/notificacoes` | ✅ | Lista notificações do usuário logado ordenadas por não lidas primeiro. Param opcional: `apenasNaoLidas=true`. Retorna `totalNaoLidas`. |
| `PUT` | `/api/notificacoes/{id}/lida` | ✅ | Marca uma notificação específica como lida. |
| `PUT` | `/api/notificacoes/marcar-todas-lidas` | ✅ | Marca todas as notificações do usuário como lidas via `ExecuteUpdateAsync`. |
| `DELETE` | `/api/notificacoes/{id}` | ✅ | Remove uma notificação específica. |

---

### 🏛️ Grupos — `/api/grupos`

| Método | Rota | 🔒 Token | Quem pode | Descrição |
|:---:|---|:---:|:---:|---|
| `GET` | `/api/grupos` | ❌ | Todos | Lista todos os grupos com total de membros. |
| `GET` | `/api/grupos/{id}` | ❌ | Todos | Detalhe do grupo com lista completa de membros e roles. |
| `POST` | `/api/grupos` | ✅ | Autenticado | Cria um grupo. O criador vira Líder automaticamente. Suporta upload de foto capa. |
| `PUT` | `/api/grupos/{id}` | ✅ | Líder | Edita nome, descrição, status e foto do grupo. |
| `DELETE` | `/api/grupos/{id}` | ✅ | Líder | Deleta o grupo e todos os dados em cascata. |
| `POST` | `/api/grupos/{id}/entrar` | ✅ | Autenticado | Entra direto se grupo Aberto. Cria solicitação e notifica o Líder se grupo Fechado. |
| `POST` | `/api/grupos/{id}/sair` | ✅ | Membro | Sai do grupo. Líder não pode sair — deve deletar o grupo. |
| `GET` | `/api/grupos/{id}/solicitacoes` | ✅ | Líder / Admin | Lista solicitações de entrada pendentes. |
| `PUT` | `/api/grupos/{id}/solicitacoes/{sid}` | ✅ | Líder / Admin | Aceita ou recusa uma solicitação. Notifica o solicitante com o resultado. |
| `PUT` | `/api/grupos/{id}/membros/{mid}/role` | ✅ | Líder | Promove membro a Admin ou rebaixa Admin a Membro. |
| `DELETE` | `/api/grupos/{id}/membros/{mid}` | ✅ | Líder / Admin | Remove um membro do grupo. |
| `GET` | `/api/grupos/{id}/posts` | ✅ | Membro | Lista posts internos com threads de respostas e paginação. |
| `POST` | `/api/grupos/{id}/posts` | ✅ | Membro | Cria post interno. Suporta `PostPaiId` para respostas em thread. |
| `DELETE` | `/api/grupos/{id}/posts/{pid}` | ✅ | Dono / Admin / Líder | Deleta post e respostas em cascata. |
| `POST` | `/api/grupos/{id}/chat` | ✅ | Membro | Envia mensagem no chat do grupo. Dispara `ReceberMensagemGrupo` apenas para membros da sala. |
| `GET` | `/api/grupos/{id}/chat` | ✅ | Membro | Histórico paginado do chat do grupo. |

---

### 🤖 Match (IA) — `/api/match`

| Método | Rota | 🔒 Token | Descrição |
|:---:|---|:---:|---|
| `POST` | `/api/match` | ✅ | Gera embedding do perfil, salva no Qdrant e retorna leitores compatíveis com perfil literário completo (livros, autores, temas) e score percentual. Notifica automaticamente matches com score ≥ 80%. |

---

### 📖 Livros (Open Library) — `/api/livro`

| Método | Rota | 🔒 Token | Descrição |
|:---:|---|:---:|---|
| `GET` | `/api/livro/livrostema` | ❌ | Busca livros por tema. Params: `tema`, `pagina`, `quantidade`. |
| `GET` | `/api/livro/livrostitulo` | ❌ | Busca livros por título. Params: `titulo`, `pagina`, `quantidade`. |
| `GET` | `/api/livro/livrosautor` | ❌ | Busca livros por autor. Params: `autor`, `pagina`, `quantidade`. |

---

### 📡 SignalR Hubs

| Hub | Endereço | Evento | Direção | Descrição |
|---|---|---|:---:|---|
| `ChatHub` | `/hubs/chat` | `ReceberNovaMensagem` | Server → Todos | Nova mensagem ou resposta no chat global com dados completos. |
| `ChatHub` | `/hubs/chat` | `MensagemDeletada` | Server → Todos | ID da mensagem removida para atualização do front. |
| `ChatHub` | `/hubs/chat` | `AtualizarCurtidas` | Server → Todos | Total de curtidas e estado `curtiu` após cada toggle. |
| `NotificacaoHub` | `/hubs/notificacoes` | `ReceberNotificacao` | Server → Usuário | Notificação entregue apenas ao grupo `usuario-{id}` do destinatário. |
| `GrupoHub` | `/hubs/grupo` | `ReceberMensagemGrupo` | Server → Sala | Mensagem entregue apenas aos membros conectados na sala `grupo-{id}`. |
| `GrupoHub` | `/hubs/grupo` | `EntrarNoGrupo(grupoId)` | Client → Server | Cliente entra na sala do grupo para começar a receber mensagens. |
| `GrupoHub` | `/hubs/grupo` | `SairDoGrupo(grupoId)` | Client → Server | Cliente sai da sala do grupo. |

---

## 🚀 Como Rodar o Projeto

### Pré-requisitos

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
```json
{
  "ConnectionStrings": {
    "Azure": "Server=SEU_SERVIDOR;Database=LetrifyDb;User Id=SEU_USUARIO;Password=SUA_SENHA;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "SUA_CHAVE_SECRETA_MUITO_LONGA_E_SEGURA_AQUI",
    "Issuer": "LetrifyAPI",
    "Audience": "LetrifyClientes"
  },
  "Gemini": {
    "ApiKey": "SUA_CHAVE_GEMINI"
  },
  "Qdrant": {
    "Host": "localhost",
    "Port": 6333
  }
}
```

> ⚠️ Nunca comite credenciais reais. Use `User Secrets` ou variáveis de ambiente em produção.

**3. Execute os scripts SQL no banco**

Como o projeto não utiliza Migrations, rode os scripts na seguinte ordem no SQL Server Management Studio:

```
1. Tabelas base: usuarios, livros, situacao_livros, favoritos, avaliacoes, Seguidores
2. MensagensChat
3. Notificacoes
4. Grupos → UsuarioGrupo → SolicitacaoGrupo → PostGrupo → MensagemGrupo
5. CurtidaChat
6. ALTER TABLE MensagensChat ADD GrupoId (script de recrutamento)
```

**4. Crie as pastas de upload**
```
wwwroot/
  fotos/     ← fotos de perfil dos usuários
  grupos/    ← fotos de capa dos grupos
```

**5. Execute a API**
```bash
dotnet run
```

**6. Acesse a documentação OpenAPI**
```
https://localhost:7XXX/openapi
```

> 💡 Os endpoints de `/api/match` ficam indisponíveis sem as chaves do Gemini e do Qdrant configuradas. Todos os outros funcionam normalmente.

---

<div align="center">

Feito com ❤️ e muitas páginas viradas.

</div>
