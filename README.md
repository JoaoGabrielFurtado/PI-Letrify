# Projeto LETRIFY 📖

Rede social de livros que utiliza a [Open Library API](https://openlibrary.org/) para explorar, buscar e exibir informações detalhadas sobre livros. Os usuários podem criar uma conta, montar sua estante pessoal e organizar seus livros por status de leitura.

## 📁 Estrutura do Projeto
- **/backend**: API construída em .NET 9 (C#) com PostgreSQL (Supabase)
- **/frontend**: SPA (Single Page Application) em HTML, CSS e JavaScript puro

---

## 🚀 Como rodar o Backend
1. Abra a pasta `/backend` no Visual Studio 2022.
2. Restaure os pacotes NuGet.
3. Configure a connection string do PostgreSQL em `appsettings.json`.
4. Aperte `F5` ou execute `dotnet run` no terminal.
5. Faça o teste no Postman/Swagger/Navegador.

---

## 🚀 Como rodar o Frontend
1. Certifique-se de que o **backend está rodando** em `http://localhost:5265`.
2. Abra a pasta `/frontend/pi-projetolivros`.
3. Abra o arquivo `index.html` no navegador (ou use a extensão **Live Server** do VS Code).
4. O frontend se conectará automaticamente ao backend na porta 5265.

### ⚙️ Configuração
- A URL base do backend pode ser alterada no topo do arquivo `app.js`:
  ```javascript
  const API_BASE = 'http://localhost:5265';
  ```

### 🎨 Funcionalidades do Frontend
- **Cadastro e Login** com autenticação JWT
- **Perfil do Usuário** — editar nome, idade, cidade, bio e foto
- **Minha Estante** — organizar livros em: 📖 Lendo, ✅ Lido, 📋 Quero Ler
- **Busca de Livros** — por título, autor ou tema/gênero (com filtros de ordenação e quantidade)
- **Capas dos Livros** — carregadas automaticamente pela Open Library via ISBN
- **Dark/Light Mode** — tema salvo no navegador

---

## 🔌 Endpoints da API (Backend)

### 📚 Livros (`/api/livro`)

| Método | Rota | Descrição | Auth |
|--------|------|-----------|------|
| `GET` | `/api/livro/livroespecifico/{isbn}` | Retorna detalhes de um livro pelo ISBN | ❌ |
| `GET` | `/api/livro/livrostema` | Busca livros por tema/gênero | ❌ |
| `GET` | `/api/livro/livrostitulo` | Busca livros por título | ❌ |
| `GET` | `/api/livro/livrosautor` | Busca livros por nome do autor | ❌ |

**Detalhes:**

* **Buscar Livro Específico**
  * **Rota:** `GET /api/livro/livroespecifico/{isbn}`
  * **Descrição:** Retorna os detalhes completos de um livro (título, autor, editora, data e temas) através do seu código ISBN.
  * **Exemplo:** `/api/livro/livroespecifico/9788535902778`

* **Explorar por Tema (Gênero)**
  * **Rota:** `GET /api/livro/livrostema`
  * **Descrição:** Retorna uma lista de livros baseada em um tema/categoria. Suporta paginação e limite de retorno.
  * **Parâmetros Query:** `tema` (padrão: fiction), `quantidade` (padrão: 20), `pagina` (padrão: 1).
  * **Exemplo:** `/api/livro/livrostema?tema=fantasy&quantidade=10`

* **Pesquisar por Título**
  * **Rota:** `GET /api/livro/livrostitulo`
  * **Descrição:** Realiza uma busca retornando livros que contenham o termo pesquisado no título.
  * **Parâmetros Query:** `titulo` (obrigatório), `quantidade` (padrão: 20), `pagina` (padrão: 1).
  * **Exemplo:** `/api/livro/livrostitulo?titulo=principe&quantidade=20`

* **Pesquisar por Autor** *(novo)*
  * **Rota:** `GET /api/livro/livrosautor`
  * **Descrição:** Realiza uma busca retornando livros do autor pesquisado.
  * **Parâmetros Query:** `autor` (obrigatório), `quantidade` (padrão: 20), `pagina` (padrão: 1).
  * **Exemplo:** `/api/livro/livrosautor?autor=machado+de+assis&quantidade=10`

---

### 🔐 Autenticação (`/api/auth`)

| Método | Rota | Descrição | Auth |
|--------|------|-----------|------|
| `POST` | `/api/auth/register` | Cadastrar novo usuário | ❌ |
| `POST` | `/api/auth/login` | Login (retorna JWT token) | ❌ |

**Detalhes:**

* **Cadastro**
  * **Rota:** `POST /api/auth/register`
  * **Body (JSON):** `{ "nome": "...", "email": "...", "senha": "..." }`
  * **Retorno:** `{ "message": "Usuário criado com sucesso!" }`

* **Login**
  * **Rota:** `POST /api/auth/login`
  * **Body (JSON):** `{ "email": "...", "senha": "..." }`
  * **Retorno:** `{ "token": "eyJhbGciOi..." }` — Token JWT válido por 8 horas.

---

### 👤 Usuário (`/api/usuario`)

| Método | Rota | Descrição | Auth |
|--------|------|-----------|------|
| `GET` | `/api/usuario/{id}` | Ver perfil de um usuário | ❌ |
| `PUT` | `/api/usuario/editar` | Editar perfil próprio | ✅ |
| `POST` | `/api/usuario/meus-livros` | Adicionar livro à estante | ✅ |
| `GET` | `/api/usuario/{id}/livros` | Ver estante de um usuário | ❌ |
| `DELETE` | `/api/usuario/meus-livros/{livroId}` | Remover livro da estante | ✅ |

**Detalhes:**

* **Ver Perfil**
  * **Rota:** `GET /api/usuario/{id}`
  * **Retorno:** `{ id, nome, idade, cidade, descricao, fotoPerfil }`

* **Editar Perfil** 🔒
  * **Rota:** `PUT /api/usuario/editar`
  * **Body (FormData):** `idade`, `cidade`, `descricao`, `foto` (arquivo de imagem)
  * **Header:** `Authorization: Bearer {token}`

* **Adicionar Livro à Estante** 🔒
  * **Rota:** `POST /api/usuario/meus-livros`
  * **Body (JSON):** `{ "titulo": "...", "autor": "...", "isbn": "...", "status": "Lendo|Lido|Quero Ler" }`
  * **Header:** `Authorization: Bearer {token}`

* **Ver Estante do Usuário**
  * **Rota:** `GET /api/usuario/{id}/livros`
  * **Retorno:** `{ lendo: [...], lido: [...], queroLer: [...] }`

* **Remover Livro da Estante** 🔒
  * **Rota:** `DELETE /api/usuario/meus-livros/{livroId}`
  * **Header:** `Authorization: Bearer {token}`

---

## 🛠 Tecnologias Utilizadas

| Camada | Tecnologia |
|--------|-----------|
| Backend | .NET 9, C#, Entity Framework Core |
| Banco de Dados | PostgreSQL (Supabase) |
| Autenticação | JWT (JSON Web Token) + BCrypt |
| Frontend | HTML5, CSS3, JavaScript (Vanilla) |
| API Externa | Open Library API |
| Ícones | Bootstrap Icons |