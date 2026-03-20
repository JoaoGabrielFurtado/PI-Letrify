"use client";

import { useState, useEffect } from "react";
import { dadosFalsos } from "@/app/lib/mockDados";

// O nosso "banco de dados" de paletas
const paletasDeCores = [
  { 
    id: 0, 
    nome: "Clássico Verde", 
    valor: "verde", 
    corFundo: "#F5F1E8", 
    corPrimaria: "#2F855A" 
  },
  { 
    id: 1, 
    nome: "Aura Laranja", 
    valor: "laranja", 
    corFundo: "#FFFaf0", 
    corPrimaria: "#DD6B20" 
  },
  { 
    id: 2, 
    nome: "Modo Escuro", 
    valor: "escuro", 
    corFundo: "#121212", 
    corPrimaria: "#805AD5" 
  }
];

export default function PersonalizacaoPage() {
  // Estado para controlar qual bolinha está "ativa"
  const [temaAtivo, setTemaAtivo] = useState<number>(dadosFalsos.temaPadrao);
  const [salvando, setSalvando] = useState(false);

  // Quando a tela carrega, tentamos ler se o usuário já tinha salvo um tema no navegador
  useEffect(() => {
    const temaSalvo = localStorage.getItem("letrify-tema");
    if (temaSalvo) {
      const paletaEncontrada = paletasDeCores.find(p => p.valor === temaSalvo);
      if (paletaEncontrada) {
        setTemaAtivo(paletaEncontrada.id);
        document.documentElement.setAttribute('data-theme', paletaEncontrada.valor);
      }
    }
  }, []);

  // A função que o usuário chama ao clicar na bolinha
  const handleMudarTema = (id: number, valorTema: string) => {
    setTemaAtivo(id);
    
    // Muda a cor do HTML na mesma hora
    document.documentElement.setAttribute('data-theme', valorTema);
    
    // Salva a escolha no navegador
    localStorage.setItem("letrify-tema", valorTema);

    // Simula o tempo de salvar isso na API (para o nosso Mock futuro)
    setSalvando(true);
    setTimeout(() => {
      setSalvando(false);
    }, 600);
  };

  return (
    <div className="max-w-3xl animate-fade-in">
      <h1 className="text-3xl font-bold mb-2" style={{ color: 'var(--cor-texto-principal)' }}>Personalização</h1>
      <p className="mb-8" style={{ color: 'var(--cor-texto-secundario)' }}>
        Ajuste a aparência do Letrify para combinar com o seu estilo de leitura.
      </p>

      {/* BLOCO DE PALETA DE CORES */}
      <div 
        className="p-8 rounded-xl border shadow-sm mb-8"
        style={{ backgroundColor: 'var(--cor-fundo-card)', borderColor: 'var(--cor-fundo-sidebar)' }}
      >
        <div className="flex items-center justify-between mb-6">
          <h2 className="text-xl font-bold" style={{ color: 'var(--cor-texto-principal)' }}>Paleta de Cores</h2>
          {salvando && (
            <span className="text-sm font-semibold animate-pulse" style={{ color: 'var(--cor-primaria)' }}>
              Salvando preferências...
            </span>
          )}
        </div>
        
        <div className="flex flex-wrap gap-8 items-center justify-center sm:justify-start">
          {paletasDeCores.map((paleta) => {
            const isAtivo = temaAtivo === paleta.id;

            return (
              <div key={paleta.id} className="flex flex-col items-center gap-3">
                {/* O Círculo Colorido */}
                <button
                  onClick={() => handleMudarTema(paleta.id, paleta.valor)}
                  className={`w-20 h-20 rounded-full shadow-md transition-transform duration-300 overflow-hidden relative ${
                    isAtivo ? 'scale-110 ring-4 ring-offset-4' : 'hover:scale-105 hover:shadow-lg'
                  }`}
                  style={{ 
                    // A cor do anel de destaque será a cor primária do próprio tema
                    '--tw-ring-color': paleta.corPrimaria,
                    // O offset puxa a cor de fundo atual do site para dar o contraste correto
                    '--tw-ring-offset-color': 'var(--cor-fundo-app)',
                  } as React.CSSProperties}
                  aria-label={`Selecionar tema ${paleta.nome}`}
                >
                  {/* Metade Esquerda (Cor de Fundo) */}
                  <div className="absolute inset-y-0 left-0 w-1/2" style={{ backgroundColor: paleta.corFundo }}></div>
                  
                  {/* Metade Direita (Cor Primária) */}
                  <div className="absolute inset-y-0 right-0 w-1/2" style={{ backgroundColor: paleta.corPrimaria }}></div>
                  
                  {/* Um detalhe central (Opcional, imitando um botão) */}
                  <div className="absolute inset-0 m-auto w-6 h-6 rounded-full bg-white/20 backdrop-blur-sm border border-white/40"></div>
                </button>
                
                {/* Nome do Tema */}
                <span 
                  className={`text-sm font-semibold transition-colors ${isAtivo ? 'opacity-100' : 'opacity-60'}`} 
                  style={{ color: isAtivo ? 'var(--cor-primaria)' : 'var(--cor-texto-secundario)' }}
                >
                  {paleta.nome}
                </span>
              </div>
            );
          })}
        </div>
      </div>

    </div>
  );
}