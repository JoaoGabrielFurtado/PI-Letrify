"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";

// 1. Nossa função auxiliar (fica de fora do componente)
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

// 2. O Componente Principal (Obrigatório ter o "export default")
export default function PerfilPage() {
  const router = useRouter();
  
  const [usuario, setUsuario] = useState<any>(null);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState("");

  useEffect(() => {
    const rawAuth = localStorage.getItem("letrify-auth");
    if (!rawAuth) {
      router.replace("/login");
      return;
    }

    try {
      const { token } = JSON.parse(rawAuth);
      const dadosDoToken = extrairDadosDoToken(token);
      
      // Usamos a chave exata (Name Identifier) que o ASP.NET coloca no Token
      const chaveId = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier';
      const usuarioId = dadosDoToken?.[chaveId];

      if (!usuarioId) {
        setErro("Não conseguimos ler o ID no seu token. O formato pode estar diferente na API.");
        setCarregando(false);
        return;
      }

      fetch(`https://letrify.fly.dev/api/usuario/${usuarioId}`, {
        method: "GET",
        headers: {
          "Authorization": `Bearer ${token}` 
        }
      })
        .then(res => {
          if (!res.ok) throw new Error("Falha ao carregar o perfil da API");
          return res.json();
        })
        .then(dados => {
          setUsuario(dados);
        })
        .catch(err => {
          console.error("Erro na busca do perfil:", err);
          setErro("Tivemos um problema ao carregar seus dados reais da API.");
        })
        .finally(() => {
          setCarregando(false);
        });

    } catch (error) {
      setErro("Sessão inválida. Por favor, faça login novamente.");
      setCarregando(false);
    }
  }, [router]);

  if (carregando) {
    return (
      <div className="flex justify-center items-center min-h-[50vh]">
        <p className="text-xl animate-pulse font-bold" style={{ color: 'var(--cor-primaria)' }}>
          Buscando seu perfil nas prateleiras... 📚
        </p>
      </div>
    );
  }

  if (erro) {
    return (
      <div className="text-center mt-20 p-8 rounded-xl border border-red-200 bg-red-50 text-red-600 max-w-lg mx-auto">
        <p className="font-bold">Ops!</p>
        <p>{erro}</p>
      </div>
    );
  }

  if (!usuario) return null;

  return (
    <div className="max-w-4xl mx-auto mt-8">
      
      <div 
        className="rounded-xl p-8 shadow-md border mb-8 flex flex-col md:flex-row items-center gap-6 transition-all"
        style={{ 
          backgroundColor: 'var(--cor-fundo-card)', 
          borderColor: 'var(--cor-fundo-sidebar)' 
        }}
      >
        <div 
          className="w-28 h-28 rounded-full flex items-center justify-center text-4xl font-bold shadow-lg overflow-hidden"
          style={{ backgroundColor: 'var(--cor-primaria)', color: 'var(--cor-botao-texto)' }}
        >
          {usuario.fotoPerfil ? (
            <img src={usuario.fotoPerfil} alt={`Foto de ${usuario.nome}`} className="w-full h-full object-cover" />
          ) : (
            usuario.nome ? usuario.nome.charAt(0).toUpperCase() : "U"
          )}
        </div>
        
        <div className="text-center md:text-left">
          <h1 className="text-3xl font-bold" style={{ color: 'var(--cor-texto-principal)' }}>
            {usuario.nome}
          </h1>
          
          <p className="mt-2 text-sm max-w-lg" style={{ color: 'var(--cor-texto-secundario)' }}>
            {usuario.descricao || "Olá! Sou novo no Letrify e ainda estou organizando a minha estante de livros. 📖"}
          </p>

          {usuario.cidade && (
            <p className="mt-2 text-xs font-semibold tracking-wide" style={{ color: 'var(--cor-destaque)' }}>
              📍 {usuario.cidade}
            </p>
          )}
          
          <div className="mt-4 flex gap-4 justify-center md:justify-start">
            <div className="text-center">
              <span className="block font-bold" style={{ color: 'var(--cor-primaria)' }}>0</span>
              <span className="text-xs uppercase tracking-wider" style={{ color: 'var(--cor-texto-secundario)' }}>Resenhas</span>
            </div>
            <div className="text-center">
              <span className="block font-bold" style={{ color: 'var(--cor-primaria)' }}>0</span>
              <span className="text-xs uppercase tracking-wider" style={{ color: 'var(--cor-texto-secundario)' }}>Lendo</span>
            </div>
          </div>
        </div>
      </div>

      <h2 className="text-xl font-bold mb-4" style={{ color: 'var(--cor-texto-principal)' }}>Minhas Atividades</h2>
      <div 
        className="rounded-xl p-10 text-center shadow-sm border"
        style={{ backgroundColor: 'var(--cor-fundo-card)', borderColor: 'var(--cor-fundo-sidebar)' }}
      >
        <p style={{ color: 'var(--cor-texto-secundario)' }}>
          Ainda não há resenhas publicadas. Que tal avaliar o seu primeiro livro?
        </p>
        <button 
          className="mt-6 px-6 py-2 rounded-md font-semibold transition-transform hover:scale-105 shadow-md"
          style={{ backgroundColor: 'var(--cor-botao-primario)', color: 'var(--cor-botao-texto)' }}
        >
          Buscar Livros 🔍
        </button>
      </div>

    </div>
  );
}