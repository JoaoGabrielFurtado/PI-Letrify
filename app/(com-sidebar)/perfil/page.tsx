"use client";

import { useState, useEffect, Suspense} from "react";
import { useSearchParams, useRouter } from "next/navigation";
import Link from "next/link";
import useSWR from "swr";
import CabecalhoPerfil, { SkeletonCabecalho } from "@/components/CabecalhoPerfil";
import ResumoLateral from "@/components/ResumoLateral";
import { mapearPerfilDaApi } from "@/app/lib/usuarioService";
import { authService } from "@/app/lib/authService";

const fetcherUsuarioDaApi = async (url: string) => {
  try {
    const resposta = await fetch(`https://letrify.fly.dev${url}`);
    if (!resposta.ok) throw new Error(`Erro na API: ${resposta.status}`);
    return await resposta.json();
  } catch (erro) {
    console.warn("Aviso do Front: A API falhou.", erro);
    return null;
  }
};

// Criamos um componente interno com toda a lógica
function ConteudoDoPerfil() {
  const router = useRouter();
  const searchParams = useSearchParams();
  
  const idDaUrl = searchParams.get("id");
  const isPreview = searchParams.get("preview") === "visitante";

  const [meuId, setMeuId] = useState<string | null>(null);
  const [carregandoId, setCarregandoId] = useState(true);

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

  const { data: usuarioApi, error, isLoading } = useSWR(
    idParaBuscar ? `/api/usuario/${idParaBuscar}` : null, 
    fetcherUsuarioDaApi
  );

  if (carregandoId) return <div className="p-8 text-center opacity-50">🔐 Validando sessão...</div>;
  if (error) return <div className="text-red-500 p-8 text-center font-bold">Erro crítico! 🚨</div>;
  
  if (isLoading || (idParaBuscar && !usuarioApi)) return (
    <div className="max-w-7xl mx-auto w-full pt-4"><SkeletonCabecalho /></div>
  );

  const perfilMapeado = mapearPerfilDaApi(usuarioApi);
  if (!perfilMapeado) return <div className="p-8 text-center">Perfil não encontrado.</div>;

  // Injeção de privacidade local
  if (idParaBuscar === meuId) {
    const dadosSalvos = typeof window !== "undefined" ? localStorage.getItem("letrify-privacidade") : null;
    if (dadosSalvos) {
      const preferencias = JSON.parse(dadosSalvos);
      perfilMapeado.isPrivado = preferencias.contaPrivada; 
    }
  }

  return (
    <div className="max-w-7xl mx-auto w-full pt-4 pb-20 relative">
      {isPreview && (
        <div className="mb-6 flex justify-end">
          <div className="flex items-center gap-4 bg-red-500 text-white px-5 py-2 rounded-full shadow-lg">
            <span className="text-sm font-bold animate-pulse">🔴 Modo Visitante Ativo</span>
            <Link href="/privacidade" className="text-xs bg-black/20 hover:bg-black/40 px-3 py-1 rounded-full transition-colors font-bold">
              Voltar para Privacidade
            </Link>
          </div>
        </div>
      )}

      <CabecalhoPerfil 
        nome={perfilMapeado.nome || "Usuário"}
        cidade={perfilMapeado.cidade || ""}
        descricao={perfilMapeado.descricao || ""}
        fotoPerfil={perfilMapeado.fotoPerfil || ""}
        bannerUrl={perfilMapeado.bannerUrl || ""}
        estatisticas={perfilMapeado.estatisticas}
        isDonoDoPerfil={isDonoDoPerfil}
      />

      {perfilMapeado.isPrivado && !isDonoDoPerfil ? (
        <div className="mt-8 flex flex-col items-center justify-center p-12 rounded-2xl border-2 border-dashed" style={{ backgroundColor: 'var(--cor-fundo-card)', borderColor: 'var(--cor-fundo-sidebar)' }}>
          <span className="text-5xl mb-4">🔒</span>
          <h3 className="font-bold text-xl mb-1">Esta conta é privada</h3>
          <p className="text-sm text-center">Você não tem permissão para ver as informações de {perfilMapeado.nome}.</p>
        </div>
      ) : (
        <div className="mt-8 grid grid-cols-1 md:grid-cols-3 gap-6">
          <div className="md:col-span-2 border-2 border-dashed rounded-xl p-8 flex flex-col items-center justify-center opacity-50" style={{ borderColor: 'var(--cor-fundo-sidebar)' }}>
            <span className="text-3xl mb-3">🛠️</span>
            <p className="font-bold text-lg">Área das Abas e Vitrines</p>
            <p className="text-sm">(Próxima etapa da construção)</p>
          </div>
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

// A exportação principal agora "embrulha" o conteúdo no Suspense
export default function PerfilPage() {
  return (
    <Suspense fallback={<div className="max-w-7xl mx-auto w-full pt-4"><SkeletonCabecalho /></div>}>
      <ConteudoDoPerfil />
    </Suspense>
  );
}