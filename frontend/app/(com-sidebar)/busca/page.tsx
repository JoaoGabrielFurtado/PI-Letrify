"use client";

import { useState, FormEvent } from "react";

export default function BuscaLivros() {
  const [termo, setTermo] = useState("");
  const [filtro, setFiltro] = useState("titulo"); // Pode ser: titulo, autor, isbn
  const [livros, setLivros] = useState<any[]>([]);
  const [carregando, setCarregando] = useState(false);
  const [erro, setErro] = useState("");

  // Função para lidar com a barra de pesquisa
  const realizarBusca = async (e?: FormEvent) => {
    if (e) e.preventDefault();
    if (!termo.trim()) return;

    setCarregando(true);
    setErro("");
    setLivros([]);

    try {
      let url = "";
      
      // Decidindo qual endpoint chamar com base no seletor
      if (filtro === "titulo") {
        url = `https://letrify.fly.dev/api/livro/livrostitulo?titulo=${encodeURIComponent(termo)}&quantidade=20`;
      } else if (filtro === "autor") {
        url = `https://letrify.fly.dev/api/livro/livrosautor?autor=${encodeURIComponent(termo)}&quantidade=20`;
      } else if (filtro === "isbn") {
        url = `https://letrify.fly.dev/api/livro/livroespecifico/${encodeURIComponent(termo)}`;
      }

      const resposta = await fetch(url);
      if (!resposta.ok) throw new Error("Livro não encontrado ou erro na API.");
      
      const dados = await resposta.json();

      // O ISBN retorna 1 livro só (objeto), os outros retornam listas (array)
      // Precisamos normalizar isso para o nosso estado "livros" sempre ser um Array
      if (filtro === "isbn") {
        setLivros([dados]); 
      } else {
        setLivros(dados); // Assumindo que a API devolve um Array direto
      }

    } catch (err) {
      setErro("Nenhum livro encontrado com esse termo.");
    } finally {
      setCarregando(false);
    }
  };

  // Função rápida para os botões de gênero
  const buscarPorTema = async (tema: string) => {
    setTermo(tema);
    setFiltro("tema"); // Usamos isso só visualmente
    setCarregando(true);
    setErro("");
    setLivros([]);

    try {
      const url = `https://letrify.fly.dev/api/livro/livrostema?tema=${encodeURIComponent(tema)}&quantidade=20`;
      const resposta = await fetch(url);
      if (!resposta.ok) throw new Error("Erro ao buscar tema");
      const dados = await resposta.json();
      setLivros(dados);
    } catch (err) {
      setErro("Não conseguimos carregar os livros desse gênero.");
    } finally {
      setCarregando(false);
    }
  };

  // Adicione esta função dentro do seu componente BuscaLivros, antes do "return"
  const adicionarNaEstante = async (livro: any) => {
    const rawAuth = localStorage.getItem("letrify-auth");
    if (!rawAuth) {
      alert("Você precisa estar logado para adicionar livros!");
      return;
    }

    try {
      const { token } = JSON.parse(rawAuth);
      
      const resposta = await fetch("https://letrify.fly.dev/api/usuario/meus-livros", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          "Authorization": `Bearer ${token}`
        },
        // Enviando no formato exato que a API espera
        body: JSON.stringify({
          titulo: livro.titulo || "Sem Título",
          autor: livro.autor || livro.autores?.[0] || livro.autorPrincipal || "Desconhecido",
          isbn: livro.isbn || "Sem ISBN",
          status: "Quero Ler" // Por padrão, vamos colocar na estante "Quero Ler"
        })
      });

      if (resposta.ok) {
        alert(`Sucesso! "${livro.titulo}" foi adicionado à sua estante.`);
      } else {
        const erroMsg = await resposta.text();
        if (erroMsg.includes("duplicate") || erroMsg.includes("unique")) {
          alert("Você já tem esse livro na sua estante!");
        } else {
          alert("Erro ao adicionar o livro.");
        }
      }
    } catch (err) {
      alert("Erro de conexão ao tentar salvar o livro.");
    }
  };

  return (
    <div className="max-w-6xl mx-auto mt-8 px-4">
      
      {/* CABEÇALHO E BARRA DE PESQUISA */}
      <div 
        className="rounded-xl p-8 shadow-md border mb-8 text-center"
        style={{ backgroundColor: 'var(--cor-fundo-card)', borderColor: 'var(--cor-fundo-sidebar)' }}
      >
        <h1 className="text-3xl font-bold mb-6" style={{ color: 'var(--cor-texto-principal)' }}>
          Explorar Livros 🔍
        </h1>
        
        <form onSubmit={realizarBusca} className="flex flex-col md:flex-row gap-4 max-w-3xl mx-auto">
          {/* Seletor de Tipo de Busca */}
          <select 
            value={filtro} 
            onChange={(e) => setFiltro(e.target.value)}
            className="p-3 rounded-md border focus:outline-none focus:ring-2 font-medium"
            style={{ backgroundColor: 'var(--cor-fundo-app)', color: 'var(--cor-texto-principal)', borderColor: 'var(--cor-destaque)' }}
          >
            <option value="titulo">Título</option>
            <option value="autor">Autor</option>
            <option value="isbn">ISBN</option>
          </select>

          {/* Campo de Digitação */}
          <input 
            type="text"
            value={termo}
            onChange={(e) => setTermo(e.target.value)}
            placeholder={`Digite o ${filtro} que procura...`}
            className="flex-1 p-3 rounded-md border focus:outline-none focus:ring-2"
            style={{ backgroundColor: 'var(--cor-fundo-app)', color: 'var(--cor-texto-principal)' }}
          />

          {/* Botão de Busca */}
          <button 
            type="submit"
            className="px-8 py-3 rounded-md font-bold transition-transform hover:scale-105"
            style={{ backgroundColor: 'var(--cor-botao-primario)', color: 'var(--cor-botao-texto)' }}
          >
            Buscar
          </button>
        </form>

        {/* VITRINE DE TEMAS */}
        <div className="mt-8 flex flex-wrap justify-center gap-3">
          {["fiction", "fantasy", "romance", "horror", "science", "philosophy"].map((tema) => (
            <button
              key={tema}
              onClick={() => buscarPorTema(tema)}
              className="px-4 py-2 rounded-full text-sm font-semibold border transition-all hover:opacity-80 uppercase tracking-wider"
              style={{ 
                backgroundColor: 'var(--cor-fundo-app)', 
                color: 'var(--cor-texto-principal)',
                borderColor: 'var(--cor-destaque)'
              }}
            >
              {tema}
            </button>
          ))}
        </div>
      </div>

      {/* ÁREA DE RESULTADOS */}
      {carregando && (
        <p className="text-center text-xl font-bold animate-pulse mt-10" style={{ color: 'var(--cor-primaria)' }}>
          Vasculhando as prateleiras... 📚
        </p>
      )}

      {erro && (
        <div className="text-center p-6 rounded-lg text-red-600 bg-red-50 border border-red-200">
          <p>{erro}</p>
        </div>
      )}

      {/* GRADE DE LIVROS */}
      {!carregando && !erro && livros.length > 0 && (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
          {livros.map((livro, index) => (
            <div 
              key={livro.isbn || index} 
              className="rounded-lg overflow-hidden shadow-sm border flex flex-col transition-transform hover:-translate-y-1 hover:shadow-lg"
              style={{ backgroundColor: 'var(--cor-fundo-card)', borderColor: 'var(--cor-fundo-sidebar)' }}
            >
                {/* Capa do Livro */}
              <div 
                className="h-64 flex items-center justify-center p-4"
                style={{ backgroundColor: 'var(--cor-fundo-app)' }}
              >
                {/* Se o livro tiver ISBN, tenta puxar a capa da Open Library */}
                {livro.isbn && livro.isbn !== 'Sem ISBN' ? (
                  <img 
                    src={`https://covers.openlibrary.org/b/isbn/${livro.isbn.trim()}-M.jpg`} 
                    alt={`Capa de ${livro.titulo}`} 
                    className="h-full object-contain shadow-md rounded"
                    // Um truquezinho maroto: se a imagem falhar (404), ele mostra o emoji de livro!
                    onError={(e) => {
                      e.currentTarget.style.display = 'none';
                      e.currentTarget.parentElement!.innerHTML = '<span class="text-5xl opacity-20">📖</span>';
                    }}
                  />
                ) : (
                  // Se não tiver ISBN, mostra o emoji direto
                  <span className="text-5xl opacity-20">📖</span>
                )}
              </div>
              
              {/* Informações */}
              <div className="p-4 flex flex-col flex-1">
                <h3 className="font-bold text-lg line-clamp-2" style={{ color: 'var(--cor-texto-principal)' }}>
                  {livro.titulo || "Título Desconhecido"}
                </h3>
                <p className="text-sm mt-1" style={{ color: 'var(--cor-texto-secundario)' }}>
                  {livro.autor || livro.autores?.join(", ") || livro.autorPrincipal || "Autor Desconhecido"}
                </p>
                {/* Se tiver ISBN, mostra bem pequenininho */}
                {livro.isbn && livro.isbn !== 'Sem ISBN' && (
                  <p className="text-xs mt-2 opacity-50" style={{ color: 'var(--cor-texto-principal)' }}>
                    ISBN: {livro.isbn}
                  </p>
                )}

                {/* Botão de Estante */}
                <button 
                  onClick={() => adicionarNaEstante(livro)}
                  className="mt-auto py-2 w-full rounded font-semibold text-sm transition-opacity hover:opacity-80"
                  style={{ backgroundColor: 'var(--cor-fundo-sidebar)', color: 'var(--cor-texto-sidebar)' }}
                >
                  + Adicionar à Estante
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

    </div>
  );
}