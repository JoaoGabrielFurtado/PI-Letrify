// app/lib/authService.ts
import { extrairIdDoToken } from "./jwt";

const API_BASE_URL = "https://letrify.fly.dev/api/auth";

export const authService = {
  //  FAZER LOGIN REAL
  async login(email: string, senha: string) {
    const resposta = await fetch(`${API_BASE_URL}/login`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ email, senha }),
    });

    if (!resposta.ok) throw new Error("E-mail ou senha incorretos.");

    const dados = await resposta.json();
    
    if (dados.token) {
      // Guarda o token
      localStorage.setItem("letrify_token", dados.token);
      
      // Descriptografa o token, rouba o ID e guarda o ID separado
      const userId = extrairIdDoToken(dados.token);
      if (userId) {
        localStorage.setItem("letrify_user_id", userId);
      }
    }
    
    return dados;
  },

  // 2. CADASTRAR USUÁRIO
  async cadastrar(nome: string, email: string, senha: string) {
    const resposta = await fetch(`${API_BASE_URL}/register`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ nome, email, senha }),
    });

    if (!resposta.ok) {
      // Se o back-end mandar mensagem de erro (ex: e-mail já existe), tenta ler essa mensagem e mostrar para a pessoa. Se não conseguir ler, mostra uma mensagem genérica.
      const erroDados = await resposta.json().catch(() => null);
      throw new Error(erroDados?.message || "Erro ao criar conta. Tente novamente.");
    }

    return await resposta.json();
  },

  // PEGA O TOKEN 
  getToken() {
    if (typeof window !== "undefined") return localStorage.getItem("letrify_token");
    return null;
  },

  // PEGAR O ID ROUBADO
  getUserId() {
    if (typeof window !== "undefined") return localStorage.getItem("letrify_user_id");
    return null;
  },

  logout() {
    if (typeof window !== "undefined") {
      localStorage.removeItem("letrify_token");
      localStorage.removeItem("letrify_user_id");
    }
  }
};