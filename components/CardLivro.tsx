"use client";

import { useState } from "react";
import { authService } from "@/app/lib/authService";

export interface LivroDados {
  isbn?: string;
  titulo?: string;
  autor?: string;
  autores?: string[];
  autorPrincipal?: string;
  status?: string; // Se vier da estante, já sabemos o status atual
}

interface CardLivroProps {
  livro: LivroDados;
  // Nova organização de variantes:
  // "busca" -> Os 3 botões empilhados (Pedido do Prof)
  // "estante" -> Os 3 emojis na horizontal para trocar de status
  variante?: "busca" | "estante" | "simples"; // "simples" é só um botão grande para as vitrines
}

export default function CardLivro({ livro, variante = "busca" }: CardLivroProps) {
  const [carregandoStatus, setCarregandoStatus] = useState<string | null>(null);
  // Agora o feedback sabe qual botão mostrar a mensagem!
  const [sucessoNoBotao, setSucessoNoBotao] = useState<string | null>(null);

  const tituloFinal = livro.titulo || "Título Desconhecido";
  const autorFinal = livro.autor || (livro.autores && livro.autores.join(", ")) || livro.autorPrincipal || "Autor Desconhecido";
  const isbnFinal = livro.isbn && livro.isbn !== 'Sem ISBN' ? livro.isbn.trim() : null;

  const adicionarNaEstante = async (statusEscolhido: string) => {
    setCarregandoStatus(statusEscolhido);
    setSucessoNoBotao(null);

    try {
      const token = authService.getToken();
      if (!token) throw new Error("Faça login para salvar.");

      const resposta = await fetch("https://letrify.fly.dev/api/usuario/meus-livros", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          "Authorization": `Bearer ${token}`
        },
        body: JSON.stringify({
          titulo: tituloFinal,
          autor: autorFinal,
          isbn: isbnFinal || "Sem ISBN",
          status: statusEscolhido 
        })
      });

      if (resposta.ok || resposta.status === 200) {
        // Se deu certo, mostra "Salvo!" só no botão que o cara clicou
        setSucessoNoBotao(statusEscolhido);
      } else {
        const erroMsg = await resposta.text();
        if (erroMsg.includes("duplicate") || erroMsg.includes("unique")) {
           alert("Livro já está nesta prateleira!");
        } else {
           alert("Erro ao adicionar.");
        }
      }
    } catch (err: any) {
      alert(err.message);
    } finally {
      setCarregandoStatus(null);
      setTimeout(() => setSucessoNoBotao(null), 2500); // O "Salvo!" some depois de 2.5s
    }
  };

  return (
    <div 
      className="rounded-xl overflow-hidden shadow-sm border flex flex-col transition-transform hover:-translate-y-1 hover:shadow-lg h-full bg-card-limpo"
      style={{ borderColor: 'var(--cor-fundo-sidebar)' }}
    >
      <div className="h-64 flex items-center justify-center p-4 bg-black/5 dark:bg-white/5">
        {isbnFinal ? (
          <img 
            src={`https://covers.openlibrary.org/b/isbn/${isbnFinal}-M.jpg`} 
            alt={`Capa`} 
            className="h-full object-contain shadow-md rounded"
            onError={(e) => {
              e.currentTarget.style.display = 'none';
              e.currentTarget.parentElement!.innerHTML = '<span class="text-5xl opacity-20">📖</span>';
            }}
          />
        ) : (
          <span className="text-5xl opacity-20">📖</span>
        )}
      </div>
      
      <div className="p-4 flex flex-col flex-1">
        <h3 className="font-bold text-sm lg:text-base line-clamp-2 leading-tight" style={{ color: 'var(--cor-texto-principal)' }} title={tituloFinal}>
          {tituloFinal}
        </h3>
        <p className="text-xs mt-1 line-clamp-1 opacity-70" style={{ color: 'var(--cor-texto-principal)' }} title={autorFinal}>
          {autorFinal}
        </p>
        
        {isbnFinal && (
          <p className="text-[10px] mt-2 opacity-40 font-mono" style={{ color: 'var(--cor-texto-principal)' }}>
            ISBN: {isbnFinal}
          </p>
        )}

        <div className="mt-auto pt-4">
          
          {/* VARIANTE 1: BUSCA (Os 3 botões empilhados com feedback interno) */}
          {variante === "busca" && (
            <div className="flex flex-col gap-2">
              {[
                { label: "Quero Ler", icone: "📌" },
                { label: "Lendo", icone: "📖" },
                { label: "Lido", icone: "✅" }
              ].map((btn) => (
                <button 
                  key={btn.label}
                  onClick={() => adicionarNaEstante(btn.label)} 
                  disabled={!!carregandoStatus}
                  className={`py-1.5 px-3 rounded text-xs font-bold border transition-colors flex items-center justify-center gap-2 ${
                    sucessoNoBotao === btn.label ? 'bg-green-500/10 border-green-500 text-green-500' : 'hover:opacity-80'
                  }`}
                  style={sucessoNoBotao === btn.label ? {} : { borderColor: 'var(--cor-fundo-sidebar)', color: 'var(--cor-texto-principal)' }}
                >
                  {carregandoStatus === btn.label ? "⏳ ..." : sucessoNoBotao === btn.label ? "Salvo!" : `${btn.icone} ${btn.label}`}
                </button>
              ))}
            </div>
          )}

          {/* VARIANTE 2: ESTANTE (Os 3 emojis na horizontal para mover livros) */}
          {variante === "estante" && (
            <div className="flex justify-between items-center gap-2 border-t pt-3 mt-1" style={{ borderColor: 'var(--cor-fundo-sidebar)' }}>
              {[
                { label: "Quero Ler", icone: "📌", title: "Mover para Quero Ler" },
                { label: "Lendo", icone: "📖", title: "Mover para Lendo" },
                { label: "Lido", icone: "✅", title: "Mover para Lido" }
              ].map((btn) => (
                <button
                  key={btn.label}
                  title={btn.title}
                  onClick={() => adicionarNaEstante(btn.label)}
                  disabled={!!carregandoStatus}
                  className={`flex-1 py-1.5 rounded-md text-sm transition-all flex items-center justify-center border ${
                    sucessoNoBotao === btn.label ? 'bg-green-500/20 border-green-500' : 'bg-transparent hover:bg-black/5 dark:hover:bg-white/5'
                  }`}
                  style={sucessoNoBotao === btn.label ? {} : { borderColor: 'var(--cor-fundo-sidebar)' }}
                >
                  {carregandoStatus === btn.label ? "⏳" : sucessoNoBotao === btn.label ? "✨" : btn.icone}
                </button>
              ))}
            </div>
          )}

          {/* VARIANTE 3: SIMPLES (1 botão grande para as Vitrines) */}
          {variante === "simples" && (
            <button 
              onClick={() => adicionarNaEstante("Quero Ler")} 
              disabled={!!carregandoStatus}
              className={`py-2 w-full rounded font-bold text-sm transition-opacity border flex items-center justify-center ${
                sucessoNoBotao ? 'bg-green-500/10 border-green-500 text-green-500' : 'hover:opacity-80'
              }`}
              style={sucessoNoBotao ? {} : { backgroundColor: 'var(--cor-fundo-sidebar)', color: 'var(--cor-texto-sidebar)', borderColor: 'var(--cor-fundo-sidebar)' }}
            >
              {carregandoStatus ? "Salvando..." : sucessoNoBotao ? "Salvo!" : "+ Adicionar"}
            </button>
          )}

        </div>
      </div>
    </div>
  );
}