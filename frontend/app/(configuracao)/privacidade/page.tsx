"use client";

import { useState, useEffect } from "react";
import Link from "next/link";
import Interruptor from "@/components/Interruptor";
import CaixaSelecao from "@/components/CaixaSelecao";

// Constante separada para organização
const OPCOES_INTERACAO = [
  { valor: "todos", rotulo: "Qualquer pessoa" },
  { valor: "seguidores", rotulo: "Apenas Seguidores" },
  { valor: "ninguem", rotulo: "Ninguém" }
];

export default function PrivacidadePage() {
  const [carregando, setCarregando] = useState(true);
  
  // ESTADOS (Sincronizados com o localStorage)
  const [contaPrivada, setContaPrivada] = useState(false);
  const [ocultarBuscas, setOcultarBuscas] = useState(false);
  const [mostrarEstante, setMostrarEstante] = useState(true);
  const [mostrarResenhas, setMostrarResenhas] = useState(true);
  const [mostrarConexoes, setMostrarConexoes] = useState(true);
  const [quemPodeMensagem, setQuemPodeMensagem] = useState("todos");
  const [quemPodeGrupo, setQuemPodeGrupo] = useState("seguidores");

  // Carrega do Banco Falso (localStorage) ao abrir
  useEffect(() => {
    const dadosSalvos = localStorage.getItem("letrify-privacidade");
    if (dadosSalvos) {
      const p = JSON.parse(dadosSalvos);
      setContaPrivada(p.contaPrivada ?? false);
      setOcultarBuscas(p.ocultarBuscas ?? false);
      setMostrarEstante(p.mostrarEstante ?? true);
      setMostrarResenhas(p.mostrarResenhas ?? true);
      setMostrarConexoes(p.mostrarConexoes ?? true);
      setQuemPodeMensagem(p.quemPodeMensagem ?? "todos");
      setQuemPodeGrupo(p.quemPodeGrupo ?? "seguidores");
    }
    setCarregando(false);
  }, []);

  // Salva no Banco Falso (localStorage) a cada clique
  const salvarPreferencia = (chave: string, valor: any) => {
    const novasPreferencias = {
      contaPrivada, ocultarBuscas, mostrarEstante, mostrarResenhas, 
      mostrarConexoes, quemPodeMensagem, quemPodeGrupo,
      [chave]: valor 
    };
    
    // --- SIMULADOR DE SALVAMENTO NO BANCO ---
    localStorage.setItem("letrify-privacidade", JSON.stringify(novasPreferencias));
    // ----------------------------------------
    
    if (chave === 'contaPrivada') setContaPrivada(valor);
    if (chave === 'ocultarBuscas') setOcultarBuscas(valor);
    if (chave === 'mostrarEstante') setMostrarEstante(valor);
    if (chave === 'mostrarResenhas') setMostrarResenhas(valor);
    if (chave === 'mostrarConexoes') setMostrarConexoes(valor);
    if (chave === 'quemPodeMensagem') setQuemPodeMensagem(valor);
    if (chave === 'quemPodeGrupo') setQuemPodeGrupo(valor);
  };

  if (carregando) return <div className="animate-pulse p-10 text-center opacity-50">🔒 Carregando os teus segredos...</div>;

  return (
    <div className="animate-fade-in space-y-10 max-w-4xl mx-auto pb-20">
      
      <div>
        <h1 className="text-3xl font-black mb-2" style={{ color: 'var(--cor-texto-principal)' }}>Privacidade</h1>
        <p className="text-sm" style={{ color: 'var(--cor-texto-secundario)' }}>
          Controle quem pode ver o seu perfil e interagir com você no Letrify.
        </p>
      </div>

      {/* BLOCO 1: VISIBILIDADE GLOBAL */}
      <section className="space-y-4">
        <h2 className="text-sm font-bold uppercase tracking-wider" style={{ color: 'var(--cor-primaria)' }}>Visibilidade Global</h2>
        
        <Interruptor 
          titulo="Conta Privada"
          descricao="Apenas os seguidores aprovados poderão ver as suas resenhas, guias e estante."
          ativo={contaPrivada}
          aoAlternar={() => salvarPreferencia('contaPrivada', !contaPrivada)}
        />

        <Interruptor 
          titulo="Ocultar das buscas"
          descricao="O seu perfil não aparecerá quando outros utilizadores pesquisarem pelo seu nome."
          ativo={ocultarBuscas}
          aoAlternar={() => salvarPreferencia('ocultarBuscas', !ocultarBuscas)}
        />
      </section>

      {/* BLOCO 2: O QUE MOSTRAR NO PERFIL (A MÁGICA DO CADEADO ESTÁ AQUI) */}
      {/* O relative aqui é crucial para o cadeado se basear NESTA seção inteira! */}
      <section className="space-y-4">
        
        <h2 className="text-sm font-bold uppercase tracking-wider" style={{ color: 'var(--cor-primaria)' }}>O que mostrar no Perfil?</h2>
        <div className="relative flex flex-col gap-4 w-full">
        <p></p>
        {/* O CADEADO GIGANTE E CORRIGIDO */}
        
          {contaPrivada && (
            <div 
             className="absolute inset-0 z-10 flex flex-col items-center justify-center rounded-2xl backdrop-blur-[2px] mt-0 border-2 border-dashed gap-3 p-10" 
             style={{ 
                backgroundColor: 'rgba(var(--cor-fundo-app-rgb), 0.7)', // Fundo translúcido do próprio tema
               borderColor: 'var(--cor-texto-secundario)',
               cursor: 'not-allowed' // Sinal de proibido no cadeado inteiro!
              }}
            >
              <span className="text-4xl">🔒</span>
              <div className="bg-white dark:bg-black px-6 py-3 rounded-xl shadow-lg flex flex-col items-center text-center gap-2">
               <span className="font-extrabold text-sm" style={{ color: 'var(--cor-texto-principal)' }}>A sua conta já é privada.</span>
               <span className="text-xs max-w-xs" style={{ color: 'var(--cor-texto-secundario)' }}>Todas as opções abaixo estão ocultas para visitantes por padrão.</span>
              </div>
            </div>
          )}

          {/* Os Legos agora com as descrições de volta */}
         <Interruptor 
            titulo="Mostrar Estante de Livros"
            descricao="Permite que qualquer pessoa veja os livros que leu ou quer ler."
            ativo={mostrarEstante}
            aoAlternar={() => salvarPreferencia('mostrarEstante', !mostrarEstante)}
            desativado={contaPrivada} 
          />
          <Interruptor 
            titulo="Mostrar Resenhas e Guias"
            descricao="Exibe as avaliações e guias que fez aos livros."
            ativo={mostrarResenhas}
            aoAlternar={() => salvarPreferencia('mostrarResenhas', !mostrarResenhas)}
            desativado={contaPrivada}
          />
          <Interruptor 
            titulo="Mostrar Seguidores/Seguindo"
            descricao="Exibe os números e a lista das tuas ligações."
            ativo={mostrarConexoes}
            aoAlternar={() => salvarPreferencia('mostrarConexoes', !mostrarConexoes)}
            desativado={contaPrivada}
          />
          <p></p>
        </div>
      </section>

      {/* BLOCO 3: INTERAÇÕES E CONTATO */}
      <section className="space-y-4">
        <h2 className="text-sm font-bold uppercase tracking-wider" style={{ color: 'var(--cor-primaria)' }}>Interações e Contato</h2>
        
        <CaixaSelecao 
          titulo="Quem pode enviar Mensagens Diretas?"
          descricao="Defina quem pode iniciar uma conversa privada consigo."
          opcoes={OPCOES_INTERACAO}
          valorSelecionado={quemPodeMensagem}
          aoMudar={(valor) => salvarPreferencia('quemPodeMensagem', valor)}
        />

        <CaixaSelecao 
          titulo="Quem pode te convidar para Grupos/Clubes?"
          descricao="Defina quem pode enviar convites para comunidades."
          opcoes={OPCOES_INTERACAO}
          valorSelecionado={quemPodeGrupo}
          aoMudar={(valor) => salvarPreferencia('quemPodeGrupo', valor)}
        />
        
      </section>
    
      {/* BOTÃO FLUTUANTE DE VISITANTE */}
      <Link 
        href="/perfil?preview=visitante"
        className="fixed bottom-8 right-8 z-50 px-6 py-3 rounded-full font-bold shadow-lg hover:scale-105 transition-transform"
        style={{ backgroundColor: 'var(--cor-destaque)', color: 'var(--cor-botao-texto)' }}
      >
        👁️ Ver como Visitante
      </Link>

    </div>
  );
}