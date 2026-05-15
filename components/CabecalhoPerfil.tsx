"use client";
import { useState, useEffect } from "react";
import EditorPerfil from "./EditorPerfil";
import { authService } from "@/app/lib/authService";

export function SkeletonCabecalho() {
  return (
    <div className="animate-pulse relative mb-8">
      <div className="h-56 w-full rounded-t-2xl bg-black/10 dark:bg-white/10"></div>
      <div className="px-8 pb-8 rounded-b-2xl shadow-sm border-x border-b" style={{ backgroundColor: 'var(--cor-fundo-card)', borderColor: 'var(--cor-fundo-sidebar)' }}>
        <div className="flex flex-col md:flex-row gap-6">
          <div className="flex flex-col items-center md:items-start shrink-0 -mt-16 z-10">
            <div className="w-36 h-36 border-4 bg-black/15 dark:bg-white/15" style={{ borderColor: 'var(--cor-fundo-card)', borderRadius: '2rem' }}></div>
            <div className="mt-4 h-4 w-24 bg-black/10 dark:bg-white/10 rounded-md"></div>
          </div>
          <div className="flex-1 mt-4 md:mt-2 flex flex-col justify-between">
            <div className="flex justify-between items-start md:items-center">
              <div className="h-8 w-48 bg-black/15 dark:bg-white/15 rounded-md"></div>
              <div className="h-10 w-32 bg-black/10 dark:bg-white/10 rounded-xl"></div>
            </div>
            <div className="mt-4 flex flex-col md:flex-row justify-between items-start md:items-end gap-4">
              <div className="space-y-2 w-full max-w-xl">
                <div className="h-4 w-full bg-black/10 dark:bg-white/10 rounded-md"></div>
                <div className="h-4 w-5/6 bg-black/10 dark:bg-white/10 rounded-md"></div>
              </div>
              <div className="h-5 w-40 bg-black/15 dark:bg-white/15 rounded-md shrink-0"></div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

interface CabecalhoProps {
  nome: string;
  cidade: string;
  descricao: string;
  fotoPerfil: string;
  bannerUrl: string;
  estatisticas: { seguindo: number; seguidores: number };
  isDonoDoPerfil: boolean;
  onFollowClick?: () => Promise<void>; 
  isSeguindo: boolean;
  onConexaoClick: (tipo: "seguidores" | "seguindo") => void;
}

export default function CabecalhoPerfil({
  nome: initialNome,
  cidade: initialCidade,
  descricao: initialDescricao,
  fotoPerfil: initialFoto,
  bannerUrl: initialBanner,
  estatisticas,
  isDonoDoPerfil,
  onFollowClick,
  isSeguindo,
  onConexaoClick
}: CabecalhoProps) {
  
  const [dadosPerfil, setDadosPerfil] = useState({
    nome: initialNome,
    cidade: initialCidade,
    descricao: initialDescricao,
    fotoPerfil: initialFoto,
    bannerUrl: initialBanner
  });

  const [isEditorAberto, setIsEditorAberto] = useState(false);
  const [estaSeguindoLocal, setEstaSeguindoLocal] = useState(isSeguindo);
  const [processandoFollow, setProcessandoFollow] = useState(false);

  useEffect(() => {
    setEstaSeguindoLocal(isSeguindo);
  }, [isSeguindo]);

  // 3. Função de clique ultra-reativa
  const handleFollowAction = async () => {
    if (!onFollowClick || processandoFollow) return;
    
    // Inverte o visual IMEDIATAMENTE na tela do usuário
    setEstaSeguindoLocal(!estaSeguindoLocal);
    setProcessandoFollow(true);

    try {
      await onFollowClick(); // Executa a requisição fetch da Page.tsx
    } catch (error) {
      // Se der erro na API, desfaz a alteração visual para não mentir pro usuário
      setEstaSeguindoLocal(isSeguindo);
      console.error("Erro ao seguir usuário:", error);
    } finally {
      setProcessandoFollow(false);
    }
  };

  useEffect(() => {
    setDadosPerfil({
      nome: initialNome,
      cidade: initialCidade,
      descricao: initialDescricao,
      fotoPerfil: initialFoto,
      bannerUrl: initialBanner
    });
  }, [initialNome, initialCidade, initialDescricao, initialFoto, initialBanner]);

  const handleSalvarDados = async (novosDados: any) => {
    try {
      const token = authService.getToken();
      if (!token) throw new Error("Você precisa estar logado para editar o perfil.");
    
      const formData = new FormData();
      if (novosDados.nome) formData.append("nome", novosDados.nome);
      if (novosDados.cidade) formData.append("cidade", novosDados.cidade);
      if (novosDados.descricao) formData.append("descricao", novosDados.descricao);

      if (novosDados.fotoPerfil instanceof File) {
        formData.append("foto", novosDados.fotoPerfil);
      }

      const resposta = await fetch("https://letrify.fly.dev/api/usuario/editar", {
        method: "PUT",
        headers: { "Authorization": `Bearer ${token}` },
        body: formData
      });

      if (resposta.ok) {
        setDadosPerfil(novosDados);
        setIsEditorAberto(false);
        alert("Perfil atualizado! ✨");
      } else {
        throw new Error("Erro ao atualizar perfil.");
      }
    } catch (err: any) {
      alert(err.message);
    }
  };

  const inicial = dadosPerfil.nome ? dadosPerfil.nome.charAt(0).toUpperCase() : "U";

  return (
    <div className="animate-fade-in relative mb-8">
      
      {/* BANNER */}
      <div 
        className="h-56 w-full rounded-t-2xl bg-cover bg-center relative"
        style={{ backgroundImage: `url("${dadosPerfil.bannerUrl}")` }}
      >
        <div className="absolute inset-0 bg-gradient-to-t from-black/50 to-transparent rounded-t-2xl"></div>
      </div>

      {/* CORPO DO PERFIL */}
      <div 
        className="px-8 pb-8 rounded-b-2xl shadow-sm border-x border-b relative"
        style={{ backgroundColor: 'var(--cor-fundo-card)', borderColor: 'var(--cor-fundo-sidebar)' }}
      >
        <div className="flex flex-col md:flex-row gap-6">
          
          {/* AVATAR E LOCALIZAÇÃO */}
          <div className="flex flex-col items-center md:items-start shrink-0 -mt-16 z-10">
            <div 
              className="w-36 h-36 flex items-center justify-center text-5xl font-bold shadow-xl border-4 object-cover overflow-hidden"
              style={{ 
                backgroundColor: 'var(--cor-primaria)', 
                color: 'var(--cor-botao-texto)', 
                borderColor: 'var(--cor-fundo-card)', 
                borderRadius: '2rem' 
              }}
            >
              {dadosPerfil.fotoPerfil ? (
                <img src={dadosPerfil.fotoPerfil} alt={dadosPerfil.nome} className="w-full h-full object-cover" />
              ) : (
                inicial
              )}
            </div>
            
            {dadosPerfil.cidade && (
              <div className="mt-3 flex items-center gap-1 font-semibold text-sm" style={{ color: 'var(--cor-texto-secundario)' }}>
                📍 {dadosPerfil.cidade}
              </div>
            )}
          </div>

          {/* INFOS E BOTÕES */}
          <div className="flex-1 mt-4 md:mt-2 flex flex-col justify-between">

            <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
              <h1 className="text-3xl font-black tracking-tight" style={{ color: 'var(--cor-texto-principal)' }}>
                {dadosPerfil.nome}
              </h1>
  
              {isDonoDoPerfil ? (
                <button 
                  onClick={() => setIsEditorAberto(true)}
                  className="px-5 py-2 text-sm font-bold rounded-xl shadow transition-transform hover:scale-105 active:scale-95"
                  style={{ 
                    backgroundColor: 'var(--cor-fundo-app)', 
                    color: 'var(--cor-texto-principal)', 
                    border: '2px solid var(--cor-destaque)' 
                   }}
                >
                  Editar Perfil
                </button>
              ) : (
                <button 
                  onClick={handleFollowAction} 
                  disabled={processandoFollow}
                  // O segredo está aqui: adicionamos a classe 'group' para controlar o hover do texto interno de forma elegante
                  className={`group px-8 py-2 text-sm font-bold rounded-xl shadow transition-all hover:scale-105 active:scale-95 disabled:opacity-40 flex items-center gap-2 border-2
                    ${estaSeguindoLocal 
                      ? "bg-zinc-200/90 dark:bg-zinc-800/90 hover:bg-red-500/10 hover:text-red-600 border-zinc-300 dark:border-zinc-700 hover:border-red-500/30 text-zinc-800 dark:text-zinc-200" 
                      : "border-transparent"
                    }`}
                  style={!estaSeguindoLocal ? { 
                    backgroundColor: 'var(--cor-botao-primario)', 
                    color: 'var(--cor-botao-texto)' 
                  } : undefined}
                >
                  {processandoFollow ? (
                    // Um spinner discreto ao lado do texto para mostrar que está salvando no banco C#
                    <div className="flex items-center gap-2">
                      <div className="w-3 h-3 border-2 border-current border-t-transparent rounded-full animate-spin"></div>
                      <span>{estaSeguindoLocal ? "Seguindo" : "Seguir"}</span>
                    </div>
                  ) : estaSeguindoLocal ? (
                    <>
                      {/* O texto padrão é 'Seguindo', mas ao passar o mouse (group-hover), vira 'Parar de seguir' */}
                      <span className="block group-hover:hidden">Seguindo</span>
                      <span className="hidden group-hover:block text-red-600">Parar de seguir</span>
                    </>
                  ) : (
                    "Seguir"
                  )}
                </button>
              )}
            </div>

            <div className="mt-4 flex flex-col gap-3">
              <div className="text-sm leading-relaxed max-w-2xl flex-1" style={{ color: 'var(--cor-texto-principal)' }}>
                {dadosPerfil.descricao || <span className="italic opacity-60">Este usuário ainda não escreveu uma biografia.</span>}
              </div>

              {/* CONEXÕES TOTALMENTE INTERATIVAS */}
              <div className="flex gap-4 text-sm font-semibold shrink-0 mb-1">
                <button 
                  onClick={() => onConexaoClick("seguindo")}
                  className="hover:underline text-left group" 
                >
                  <strong style={{ color: 'var(--cor-texto-principal)' }}>{estatisticas?.seguindo || 0}</strong>{" "}
                  <span style={{ color: 'var(--cor-texto-secundario)' }} className="group-hover:text-blue-400 transition-colors">Seguindo</span>
                </button>
                
                <button 
                  onClick={() => onConexaoClick("seguidores")}
                  className="hover:underline text-left group" 
                >
                  <strong style={{ color: 'var(--cor-texto-principal)' }}>{estatisticas?.seguidores || 0}</strong>{" "}
                  <span style={{ color: 'var(--cor-texto-secundario)' }} className="group-hover:text-blue-400 transition-colors">Seguidores</span>
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* MODAL DE EDIÇÃO */}
      {isEditorAberto && (
        <EditorPerfil 
          dadosIniciais={dadosPerfil} 
          onClose={() => setIsEditorAberto(false)} 
          onSave={handleSalvarDados}
        />
      )}
    </div>
  );
}