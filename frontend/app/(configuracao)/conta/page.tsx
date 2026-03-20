"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { dadosFalsos } from "@/app/lib/mockDados"; // Puxando do nosso arquivo limpo!

// Função auxiliar para ler o Token (reaproveitada do Perfil)
function extrairDadosDoToken(token: string) {
  try {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(window.atob(base64).split('').map(function(c) {
        return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
    }).join(''));
    return JSON.parse(jsonPayload);
  } catch (e) {
    return null;
  }
}

export default function ContaPage() {
  const router = useRouter();
  
  // Estados do Usuário Real
  const [nomeReal, setNomeReal] = useState("Carregando...");
  const [emailReal, setEmailReal] = useState(dadosFalsos.emailSecundario);
  const [tokenSessao, setTokenSessao] = useState("");

  // Estados da Senha
  const [senhaEsquecida, setSenhaEsquecida] = useState(false);

  // Estados da Zona de Perigo
  const [mostrarConfirmacao, setMostrarConfirmacao] = useState(false);
  const [textoDigitado, setTextoDigitado] = useState("");
  const [carregando, setCarregando] = useState(false);
  const [erro, setErro] = useState("");

  useEffect(() => {
    const rawAuth = localStorage.getItem("letrify-auth");
    if (rawAuth) {
      try {
        const { token } = JSON.parse(rawAuth);
        setTokenSessao(token);
        const dadosDoToken = extrairDadosDoToken(token);
        
        // Puxando as chaves gigantes do C# ASP.NET que descobrimos!
        const chaveNome = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname';
        const chaveEmail = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress';
        
        if (dadosDoToken?.[chaveNome]) setNomeReal(dadosDoToken[chaveNome]);
        if (dadosDoToken?.[chaveEmail]) setEmailReal(dadosDoToken[chaveEmail]);
      } catch (e) {
        console.error("Erro ao ler token na página de conta.");
      }
    }
  }, []);

  // Validação estrita: O botão só liga se digitar o nome real exato
  const podeDeletar = textoDigitado === nomeReal;

  // Conexão REAL com a API de deletar
  const handleDeletarConta = async () => {
    if (!podeDeletar || !tokenSessao) return;
    setCarregando(true);
    setErro("");
    
    try {
      const resposta = await fetch("https://letrify.fly.dev/api/usuario/deletar", {
        method: "DELETE",
        headers: {
          "Authorization": `Bearer ${tokenSessao}`
        }
      });

      if (resposta.ok) {
        alert("Sua conta e todos os dados foram removidos permanentemente.");
        // Limpa a sessão do navegador e chuta pro login
        localStorage.removeItem("letrify-auth");
        router.push("/login");
      } else {
        const dados = await resposta.json();
        setErro(dados.message || "Erro ao tentar excluir a conta.");
        setCarregando(false);
      }
    } catch (err) {
      setErro("Falha na conexão com o servidor.");
      setCarregando(false);
    }
  };

  return (
    <div className="max-w-2xl animate-fade-in">
      <h1 className="text-3xl font-bold mb-2" style={{ color: 'var(--cor-texto-principal)' }}>Minha Conta</h1>
      <p className="mb-8" style={{ color: 'var(--cor-texto-secundario)' }}>
        Gerencie suas informações pessoais e credenciais de acesso.
      </p>

      {/* BLOCO 1: INFORMAÇÕES BÁSICAS (Dados Reais, Botão Falso) */}
      <div 
        className="p-6 rounded-xl border shadow-sm mb-8"
        style={{ backgroundColor: 'var(--cor-fundo-card)', borderColor: 'var(--cor-fundo-sidebar)' }}
      >
        <h2 className="text-xl font-bold mb-4" style={{ color: 'var(--cor-texto-principal)' }}>Informações Básicas</h2>
        
        <form className="space-y-4" onSubmit={(e) => { e.preventDefault(); alert("A API ainda não suporta edição de nome/email."); }}>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <label className="block">
              <span className="text-sm font-semibold mb-1 block" style={{ color: 'var(--cor-texto-secundario)' }}>Nome de exibição</span>
              <input 
                type="text" 
                defaultValue={nomeReal}
                className="w-full p-2 rounded-md border outline-none"
                style={{ backgroundColor: 'var(--cor-fundo-app)', color: 'var(--cor-texto-principal)', borderColor: 'var(--cor-fundo-sidebar)' }}
              />
            </label>

            <label className="block">
              <span className="text-sm font-semibold mb-1 block" style={{ color: 'var(--cor-texto-secundario)' }}>E-mail</span>
              <input 
                type="email" 
                defaultValue={emailReal}
                className="w-full p-2 rounded-md border outline-none opacity-70 cursor-not-allowed"
                style={{ backgroundColor: 'var(--cor-fundo-app)', color: 'var(--cor-texto-principal)', borderColor: 'var(--cor-fundo-sidebar)' }}
                disabled
              />
            </label>
          </div>

          <div className="pt-4 mt-6 flex items-center justify-between border-t" style={{ borderColor: 'var(--cor-fundo-app)' }}>
            <button 
              type="submit"
              className="px-6 py-2 rounded-md font-bold transition-transform hover:scale-105"
              style={{ backgroundColor: 'var(--cor-botao-primario)', color: 'var(--cor-botao-texto)' }}
            >
              Salvar Alterações
            </button>
            <span className="text-xs italic" style={{ color: 'var(--cor-texto-secundario)' }}>(Visual por enquanto)</span>
          </div>
        </form>
      </div>

      {/* BLOCO 2: ALTERAR SENHA */}
      <div 
        className="p-6 rounded-xl border shadow-sm mb-8"
        style={{ backgroundColor: 'var(--cor-fundo-card)', borderColor: 'var(--cor-fundo-sidebar)' }}
      >
        <h2 className="text-xl font-bold mb-4" style={{ color: 'var(--cor-texto-principal)' }}>Segurança</h2>
        
        <label className="block max-w-sm mb-4">
          <span className="text-sm font-semibold mb-1 block" style={{ color: 'var(--cor-texto-secundario)' }}>Senha Atual</span>
          <input 
            type="password" 
            placeholder="••••••••"
            className="w-full p-2 rounded-md border outline-none"
            style={{ backgroundColor: 'var(--cor-fundo-app)', color: 'var(--cor-texto-principal)', borderColor: 'var(--cor-fundo-sidebar)' }}
          />
        </label>

        {!senhaEsquecida ? (
          <p className="text-sm">
            Perdeu sua senha?{' '}
            <button 
              onClick={() => setSenhaEsquecida(true)}
              className="font-bold hover:underline"
              style={{ color: 'var(--cor-primaria)' }}
            >
              Altere-a aqui
            </button>
          </p>
        ) : (
          <p className="text-sm font-bold p-3 rounded-md border" style={{ backgroundColor: 'var(--cor-fundo-app)', color: 'var(--cor-destaque)', borderColor: 'var(--cor-destaque)' }}>
            📧 Enviamos um e-mail com instruções para você.
          </p>
        )}
      </div>

      {/* BLOCO 3: DANGER ZONE (Totalmente Funcional e com Cores Sólidas) */}
      <div className="p-6 rounded-xl border-2 border-red-500 bg-[#fff5f5] dark:bg-[#450a0a] shadow-sm mt-12">
        <h2 className="text-xl font-bold text-red-700 dark:text-red-400 mb-2">Zona de Perigo</h2>
        <p className="text-sm text-red-900 dark:text-red-200 mb-6 font-medium">
          A exclusão da sua conta é permanente e removerá todos os seus dados. Esta ação não pode ser desfeita.
        </p>

        {erro && <p className="mb-4 text-red-700 font-bold">{erro}</p>}

        {!mostrarConfirmacao ? (
          <button 
            onClick={() => setMostrarConfirmacao(true)}
            className="px-4 py-2 bg-red-600 text-white font-bold rounded-md border border-red-800 hover:bg-red-700 transition-colors"
          >
            Excluir minha conta
          </button>
        ) : (
          <div className="mt-4 p-5 border border-red-300 rounded-lg bg-white dark:bg-gray-900 shadow-inner">
            <p className="text-sm mb-3" style={{ color: 'var(--cor-texto-principal)' }}>
              Para confirmar a exclusão, digite <span className="font-mono font-bold select-all bg-red-100 dark:bg-red-900/40 text-red-600 dark:text-red-400 px-2 py-1 rounded">{nomeReal}</span> abaixo:
            </p>
            
            <input 
              type="text" 
              value={textoDigitado}
              onChange={(e) => setTextoDigitado(e.target.value)}
              className="w-full p-2 rounded-md border border-red-400 focus:ring-2 focus:ring-red-500 outline-none mb-4 font-mono"
              style={{ backgroundColor: 'var(--cor-fundo-app)', color: 'var(--cor-texto-principal)' }}
              placeholder="Digite seu nome"
            />
            
            <div className="flex gap-3 flex-wrap">
              <button 
                onClick={handleDeletarConta}
                disabled={!podeDeletar || carregando}
                className="px-6 py-2 bg-red-600 text-white font-bold rounded-md hover:bg-red-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed flex-1 text-center"
              >
                {carregando ? "Excluindo para sempre..." : "Entendo as consequências, excluir conta"}
              </button>
              
              <button 
                onClick={() => {
                  setMostrarConfirmacao(false);
                  setTextoDigitado("");
                }}
                className="px-4 py-2 font-semibold rounded-md border bg-gray-100 hover:bg-gray-200 text-gray-800 transition-colors"
              >
                Cancelar
              </button>
            </div>
          </div>
        )}
      </div>

    </div>
  );
}