"use client";

import { useState, useEffect, Suspense } from "react";
import { useSearchParams, useRouter } from "next/navigation";
import Link from "next/link";
import useSWR from "swr";

// Componentes
import CabecalhoPerfil, { SkeletonCabecalho } from "@/components/CabecalhoPerfil";
import ResumoLateral from "@/components/ResumoLateral";
import VitrineDestaques from "@/components/VitrineDestaques";

// Libs e Services
import { mapearPerfilDaApi } from "@/app/lib/usuarioService";
import { authService } from "@/app/lib/authService";

// ----------------------------------------------------------------------
// 1. FETCHER UNIFICADO (Com Suporte a Token e Logs de Debug)
// ----------------------------------------------------------------------
const fetcherUsuarioDaApi = async (url: string) => {
  try {
    const token = authService.getToken(); 
    const cabecalhos: HeadersInit = { "Content-Type": "application/json" };

    if (token) {
      cabecalhos["Authorization"] = `Bearer ${token}`;
    }

    const resposta = await fetch(`https://letrify.fly.dev${url}`, {
      method: "GET",
      headers: cabecalhos,
    });

    if (!resposta.ok) throw new Error(`Erro na API: ${resposta.status}`);
    
    return await resposta.json();
  } catch (erro) {
    console.warn("Aviso do Front: Falha ao buscar dados do usuário.", erro);
    return null;
  }
};

