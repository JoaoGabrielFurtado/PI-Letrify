"use client";

import { FormEvent, useEffect, useState } from "react";
import { useRouter } from "next/navigation";

// A chave continua a mesma para manter a compatibilidade com o layout do Feed
const AUTH_KEY = "letrify-auth";
// O Token da API vale 8h, mas vamos colocar 4h no front por segurança
const LOGIN_DURATION_MS = 1000 * 60 * 60 * 4; 

function getStoredAuth() {
  if (typeof window === "undefined") return null;
  const raw = localStorage.getItem(AUTH_KEY);
  if (!raw) return null;
  try {
    // Agora esperamos salvar o token da API, não só o nome do usuário
    const parsed = JSON.parse(raw) as { token: string; expiresAt: number };
    if (typeof parsed.expiresAt !== "number" || !parsed.token) return null;
    if (Date.now() > parsed.expiresAt) {
      localStorage.removeItem(AUTH_KEY);
      return null;
    }
    return parsed;
  } catch {
    localStorage.removeItem(AUTH_KEY);
    return null;
  }
}

// Atualizamos a função para salvar o token real recebido da API
function setStoredAuth(token: string) {
  localStorage.setItem(
    AUTH_KEY,
    JSON.stringify({ token, expiresAt: Date.now() + LOGIN_DURATION_MS })
  );
}

export default function LoginPage() {
  const router = useRouter();
  const [nome, setNome] = useState("");
  const [email, setEmail] = useState("");
  const [senha, setSenha] = useState("");
  const [erro, setErro] = useState("");
  const [carregando, setCarregando] = useState(false);

  useEffect(() => {
    const auth = getStoredAuth();
    if (auth) {
      router.replace("/");
    }
  }, [router]);

  // Transformamos a função em async para poder "esperar" a resposta da API
  const handleRegister = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setErro("");

    if (!nome.trim() || !email.trim() ||!senha.trim()) {
      setErro("Preencha todos os campos para continuar.");
      return;
    }

    setCarregando(true);

    try {
      // 1. Fazemos a chamada real para a API do seu colega
      const resposta = await fetch("https://letrify.fly.dev/api/auth/register", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ nome: nome.trim(), email: email.trim(), senha: senha.trim() }),
      });

        const dados = await resposta.json();

      if (resposta.ok) {
        // Redireciona para o login (você pode até usar um alert("Cadastrado com sucesso!") antes)
        router.push("/login"); 
      } else {
        setErro(dados.message || "Erro ao realizar o cadastro.");
      }
    } catch (err) {
      console.error("Erro ao conectar com a API:", err);
      setErro("Erro de conexão com o servidor. Tente novamente mais tarde.");
    } finally {
      setCarregando(false);
    }
  }


 return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-violet-900 via-purple-800 to-indigo-900 px-4 py-10">
      <div className="w-full max-w-md rounded-2xl bg-white/90 border border-white/40 p-8 shadow-2xl backdrop-blur">
        
        {/* Cabeçalho */}
        <div className="mb-6 text-center">
          <p className="text-xs uppercase tracking-[0.3em] text-violet-700 font-semibold">Letrify</p>
          <h1 className="mt-2 text-3xl font-bold text-slate-900">Criar Conta</h1>
          <p className="mt-2 text-sm text-slate-600">Junte-se à nossa comunidade literária.</p>
        </div>

        {/* Formulário */}
        <form onSubmit={handleRegister} className="space-y-4">
          
          <label className="block">
            <span className="text-sm text-slate-700">Nome</span>
            <input
              type="text"
              value={nome}
              onChange={(e) => setNome(e.target.value)}
              className="mt-1 block w-full rounded-md border border-slate-300 px-3 py-2 focus:border-violet-500 focus:outline-none focus:ring-2 focus:ring-violet-200"
              placeholder="Como quer ser chamado?"
            />
          </label>

          <label className="block">
            <span className="text-sm text-slate-700">E-mail</span>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              autoComplete="email"
              className="mt-1 block w-full rounded-md border border-slate-300 px-3 py-2 focus:border-violet-500 focus:outline-none focus:ring-2 focus:ring-violet-200"
              placeholder="seu@exemplo.com"
            />
          </label>

          <label className="block">
            <span className="text-sm text-slate-700">Senha</span>
            <input
              type="password"
              value={senha}
              onChange={(e) => setSenha(e.target.value)}
              autoComplete="new-password"
              className="mt-1 block w-full rounded-md border border-slate-300 px-3 py-2 focus:border-violet-500 focus:outline-none focus:ring-2 focus:ring-violet-200"
              placeholder="••••••••"
            />
          </label>

          {/* Mensagem de Erro */}
          {erro && <p className="text-xs text-red-600 bg-red-50 p-2 rounded border border-red-200">{erro}</p>}

          <button
            type="submit"
            disabled={carregando}
            className="w-full rounded-md bg-violet-600 px-4 py-2 font-semibold text-white transition hover:bg-violet-700 disabled:cursor-not-allowed disabled:bg-violet-400"
          >
            {carregando ? "Criando conta..." : "Cadastrar"}
          </button>
        </form>

        {/* Link para voltar para o Login */}
        <div className="mt-6 text-center text-sm text-slate-600">
          <p>Já tem uma conta? <br/>
            {/* Lembre-se de importar o Link do next/link lá no topo do arquivo! */}
            <a href="/login" className="text-violet-700 font-bold hover:underline">
              Faça login aqui
            </a>
          </p>
        </div>

      </div>
    </div>
  );
}
