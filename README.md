# Projeto LETRIFY 📖

API Consumida -> [Open Library API](https://openlibrary.org/) para explorar, buscar e exibir informações detalhadas sobre livros.

## 📁 Estrutura do Projeto
- **/backend**: API construída em .NET 9 (C#) (em desenvolvimento)
- **/frontend**: Ainda não desenvolvida

## 🚀 Como rodar o Backend
1. Abra a pasta `/backend` no Visual Studio 2022.
2. Restaure os pacotes NuGet.
3. Aperte `F5` ou execute `dotnet run` no terminal.
4. Faça o teste no Postman/Swagger/Navegador.

### 🔌 Endpoints da API (Backend)

Abaixo estão as rotas disponíveis para consumo. 

* **Buscar Livro Específico**
  * **Rota:** `GET /api/livro/livroespecifico/{isbn}`
  * **Descrição:** Retorna os detalhes completos de um livro (título, autor, editora, data e temas) através do seu código ISBN (ID da API).
  * **Exemplo:** `/api/livro/livroespecifico/9788535902778`

* **Explorar por Tema (Gênero)**
  * **Rota:** `GET /api/livro/livrostema`
  * **Descrição:** Retorna uma lista de livros baseada em um tema/categoria. Suporta paginação e limite de retorno.
  * **Parâmetros Query:** `tema` (padrão: fiction), `quantidade` (padrão: 20), `pagina` (padrão: 1).
  * **Exemplo:** `/api/livro/livrostema?tema=fantasy&quantidade=10`

* **Pesquisar por Título**
  * **Rota:** `GET /api/livro/livrostitulo`
  * **Descrição:** Realiza uma busca livre retornando os livros que contenham o termo pesquisado no título.
  * **Parâmetros Query:** `titulo` (obrigatório), `quantidade` (padrão: 20), `pagina` (padrão: 1).
  * **Exemplo:** `/api/livro/livrostitulo?titulo=principe&quantidade=20`

## 🚀 Como rodar o Frontend
Em desenvolvimento