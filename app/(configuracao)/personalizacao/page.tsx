"use client";

import { useState, useEffect } from "react";

// O "banco de dados" de paletas
const paletasDeCores = [
  { 
    id: 0, 
    nome: "Clássico Verde", 
    corFundo: "#F5F1E8", 
    corPrimaria: "#2F855A" 
  },
  { 
    id: 1, 
    nome: "Aura Laranja", 
    corFundo: "#FFFaf0", 
    corPrimaria: "#DD6B20" 
  },
  { 
    id: 2, 
    nome: "Modo Escuro", 
    corFundo: "#121212", 
    corPrimaria: "#805AD5" 
  }
];

export default function PersonalizacaoPage() {
  // Estados corretos
  const [temaAtivo, setTemaAtivo] = useState(0);
  const [salvando, setSalvando] = useState(false);

  // Lê o cache ao abrir a tela para marcar a bolinha
  useEffect(() => {
    const temaSalvo = localStorage.getItem("letrify_theme");
    if (temaSalvo) {
      try {
        const { paleta } = JSON.parse(temaSalvo);
        setTemaAtivo(paleta);
      } catch (e) {
        console.error("Erro ao ler cache do tema", e);
      }
    }
  }, []);

  // A função que o usuário chama ao clicar na bolinha
  const handleMudarTema = (id: number) => {
    setTemaAtivo(id);
    
    // Define se é modo escuro baseado no ID 
    const isEscuro = id === 2;

    // Atualiza visualmente na hora (injetando no HTML)
    document.documentElement.setAttribute("data-theme-palette", String(id));
    if (isEscuro) {
      document.documentElement.classList.add("dark");
    } else {
      document.documentElement.classList.remove("dark");
    }
    
    // Salva no cache do navegador (Para o ThemeManager ler depois)
    localStorage.setItem("letrify_theme", JSON.stringify({
      paleta: id,
      escura: isEscuro
    }));

    // Efeito visual de salvamento
    setSalvando(true);
    setTimeout(() => setSalvando(false), 600);
  };

  return (
    <div className="max-w-4xl mx-auto pb-20 animate-fade-in space-y-10">
      
      <div>
        <h1 className="text-3xl font-black mb-2" style={{ color: 'var(--cor-texto-principal)' }}>Personalização</h1>
        <p className="text-sm" style={{ color: 'var(--cor-texto-secundario)' }}>
          Ajuste a aparência do Letrify para combinar com o seu estilo de leitura.
        </p>
      </div>

      {/* BLOCO DE PALETA DE CORES */}
      <section className="space-y-4">
        <div className="flex items-center justify-between">
          <h2 className="text-sm font-bold uppercase tracking-wider" style={{ color: 'var(--cor-primaria)' }}>Paleta de Cores</h2>
          {salvando && (
            <span className="text-xs font-bold animate-pulse" style={{ color: 'var(--cor-primaria)' }}>
              Salvando localmente...
            </span>
          )}
        </div>
        
        <div 
          className="p-8 rounded-2xl border"
          style={{ backgroundColor: 'var(--cor-fundo-card)', borderColor: 'var(--cor-fundo-sidebar)' }}
        >
          <div className="flex flex-wrap gap-10 items-center justify-center sm:justify-start">
            {paletasDeCores.map((paleta) => {
              const isAtivo = temaAtivo === paleta.id;

              return (
                <div key={paleta.id} className="flex flex-col items-center gap-4">
                  
                  {/* O Círculo Colorido (Botão) */}
                  <button
                    onClick={() => handleMudarTema(paleta.id)}
                    className={`w-24 h-24 rounded-full shadow-lg transition-all duration-300 overflow-hidden relative ${
                      isAtivo ? 'scale-110 ring-4 ring-offset-4' : 'hover:scale-105 hover:shadow-xl'
                    }`}
                    style={{ 
                      '--tw-ring-color': paleta.corPrimaria,
                      '--tw-ring-offset-color': 'var(--cor-fundo-app)',
                    } as React.CSSProperties}
                    aria-label={`Selecionar tema ${paleta.nome}`}
                  >
                    {/* Metade Esquerda (Cor de Fundo) */}
                    <div className="absolute inset-y-0 left-0 w-1/2" style={{ backgroundColor: paleta.corFundo }}></div>
                    
                    {/* Metade Direita (Cor Primária) */}
                    <div className="absolute inset-y-0 right-0 w-1/2" style={{ backgroundColor: paleta.corPrimaria }}></div>
                    
                    {/* Detalhe central vítreo */}
                    <div className="absolute inset-0 m-auto w-8 h-8 rounded-full bg-white/20 backdrop-blur-sm border border-white/40 shadow-inner"></div>
                  </button>
                  
                  {/* Nome do Tema */}
                  <span 
                    className={`text-sm font-bold transition-colors ${isAtivo ? 'opacity-100' : 'opacity-50'}`} 
                    style={{ color: isAtivo ? 'var(--cor-primaria)' : 'var(--cor-texto-principal)' }}
                  >
                    {paleta.nome}
                  </span>
                </div>
              );
            })}
          </div>
        </div>
      </section>

    </div>
  );
}