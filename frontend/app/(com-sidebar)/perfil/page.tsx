"use client";

import { useState } from "react";
import { useSearchParams } from "next/navigation";
import Link from "next/link";
import useSWR from "swr";
import CabecalhoPerfil, { SkeletonCabecalho } from "@/components/CabecalhoPerfil";
import { obterPerfilSeguro } from "@/app/lib/usuarioService";

// ----------------------------------------------------------------------
// 1. SIMULADOR DA API REAL (O que o Back-end em C# vai te entregar)
// Num projeto pronto, esse fetcher faria um 'fetch' real para 'https://api.letrify.com/usuario/1'
// ----------------------------------------------------------------------
const fetcherUsuarioDaApi = async () => {
  // Simulamos um delay de internet de 1.5 segundos para você ver o Skeleton brilhar!
  await new Promise((resolve) => setTimeout(resolve, 1500));
  
  return {
    id: 1,
    nome: "Tony",
    fotoPerfil: "", // Deixamos vazio para forçar o componente a renderizar a inicial "T"
    cidade: "Santos - SP",
    descricao: "Desenvolvedor Fullstack em formação. Mestre de campanhas de RPG nas horas vagas, entusiasta de filosofia e sempre acompanhado por um gato branco de rabo preto. 🎸🐈",
    // Uma estante crua, como um banco de dados relacional devolveria
    estante: [
      { id: 101, titulo: "O Senhor dos Anéis", status: "lido" },
      { id: 102, titulo: "1984", status: "lido" },
      { id: 103, titulo: "Clean Code", status: "lendo" },
      { id: 104, titulo: "O Hobbit", status: "queroLer" },
      { id: 105, titulo: "Duna", status: "queroLer" },
    ]
  };
};

export default function PerfilPage() {
  // Estado temporário só para você testar a privacidade transitando entre Dono e Visitante
  const searchParams = useSearchParams();
  
  // Verifica se a URL tem "?preview=visitante"
  const isPreview = searchParams.get("preview") === "visitante";
  
  // Se for preview, o dono é false. Se não, é true (temporário até termos login).
  const isDonoDoPerfil = !isPreview;

  // ----------------------------------------------------------------------
  // 2. O CÉREBRO BUSCANDO OS DADOS (SWR)
  // ----------------------------------------------------------------------
  const { data: usuarioApi, error, isLoading } = useSWR("usuario/1", fetcherUsuarioDaApi);

  // ----------------------------------------------------------------------
  // 3. CONTROLE DE TELA (Loading e Erro)
  // ----------------------------------------------------------------------
  if (error) return <div className="text-red-500 p-8 font-bold">Erro ao carregar o perfil! O servidor caiu? 😱</div>;
  
  // Enquanto o SWR está "pensando", mostramos o esqueleto perfeito
  if (isLoading) return (
    <div className="max-w-7xl mx-auto w-full pt-4">
      <SkeletonCabecalho />
    </div>
  );

  // ----------------------------------------------------------------------
  // 4. A MURALHA (O Firewall atuando antes de desenhar a tela)
  // ----------------------------------------------------------------------
  // Passamos o dado sujo da API e perguntamos se é o dono. Ele devolve o dado limpo!
  const perfilSeguro = obterPerfilSeguro(usuarioApi, isDonoDoPerfil);

  // ----------------------------------------------------------------------
  // 5. RENDERIZAÇÃO (Plugando o cabo no Componente Burro)
  // ----------------------------------------------------------------------
  return (
    <div className="max-w-7xl mx-auto w-full pt-4 pb-20 relative">

      {/* AVISO E BOTÃO DE SAÍDA DO PREVIEW */}
      {isPreview && (
        <div className="mb-6 flex justify-end">
          <div className="flex items-center gap-4 bg-red-500 text-white px-5 py-2 rounded-full shadow-lg">
            <span className="text-sm font-bold animate-pulse">🔴 Modo Visitante Ativo</span>
            <Link 
              href="/privacidade" 
              className="text-xs bg-black/20 hover:bg-black/40 px-3 py-1 rounded-full transition-colors font-bold"
            >
              Voltar para Privacidade
            </Link>
          </div>
        </div>
      )}

      {/* O CABEÇALHO BURRO RECEBENDO OS DADOS PROCESSADOS */}
      <CabecalhoPerfil 
        nome={perfilSeguro.nome}
        cidade={perfilSeguro.cidade}
        descricao={perfilSeguro.descricao}
        fotoPerfil={perfilSeguro.fotoPerfil}
        bannerUrl={perfilSeguro.bannerUrl}
        estatisticas={perfilSeguro.estatisticas}
        isDonoDoPerfil={isDonoDoPerfil}
      />

     {/* 4. O NOVO SETOR 3 (O GRID DE 2 COLUNAS) */}
        <div className="mt-8 grid grid-cols-1 md:grid-cols-3 gap-6">
          
          {/* LADO ESQUERDO (2/3 do espaço) - Futuras Abas e Vitrines */}
          <div className="md:col-span-2 border-2 border-dashed rounded-xl p-8 opacity-50 flex items-center justify-center" style={{ borderColor: 'var(--cor-fundo-sidebar)' }}>
             🚧 Espaço Gigante para as Abas e Vitrines...
          </div>

          {/* LADO DIREITO (1/3 do espaço) - O Resumo Lateral */}
          <div className="md:col-span-1">
             {/* Aqui entra o nosso Lego: <ResumoLateral estante={perfilSeguro.estante} /> */}
          </div>

        </div>

    </div>
  );
}