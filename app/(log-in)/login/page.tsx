"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { authService } from "@/app/lib/authService"; 

export default function LoginPage() {
  const router = useRouter();
  
  const [email, setEmail] = useState("");
  const [senha, setSenha] = useState("");
  const [erro, setErro] = useState("");
  const [carregando, setCarregando] = useState(false);

  const validarEmail = (emailTeste: string) => {
    // Regex simples para garantir que tem @ e um ponto
    const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return regex.test(emailTeste);
  };

  const fazerLogin = async (e: React.FormEvent) => {
    e.preventDefault(); // Impede a página de recarregar
    setErro("");

    // VALIDAÇÃO BÁSICA DO FRONT
    if (!email || !senha) {
      setErro("Por favor, preencha todos os campos.");
      return;
    }
    if (!validarEmail(email)) {
      setErro("Digite um formato de e-mail válido.");
      return;
    }

    // COMUNICAÇÃO COM A API
    setCarregando(true);
    try {
      await authService.login(email, senha);
      router.push("/perfil");
      
    } catch (err: any) {
      setErro(err.message);
    } finally {
      setCarregando(false);
    }
  };

  return (
    <div className="flex flex-col items-center justify-center min-h-screen p-4">
      <div className="w-full max-w-md p-8 rounded-2xl border" style={{ backgroundColor: 'var(--cor-fundo-card)', borderColor: 'var(--cor-fundo-sidebar)' }}>
        <h1 className="text-3xl font-black mb-6 text-center" style={{ color: 'var(--cor-primaria)' }}>Letrify</h1>
        
        <form onSubmit={fazerLogin} className="flex flex-col gap-4">
          
          {erro && (
            <div className="p-3 rounded bg-red-500/10 border border-red-500 text-red-500 text-sm font-bold text-center">
              {erro}
            </div>
          )}

          <input 
            type="email" 
            placeholder="Seu E-mail"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            className="p-3 rounded-lg border bg-transparent outline-none focus:ring-2"
            style={{ borderColor: 'var(--cor-texto-secundario)', color: 'var(--cor-texto-principal)' }}
          />
          
          <input 
            type="password" 
            placeholder="Sua Senha"
            value={senha}
            onChange={(e) => setSenha(e.target.value)}
            className="p-3 rounded-lg border bg-transparent outline-none focus:ring-2"
            style={{ borderColor: 'var(--cor-texto-secundario)', color: 'var(--cor-texto-principal)' }}
          />

          <button 
            type="submit" 
            disabled={carregando}
            className="p-3 rounded-lg font-bold transition-transform hover:scale-105 disabled:opacity-50 disabled:hover:scale-100"
            style={{ backgroundColor: 'var(--cor-botao-primario)', color: 'var(--cor-fundo-texto)' }}
          >
            {carregando ? "Entrando..." : "Entrar"}
          </button>
        </form>

        <p className="mt-6 text-center text-sm" style={{ color: 'var(--cor-texto-secundario)' }}>
          Ainda não tem conta? <Link href="/register" className="font-bold underline" style={{ color: 'var(--cor-primaria)' }}>Cadastre-se</Link>
        </p>
      </div>
    </div>
  );
}