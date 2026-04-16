"use client";

import { useState, useEffect } from "react";
import { authService } from "@/app/lib/authService";
import CardLivro, { LivroDados } from "@/components/CardLivro";

// Tipo para mapear a resposta da API
interface RespostaEstante {
  lendo: LivroDados[];
  lido: LivroDados[];
  queroLer: LivroDados[];
}

export default function EstanteUsuario() {
  const [estante, setEstante] = useState<RespostaEstante>({ lendo: [], lido: [], queroLer: [] });
  const [filtroAtivo, setFiltroAtivo] = useState<"Todos" | "Lendo" | "Lido" | "Quero Ler">("Todos");
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState("");

  useEffect(() => {
    const buscarEstante = async () => {
      setCarregando(true);
      setErro("");

      try {
        const token = authService.getToken();
        const userId = authService.getUserId();

        if (!token || !userId) {
          throw new Error("Você precisa estar logado para ver sua estante.");
        }

        const resposta = await fetch(`https://letrify.fly.dev/api/usuario/${userId}/livros`, {
          headers: {
            "Authorization": `Bearer ${token}`
          }
        });

        if (!resposta.ok) {
          throw new Error("Não foi possível carregar os livros da sua estante.");
        }

        const dados = await resposta.json();
        
        // Garante que se a API mandar null em alguma categoria, transforma em array vazio
        setEstante({
          lendo: dados.lendo || [],
          lido: dados.lido || [],
          queroLer: dados.queroLer || []
        });

      } catch (err: any) {
        setErro(err.message);
      } finally {
        setCarregando(false);
      }
    };

    buscarEstante();
  }, []);

  // Lógica de Filtro: Decide qual array mostrar na tela
  let livrosParaMostrar: LivroDados[] = [];
  
  if (filtroAtivo === "Todos") {
    livrosParaMostrar = [...estante.lendo, ...estante.lido, ...estante.queroLer];
  } else if (filtroAtivo === "Lendo") {
    livrosParaMostrar = estante.lendo;
  } else if (filtroAtivo === "Lido") {
    livrosParaMostrar = estante.lido;
  } else if (filtroAtivo === "Quero Ler") {
    livrosParaMostrar = estante.queroLer;
  }

  // Abas do menu
  const abas = ["Todos", "Lendo", "Lido", "Quero Ler"];

  return (
    <div className="w-full animate-fade-in">
      
      {/* O MENU DE FILTROS */}
      <div className="flex flex-wrap items-center justify-center sm:justify-start gap-3 mb-8 border-b pb-4" style={{ borderColor: 'var(--cor-fundo-sidebar)' }}>
        {abas.map((aba) => {
          const isAtivo = filtroAtivo === aba;
          return (
            <button
              key={aba}
              onClick={() => setFiltroAtivo(aba as any)}
              className={`px-5 py-2 rounded-full font-bold text-sm transition-all ${
                isAtivo ? 'shadow-md scale-105' : 'opacity-60 hover:opacity-100 border border-transparent'
              }`}
              style={{
                backgroundColor: isAtivo ? 'var(--cor-botao-primario)' : 'transparent',
                color: isAtivo ? 'var(--cor-botao-texto)' : 'var(--cor-texto-principal)',
                borderColor: isAtivo ? 'transparent' : 'var(--cor-fundo-sidebar)'
              }}
            >
              {aba === "Quero Ler" && "📌 "}
              {aba === "Lendo" && "📖 "}
              {aba === "Lido" && "✅ "}
              {aba}
            </button>
          );
        })}
      </div>

      {/* FEEDBACKS VISUAIS */}
      {carregando && (
        <div className="py-20 text-center opacity-50 font-bold animate-pulse" style={{ color: 'var(--cor-primaria)' }}>
          <span className="text-4xl block mb-4">📚</span>
          Tirando a poeira das prateleiras...
        </div>
      )}

      {erro && (
        <div className="p-4 rounded-xl bg-red-500/10 border border-red-500 text-red-500 text-center font-bold">
          {erro}
        </div>
      )}

      {!carregando && !erro && livrosParaMostrar.length === 0 && (
        <div className="text-center py-20 opacity-70 border-2 border-dashed rounded-2xl" style={{ borderColor: 'var(--cor-fundo-sidebar)' }}>
          <span className="text-5xl block mb-4">🏜️</span>
          <p className="font-bold text-xl" style={{ color: 'var(--cor-texto-principal)' }}>Nenhum livro por aqui.</p>
          <p className="text-sm" style={{ color: 'var(--cor-texto-secundario)' }}>
            {filtroAtivo === "Todos" 
              ? "Você ainda não salvou nenhum livro. Vá explorar!" 
              : `Você não tem livros marcados como "${filtroAtivo}".`}
          </p>
        </div>
      )}

      {/* A GRADE DE LIVROS */}
      {!carregando && !erro && livrosParaMostrar.length > 0 && (
        <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
          {livrosParaMostrar.map((livro, index) => (
           <div key={`${livro.isbn || 'livro'}-${index}`} className="h-full">
              <CardLivro livro={livro} variante="estante" />
            </div>
          ))}
        </div>
      )}

    </div>
  );
}