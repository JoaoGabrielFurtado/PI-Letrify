<div align="center">

# 📚 Letrify API


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





#### 🌳 Estrutura de Dados em Árvore (Auto-Relacionamento)
O sistema de respostas do chat foi implementado com um **auto-relacionamento** na entidade `MensagemChat`. Cada mensagem possui um `MensagemPaiId` opcional que aponta para outra mensagem da mesma tabela. Isso cria uma estrutura de árvore (thread) no banco de dados. A **deleção em cascata** é tratada manualmente no código: ao deletar uma mensagem pai, o sistema primeiro remove todas as suas `Respostas` antes de remover a mensagem principal, garantindo a integridade referencial.

#### 📄 Paginação de Dados
O endpoint de listagem do chat (`GET /api/chat/listar`) suporta **paginação server-side** com os parâmetros `pagina` e `tamanhoPagina` (máximo de 100 itens). A query é otimizada com `.AsNoTracking()` para leituras somente de dados, `.Skip()` e `.Take()` para o controle eficiente do offset, e projeções via `.Select()` para evitar o carregamento desnecessário de colunas.

#### 🔐 Autenticação via Token JWT
Toda a segurança da API é baseada em **JSON Web Tokens (JWT)**. Ao fazer login, o usuário recebe um token com validade de 8 horas contendo seus claims (`Id`, `Email`, `Nome`). O token é validado em cada requisição protegida, verificando assinatura, issuer, audience e tempo de expiração com `ClockSkew = TimeSpan.Zero` para máxima precisão.

#### 🤖 Matchmaking com IA e Busca Vetorial
O `MatchController` orquestra um fluxo de IA sofisticado: ele constrói um **perfil semântico textual** do leitor (baseado em temas preferidos, autores favoritos e livros lidos), converte esse perfil em um **vetor de embeddings** via Google Gemini, persiste o vetor no **Qdrant** (banco de dados vetorial) e realiza uma **busca por similaridade cosseno** para encontrar os usuários com perfis literários mais parecidos.

---









---






---



---




---










---



---

## ⚙️ Configurações de Ambiente
O projeto separa chaves sensíveis do código-fonte através do `.gitignore`. Para rodar o ambiente completo, configure o `appsettings.json` com:
- `Jwt:Key`: Chave para assinatura dos tokens.
- `Gemini:ApiKey`: Chave de acesso ao Google AI Studio.
- `Qdrant:Url` / `ApiKey`: Endereço e chave do cluster vetorial.
- `SegurancaDaApi:ChaveMestraSeed`: Senha de proteção para endpoints de infraestrutura.
