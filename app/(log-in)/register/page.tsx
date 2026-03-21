"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { authService } from "@/app/lib/authService";

export default function CadastroPage() {
  const router = useRouter();
  
  const [nome, setNome] = useState("");
  const [email, setEmail] = useState("");
  const [senha, setSenha] = useState("");
  const [erro, setErro] = useState("");
  const [sucesso, setSucesso] = useState(false);
  const [carregando, setCarregando] = useState(false);

  const fazerCadastro = async (e: React.FormEvent) => {
    e.preventDefault();
    setErro("");
    setSucesso(false);

    if (!nome || !email || !senha) {
      setErro("Preencha todos os campos.");
      return;
    }
    
    // Pode colocar a mesma validação de e-mail e uma de senha forte aqui se quiser!

    setCarregando(true);
    try {
      await authService.cadastrar(nome, email, senha);
      setSucesso(true);
      
      // Espera 2 segundinhos para a pessoa ler a mensagem de sucesso e manda pro login
      setTimeout(() => {
        router.push("/login");
      }, 2000);
      
    } catch (err: any) {
      setErro(err.message);
    } finally {
      setCarregando(false);
    }
  };

  return (
    <div className="flex flex-col items-center justify-center min-h-screen p-4">
      <div className="w-full max-w-md p-8 rounded-2xl border" style={{ backgroundColor: 'var(--cor-fundo-card)', borderColor: 'var(--cor-fundo-sidebar)' }}>
        <h1 className="text-2xl font-black mb-2 text-center" style={{ color: 'var(--cor-texto-principal)' }}>Criar Conta</h1>
        <p className="text-center text-sm mb-6" style={{ color: 'var(--cor-texto-secundario)' }}>Junte-se à comunidade do Letrify</p>
        
        <form onSubmit={fazerCadastro} className="flex flex-col gap-4">
          
          {erro && <div className="p-3 rounded bg-red-500/10 border border-red-500 text-red-500 text-sm font-bold text-center">{erro}</div>}
          {sucesso && <div className="p-3 rounded bg-green-500/10 border border-green-500 text-green-500 text-sm font-bold text-center">Usuário criado com sucesso! Redirecionando...</div>}

          <input 
            type="text" 
            placeholder="Seu Nome (Como quer ser chamado)"
            value={nome}
            onChange={(e) => setNome(e.target.value)}
            className="p-3 rounded-lg border bg-transparent outline-none"
            style={{ borderColor: 'var(--cor-texto-secundario)', color: 'var(--cor-texto-principal)' }}
          />

          <input 
            type="email" 
            placeholder="Seu E-mail"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            className="p-3 rounded-lg border bg-transparent outline-none"
            style={{ borderColor: 'var(--cor-texto-secundario)', color: 'var(--cor-texto-principal)' }}
          />
          
          <input 
            type="password" 
            placeholder="Crie uma Senha"
            value={senha}
            onChange={(e) => setSenha(e.target.value)}
            className="p-3 rounded-lg border bg-transparent outline-none"
            style={{ borderColor: 'var(--cor-texto-secundario)', color: 'var(--cor-texto-principal)' }}
          />

          <button 
            type="submit" 
            disabled={carregando || sucesso}
            className="p-3 rounded-lg font-bold mt-2 disabled:opacity-50"
            style={{ backgroundColor: 'var(--cor-botao-primario)', color: 'var(--cor-botao-texto)' }}
          >
            {carregando ? "Criando..." : "Cadastrar"}
          </button>
        </form>

        <p className="mt-6 text-center text-sm" style={{ color: 'var(--cor-texto-secundario)' }}>
          Já tem uma conta? <Link href="/login" className="font-bold underline" style={{ color: 'var(--cor-primaria)' }}>Fazer Login</Link>
        </p>
      </div>
    </div>
  );
}