// ----------------------------------------------------------------------
// 2. CONTEÚDO PRINCIPAL
// ----------------------------------------------------------------------
function ConteudoDoPerfil() {
  const router = useRouter();
  const searchParams = useSearchParams();
  
  const idDaUrl = searchParams.get("id");
  const isPreview = searchParams.get("preview") === "visitante";

  const [meuId, setMeuId] = useState<string | null>(null);
  const [carregandoId, setCarregandoId] = useState(true);
  
  // Estado das Abas (do arquivo 2)
  const [abaAtiva, setAbaAtiva] = useState<"livro" | "autores" | "temas">("livro");

  useEffect(() => {
    const idNoCofre = authService.getUserId();
    if (idNoCofre) {
      setMeuId(idNoCofre);
    } else if (!idDaUrl) {
      router.push("/login");
    }
    setCarregandoId(false);
  }, [idDaUrl, router]);

  const idParaBuscar = idDaUrl || meuId;
  const isDonoDoPerfil = (idParaBuscar === meuId) && !isPreview;

  // SWR para gerenciamento de cache e estado
  const { data: usuarioApi, error, isLoading, mutate } = useSWR(
    idParaBuscar ? `/api/usuario/informacoes/${idParaBuscar}` : null, 
    fetcherUsuarioDaApi
  );

  // Lógica de Seguir
  const handleFollowToggle = async () => {
    try {
      const token = authService.getToken();
      await fetch(`https://letrify.fly.dev/api/usuario/seguir/${idParaBuscar}`, { 
        method: 'POST',
        headers: { "Authorization": `Bearer ${token}` }
      });
      mutate(); // Recarrega os dados (número de seguidores)
    } catch (err) {
      console.error("Erro ao seguir", err);
    }
  };

  if (carregandoId) return <div className="p-8 text-center opacity-50">🔐 Validando sessão...</div>;
  if (error) return <div className="text-red-500 p-8 text-center font-bold">Erro crítico ao carregar perfil! 🚨</div>;
  
  if (isLoading || (idParaBuscar && !usuarioApi)) return (
    <div className="max-w-7xl mx-auto w-full pt-4"><SkeletonCabecalho /></div>
  );

  const perfilMapeado = mapearPerfilDaApi(usuarioApi);
  if (!perfilMapeado) return <div className="p-8 text-center">Perfil não encontrado.</div>;

  // Injeção de privacidade local (Arquivo 1)
  if (isDonoDoPerfil) {
    const dadosSalvos = typeof window !== "undefined" ? localStorage.getItem("letrify-privacidade") : null;
    if (dadosSalvos) {
      const preferencias = JSON.parse(dadosSalvos);
      perfilMapeado.isPrivado = preferencias.contaPrivada; 
    }
  }

  return (
    <div className="max-w-7xl mx-auto w-full pt-4 pb-20 relative px-4">
      
      {/* AVISO MODO VISITANTE */}
      {isPreview && (
        <div className="mb-6 flex justify-end">
          <div className="flex items-center gap-4 bg-red-500 text-white px-5 py-2 rounded-full shadow-lg">
            <span className="text-sm font-bold animate-pulse">🔴 Modo Visitante</span>
            <Link href="/privacidade" className="text-xs bg-black/20 hover:bg-black/40 px-3 py-1 rounded-full transition-colors font-bold">
              Sair
            </Link>
          </div>
        </div>
      )}

      {/* CABEÇALHO */}
      <CabecalhoPerfil 
        nome={perfilMapeado.nome}
        cidade={perfilMapeado.cidade}
        descricao={perfilMapeado.descricao}
        fotoPerfil={perfilMapeado.fotoPerfil}
        bannerUrl={perfilMapeado.bannerUrl}
        estatisticas={perfilMapeado.estatisticas}
        isDonoDoPerfil={isDonoDoPerfil}
        onFollowClick={handleFollowToggle}
      />

      {/* VERIFICAÇÃO DE PRIVACIDADE */}
      {perfilMapeado.isPrivado && !isDonoDoPerfil ? (
        <div className="mt-8 flex flex-col items-center justify-center p-12 rounded-2xl border-2 border-dashed border-white/10 bg-zinc-900/50">
          <span className="text-5xl mb-4">🔒</span>
          <h3 className="font-bold text-xl mb-1">Esta conta é privada</h3>
          <p className="text-sm text-center opacity-60">Siga este usuário para ver suas atividades literárias.</p>
        </div>
      ) : (
        <div className="mt-8 grid grid-cols-1 md:grid-cols-3 gap-6">
          
          {/* COLUNA ESQUERDA: ABAS E VITRINES */}
          <div className="md:col-span-2 space-y-8">
            
            {/* SELETOR DE ABAS DINÂMICAS */}
            <section className="bg-zinc-900/40 rounded-2xl border border-white/5 p-6">
                <div className="flex gap-6 mb-6 border-b border-white/5">
                    {["livro", "autores", "temas"].map((tab) => (
                        <button 
                            key={tab}
                            onClick={() => setAbaAtiva(tab as any)} 
                            className={`pb-2 text-sm font-bold capitalize transition-all ${abaAtiva === tab ? "border-b-2 border-blue-500 text-blue-500" : "opacity-50 hover:opacity-100"}`}
                        >
                            {tab === "livro" ? "Favorito" : tab}
                        </button>
                    ))}
                </div>

                <div className="flex flex-col items-center justify-center min-h-[300px]">
                    {abaAtiva === "livro" && (
                        perfilMapeado.favorito ? (
                            <div className="text-center animate-fade-in">
                                <div className="w-32 h-48 bg-zinc-800 rounded-lg shadow-2xl mb-4 mx-auto flex items-center justify-center border border-white/10">
                                    <span className="text-[10px] uppercase tracking-widest opacity-30">Capa</span>
                                </div>
                                <h4 className="text-xl font-bold">{perfilMapeado.favorito.titulo}</h4>
                                <p className="text-sm opacity-50">{perfilMapeado.favorito.autor}</p>
                            </div>
                        ) : <p className="opacity-40 italic">Nenhum livro favorito selecionado.</p>
                    )}

                    {abaAtiva === "autores" && (
                        <div className="w-full space-y-3 animate-fade-in">
                            {perfilMapeado.topAutores?.map((aut: any, idx: number) => (
                                <div key={idx} className="flex justify-between items-center bg-white/5 p-3 rounded-xl hover:bg-white/10 transition-colors">
                                    <span className="font-medium">{aut.nome}</span>
                                    <span className="text-blue-400 font-mono font-bold">{aut.valor}</span>
                                </div>
                            )) || <p className="opacity-40 text-center">Dados de autores indisponíveis.</p>}
                        </div>
                    )}

                    {abaAtiva === "temas" && (
                         <div className="w-full grid grid-cols-2 gap-3 animate-fade-in">
                            {perfilMapeado.topTemas?.map((tema: any, idx: number) => (
                                <div key={idx} className="p-4 bg-zinc-800/50 rounded-xl text-center border border-white/5">
                                    <p className="text-lg font-black text-blue-500">{tema.valor}%</p>
                                    <p className="text-[10px] uppercase font-bold opacity-50">{tema.nome}</p>
                                </div>
                            )) || <p className="opacity-40 text-center col-span-2">Nenhum tema mapeado.</p>}
                         </div>
                    )}
                </div>
            </section>

            {/* Vitrine de Destaques (Arquivo 1) */}
            <VitrineDestaques userId={idParaBuscar as string} />
          </div>

          {/* COLUNA DIREITA: RESUMO LATERAL */}
          <div className="md:col-span-1">
            <ResumoLateral 
              estante={perfilMapeado.estanteResumo} 
              totalGrupos={perfilMapeado.grupos} 
              totalGuias={perfilMapeado.guias} 
            />
          </div>
        </div>
      )}
    </div>
  );
}

// 3. EXPORTAÇÃO COM BOUNDARY DE SUSPENSE
export default function PerfilPage() {
  return (
    <Suspense fallback={<div className="max-w-7xl mx-auto w-full pt-4"><SkeletonCabecalho /></div>}>
      <ConteudoDoPerfil />
    </Suspense>
  );
}