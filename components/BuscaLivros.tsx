"use client";

import { useState, FormEvent } from "react";
import CardLivro, { LivroDados } from "@/components/CardLivro"; 

// O DICIONÁRIO DE BOLSO (Tradutor de Gêneros)
const tradutorTemas: Record<string, string> = {
  "fantasia": "fantasy",
  "ficção": "fiction",
  "ficcao": "fiction",
  "terror": "horror",
  "horror": "horror",
  "romance": "romance",
  "ciência": "science",
  "ciencia": "science",
  "filosofia": "philosophy",
  "história": "history",
  "historia": "history"
};

export default function BuscaLivrosPage() {
  const [termo, setTermo] = useState("");
  const [carregando, setCarregando] = useState(false);
  const [buscaFeita, setBuscaFeita] = useState(false);

  const [prateleiras, setPrateleiras] = useState({
    isbn: [] as LivroDados[],
    titulos: [] as LivroDados[],
    autores: [] as LivroDados[],
    temas: [] as LivroDados[]
  });

  // Função auxiliar para não quebrar o Promise.all se uma rota falhar (Ex: 404 Not Found)
  const fetchSeguro = async (url: string) => {
    try {
      const res = await fetch(url);
      if (!res.ok) return [];
      const dados = await res.json();
      return Array.isArray(dados) ? dados : [dados]; // Se for ISBN, devolve um objeto, então envelopa em Array
    } catch {
      return [];
    }
  };

  const realizarBusca = async (e?: FormEvent, termoDireto?: string) => {
    if (e) e.preventDefault();
    const textoBusca = termoDireto || termo;
    if (!textoBusca.trim()) return;

    setCarregando(true);
    setBuscaFeita(true);
    
    // Zera as prateleiras antes de buscar
    setPrateleiras({ isbn: [], titulos: [], autores: [], temas: [] });

    // A BÚSSOLA ISBN
    const apenasNumeros = textoBusca.replace(/\D/g, '');
    const isIsbn = apenasNumeros.length === 10 || apenasNumeros.length === 13;

    if (isIsbn) {
      const livrosIsbn = await fetchSeguro(`https://letrify.fly.dev/api/livro/livroespecifico/${apenasNumeros}`);
      setPrateleiras(prev => ({ ...prev, isbn: livrosIsbn }));
      setCarregando(false);
      return;
    }

    // O TRADUTOR 
    const termoLimpo = textoBusca.toLowerCase().trim();
    const termoTema = tradutorTemas[termoLimpo] || textoBusca;

    // AS ROTAS DE BUSCA
    const urlTitulo = `https://letrify.fly.dev/api/livro/livrostitulo?titulo=${encodeURIComponent(textoBusca)}&quantidade=10`;
    const urlAutor = `https://letrify.fly.dev/api/livro/livrosautor?autor=${encodeURIComponent(textoBusca)}&quantidade=10`;
    const urlTema = `https://letrify.fly.dev/api/livro/livrostema?tema=${encodeURIComponent(termoTema)}&quantidade=10`;

    // Atira nas 3 rotas ao mesmo tempo e espera todas responderem (ou falharem)
    const [resultadosTitulos, resultadosAutores, resultadosTemas] = await Promise.all([
      fetchSeguro(urlTitulo),
      fetchSeguro(urlAutor),
      fetchSeguro(urlTema)
    ]);

    // Guarda tudo nas prateleiras
    setPrateleiras({
      isbn: [],
      titulos: resultadosTitulos,
      autores: resultadosAutores,
      temas: resultadosTemas
    });

    setCarregando(false);
  };

  // Calcula se tudo voltou vazio
  const tudoVazio = buscaFeita && !carregando && 
    prateleiras.isbn.length === 0 && 
    prateleiras.titulos.length === 0 && 
    prateleiras.autores.length === 0 && 
    prateleiras.temas.length === 0;

  return (
    <div className="max-w-7xl mx-auto mt-8 px-4 pb-20 animate-fade-in">
      
      {/* O CABEÇALHO LIMPO (Estilo Google) */}
      <div className="mb-12 flex flex-col items-center text-center">
        <h1 className="text-4xl font-black mb-6" style={{ color: 'var(--cor-texto-principal)' }}>Explorar 🔍</h1>
        
        <form onSubmit={realizarBusca} className="w-full max-w-2xl relative flex items-center shadow-lg rounded-full overflow-hidden border-2" style={{ borderColor: 'var(--cor-fundo-sidebar)', backgroundColor: 'var(--cor-fundo-app)' }}>
          <input 
            type="text"
            value={termo}
            onChange={(e) => setTermo(e.target.value)}
            placeholder="Busque por livros, autores, temas ou código ISBN..."
            className="flex-1 p-4 pl-6 bg-transparent outline-none font-medium"
            style={{ color: 'var(--cor-texto-principal)' }}
          />
          <button 
            type="submit"
            disabled={carregando}
            className="p-4 px-8 font-bold transition-colors disabled:opacity-50"
            style={{ backgroundColor: 'var(--cor-primaria)', color: 'var(--cor-fundo-app)' }}
          >
            {carregando ? "..." : "Buscar"}
          </button>
        </form>

        {/* Botoes rápidos de Gênero */}
        <div className="mt-6 flex flex-wrap justify-center gap-2">
          {["Fantasia", "Ficção", "Romance", "Terror", "Ciência", "Filosofia"].map((tema) => (
            <button
              key={tema}
              onClick={() => { setTermo(tema); realizarBusca(undefined, tema); }}
              className="px-4 py-1.5 rounded-full text-xs font-bold border transition-transform hover:scale-105"
              style={{ backgroundColor: 'var(--cor-fundo-sidebar)', color: 'var(--cor-texto-principal)', borderColor: 'var(--cor-fundo-sidebar)' }}
            >
              {tema}
            </button>
          ))}
        </div>
      </div>

      {/* FEEDBACK VISUAL */}
      {carregando && (
        <div className="flex flex-col items-center justify-center py-20 opacity-50">
          <span className="text-5xl animate-bounce mb-4">📚</span>
          <p className="font-bold text-xl" style={{ color: 'var(--cor-primaria)' }}>Vasculhando a biblioteca...</p>
        </div>
      )}

      {tudoVazio && (
        <div className="text-center py-20 opacity-70 border-2 border-dashed rounded-2xl" style={{ borderColor: 'var(--cor-fundo-sidebar)' }}>
          <span className="text-5xl block mb-4">🏜️</span>
          <p className="font-bold text-xl" style={{ color: 'var(--cor-texto-principal)' }}>Nenhum resultado encontrado.</p>
          <p className="text-sm" style={{ color: 'var(--cor-texto-secundario)' }}>Não encontramos livros, autores ou ISBNs com "{termo}".</p>
        </div>
      )}

      {/* AS PRATELEIRAS (Estilo Netflix / YouTube) */}
      {!carregando && buscaFeita && (
        <div className="space-y-12">

          {/* PRATELEIRA FALSA: USUÁRIOS (Sempre visível como demonstração) */}
          <section>
            <h2 className="text-xl font-black mb-4 flex items-center gap-2" style={{ color: 'var(--cor-texto-principal)' }}>
              👥 Perfis Encontrados
            </h2>
            <div className="flex overflow-x-auto gap-4 pb-4 snap-x hide-scrollbar">
              {/* Cards Falsos de Usuário */}
              {[1, 2, 3].map((num) => (
                <div key={num} className="min-w-[200px] snap-start shrink-0 p-4 rounded-xl border flex flex-col items-center text-center opacity-60" style={{ backgroundColor: 'var(--cor-fundo-card)', borderColor: 'var(--cor-fundo-sidebar)' }}>
                  <div className="w-16 h-16 rounded-full bg-zinc-300 dark:bg-zinc-700 mb-3"></div>
                  <span className="font-bold text-sm" style={{ color: 'var(--cor-texto-principal)' }}>Usuário {num}</span>
                  <span className="text-xs" style={{ color: 'var(--cor-texto-secundario)' }}>Em breve</span>
                </div>
              ))}
            </div>
          </section>

          {/* PRATELEIRA: ISBN (Só aparece se tiver resultado) */}
          {prateleiras.isbn.length > 0 && (
            <section>
              <h2 className="text-xl font-black mb-4" style={{ color: 'var(--cor-texto-principal)' }}>🏷️ Encontrado por ISBN</h2>
              <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 gap-6">
                {prateleiras.isbn.map((livro, i) => <CardLivro key={i} livro={livro} variante="busca" />)}
              </div>
            </section>
          )}

          {/* PRATELEIRA: TÍTULOS */}
          {prateleiras.titulos.length > 0 && (
            <section>
              <h2 className="text-xl font-black mb-4 flex justify-between items-end" style={{ color: 'var(--cor-texto-principal)' }}>
                <span>📖 Livros com este Título</span>
                <span className="text-xs opacity-50 cursor-pointer hover:underline">Ver todos</span>
              </h2>
              {/* Layout Carrossel Horizontal! */}
              <div className="flex overflow-x-auto gap-6 pb-4 snap-x hide-scrollbar">
                {prateleiras.titulos.map((livro, i) => (
                  <div key={i} className="min-w-[260px] snap-start shrink-0">
                    <CardLivro livro={livro} variante="busca" />
                  </div>
                ))}
              </div>
            </section>
          )}

          {/* PRATELEIRA: AUTORES */}
          {prateleiras.autores.length > 0 && (
            <section>
              <h2 className="text-xl font-black mb-4 flex justify-between items-end" style={{ color: 'var(--cor-texto-principal)' }}>
                <span>✍️ Livros deste Autor</span>
                <span className="text-xs opacity-50 cursor-pointer hover:underline">Ver todos</span>
              </h2>
              <div className="flex overflow-x-auto gap-6 pb-4 snap-x hide-scrollbar">
                {prateleiras.autores.map((livro, i) => (
                  <div key={i} className="min-w-[260px] snap-start shrink-0">
                    <CardLivro livro={livro} variante="busca" />
                  </div>
                ))}
              </div>
            </section>
          )}

          {/* PRATELEIRA: TEMAS */}
          {prateleiras.temas.length > 0 && (
            <section>
              <h2 className="text-xl font-black mb-4 flex justify-between items-end" style={{ color: 'var(--cor-texto-principal)' }}>
                <span>🎭 Explorando Gênero</span>
              </h2>
              <div className="flex overflow-x-auto gap-6 pb-4 snap-x hide-scrollbar">
                {prateleiras.temas.map((livro, i) => (
                  <div key={i} className="min-w-[260px] snap-start shrink-0">
                    <CardLivro livro={livro} variante="busca" />
                  </div>
                ))}
              </div>
            </section>
          )}

        </div>
      )}
    </div>
  );
}