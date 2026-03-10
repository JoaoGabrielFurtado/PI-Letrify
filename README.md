# 📖 Projeto LETRIFY

O **Letrify** é uma plataforma completa para amantes da leitura. Com ele, os usuários podem explorar o vasto catálogo da Open Library, gerenciar suas próprias estantes e personalizar seus perfis de leitores.

---

## 📁 Estrutura do Projeto
- **/backend**: Web API construída em **.NET 9 (C#)** com Entity Framework Core.
- **/frontend**: Em desenvolvimento.

---

## 🚀 Como rodar o Backend
1. Abra a pasta `/backend` no **Visual Studio 2022**.
2. No arquivo `appsettings.json`, insira a Connection String do **Supabase** (utilizando a porta **6543** para IPv4).
3. Certifique-se de que a pasta `wwwroot/fotos` existe para o armazenamento de imagens.
4. Execute o projeto (`F5` ou `dotnet run`).

---

## 🛠️ Tecnologias e Arquitetura
Este projeto segue padrões de mercado para garantir segurança e escalabilidade:

* **Autenticação:** JWT (JSON Web Tokens).
* **Criptografia:** BCrypt.Net para proteção de senhas.
* **Banco de Dados:** PostgreSQL (hospedado no Supabase).
* **Upload de Arquivos:** Processamento de `multipart/form-data` para fotos de perfil.
* **CORS:** Configurado para permitir integração total com o frontend local.
* **Serialização:** Configurada para ignorar ciclos de referência (Object Cycles).



---

## 🔌 Documentação dos Endpoints

### 1. Autenticação (`/api/auth`)
| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `POST` | `/register` | Cria uma nova conta. Enviar: `nome`, `email`, `senha`. |
| `POST` | `/login` | Valida credenciais e retorna o **Bearer Token**. |

### 2. Perfil do Usuário (`/api/usuario`)
| Método | Rota | Autenticação | Descrição |
| :--- | :--- | :--- | :--- |
| `GET` | `/{id}` | Nenhuma | Retorna Nome, Idade, Cidade, Bio e Foto. |
| `PUT` | `/editar` | **JWT** | Atualiza perfil e faz upload de foto (usar `form-data`). |

### 3. Gerenciamento da Estante (`/api/usuario`)
| Método | Rota | Autenticação | Descrição |
| :--- | :--- | :--- | :--- |
| `POST` | `/meus-livros` | **JWT** | Adiciona/Atualiza livro (Status: `Lendo`, `Lido`, `Quero Ler`). |
| `GET` | `/{id}/livros` | Nenhuma | Retorna a estante do usuário dividida em categorias. |
| `DELETE` | `/meus-livros/{id}` | **JWT** | Remove o vínculo de um livro com a estante do usuário. |



### 4. Busca Externa (Open Library)
| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `GET` | `/api/livro/livroespecifico/{isbn}` | Busca detalhes técnicos por ISBN. |
| `GET` | `/api/livro/livrostema` | Explora livros por gênero/categoria. |
| `GET` | `/api/livro/livrostitulo` | Busca livre por termos no título. |

---

## 📝 Notas para o Time de Frontend
1.  **Token JWT:** Deve ser enviado no header de todas as rotas protegidas:  
    `Authorization: Bearer <seu_token>`
2.  **Upload de Foto:** Para o endpoint `/api/usuario/editar`, utilize o objeto `FormData()` do JavaScript para enviar o arquivo e os textos simultaneamente.
3.  **URLs de Imagem:** A API retorna o caminho relativo (ex: `/fotos/imagem.jpg`). O frontend deve concatenar com a `BaseURL` da API para exibir.
4.  **Status de Livros:** Respeitar o Case-Sensitive exigido pelo banco: `Lendo`, `Lido` ou `Quero Ler`.

---

## 🚀 Como rodar o Frontend
*Em desenvolvimento.*