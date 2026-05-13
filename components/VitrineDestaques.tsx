"use client";

import { useMemo } from "react";
import useSWR from "swr";

interface VitrineDestaquesProps {
  userId: string;
}

const fetcher = (url: string) => fetch(url).then((res) => res.json());

export default function VitrineDestaques({ userId }: VitrineDestaquesProps) {
  // Buscamos os livros do usuário
  const { data, error, isLoading } = useSWR(
    userId ? `https://letrify.fly.dev/api/usuario/${userId}/livros` : null,
    fetcher
  );

  // Unifica as listas de livros (Lendo, Lido, Quero Ler) para a vitrine
  const livrosParaExibir = useMemo(() => {
    if (!data) return [];
    
    if (Array.isArray(data)) return data;

    // Se a API retornar o objeto separado por status:
    const todos = [
      ...(data.lendo || []),
      ...(data.lido || []),
      ...(data.queroLer || [])
    ];
    
    // Remove duplicatas por ID ou Título se necessário e limita a quantidade
    return todos.slice(0, 12); 
  }, [data]);

  if (isLoading) {
    return (
      <div className="mt-6 p-6 rounded-2xl border border-white/5 bg-zinc-900/20 animate-pulse">
        <div className="h-6 w-32 bg-white/10 rounded mb-6"></div>
        <div className="flex gap-4 overflow-hidden">
          {[1, 2, 3, 4, 5].map((i) => (
            <div key={i} className="shrink-0 w-28 aspect-[2/3] bg-white/5 rounded-lg"></div>
          ))}
        </div>
      </div>
    );
  }

  // Se não houver livros, não renderiza a seção de vitrine
  if (error || livrosParaExibir.length === 0) return null;

  return (
    <div 
      className="mt-6 p-6 rounded-2xl border transition-all" 
      style={{ 
        backgroundColor: 'var(--cor-fundo-card)', 
        borderColor: 'var(--cor-fundo-sidebar)' 
      }}
    >
      <div className="flex items-center justify-between mb-6">
        <h2 className="text-lg font-bold flex items-center gap-2" style={{ color: 'var(--cor-texto-principal)' }}>
          <span style={{ color: 'var(--cor-primaria)' }}>📚</span> Vitrine de Leituras
        </h2>
        <span className="text-xs font-medium opacity-50">Recentes</span>
      </div>

      {/* GRID/CARROSSEL DE LIVROS */}
      <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-6 gap-4 animate-fade-in">
        {livrosParaExibir.map((livro, i) => (
          <div key={i} className="group relative flex flex-col items-center">
            {/* CAPA DO LIVRO */}
            <div 
              className="relative aspect-[2/3] w-full rounded-xl overflow-hidden shadow-md border border-white/5 transition-all duration-300 group-hover:scale-105 group-hover:shadow-xl group-hover:border-blue-500/50"
              style={{ backgroundColor: 'var(--cor-fundo-app)' }}
            >
              <img 
                src={livro.capa || `https://covers.openlibrary.org/b/isbn/${livro.isbn}-M.jpg`} 
                alt={livro.titulo}
                className="w-full h-full object-cover"
                onError={(e) => {
                  e.currentTarget.src = "https://via.placeholder.com/150x225?text=Sem+Capa";
                }}
              />
              
              {/* OVERLAY AO PASSAR O MOUSE */}
              <div className="absolute inset-0 bg-black/60 opacity-0 group-hover:opacity-100 transition-opacity flex flex-col justify-end p-2">
                <p className="text-[10px] font-bold text-white line-clamp-1">{livro.status || 'Leitura'}</p>
              </div>
            </div>

            {/* INFO DO LIVRO */}
            <div className="mt-3 w-full text-center">
              <p 
                className="text-xs font-bold line-clamp-1 group-hover:text-blue-400 transition-colors" 
                style={{ color: 'var(--cor-texto-principal)' }}
              >
                {livro.titulo}
              </p>
              <p className="text-[10px] opacity-50 truncate" style={{ color: 'var(--cor-texto-secundario)' }}>
                {livro.autor}
              </p>
            </div>
          </div>
        ))}
      </div>

      {/* RODAPÉ DA VITRINE */}
      <div className="mt-8 pt-4 border-t border-white/5 flex justify-center">
        <button 
          className="text-xs font-bold py-2 px-4 rounded-full transition-colors hover:bg-white/5"
          style={{ color: 'var(--cor-primaria)' }}
        >
          Ver Estante Completa →
        </button>
      </div>
    </div>
  );
}