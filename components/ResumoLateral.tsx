"use client";
import Link from "next/link";

interface ResumoLateralProps {
  estante: { lidos: number; lendo: number; queroLer: number } | null;
  totalGrupos: number;
  totalGuias: number;
}

export default function ResumoLateral({ estante, totalGrupos, totalGuias }: ResumoLateralProps) {
  return (
    <div className="flex flex-col gap-4">
      
      {/* CAIXA 1: ESTANTE (Só aparece se o Firewall não enviou null) */}
      {estante && (
        <div 
          className="p-5 rounded-2xl border transition-colors hover:bg-black/5 dark:hover:bg-white/5 cursor-pointer"
          style={{ backgroundColor: 'var(--cor-fundo-card)', borderColor: 'var(--cor-fundo-sidebar)' }}
        >
          <Link href="/estante" className="flex items-center justify-between mb-3 hover:opacity-70 transition-opacity">
            <h3 className="font-bold text-sm uppercase tracking-wider" style={{ color: 'var(--cor-primaria)' }}>Estante</h3>
            <span className="text-lg opacity-50">📚</span>
          </Link>
          
          <div className="flex flex-col gap-2 text-sm font-semibold">
            <Link href="/estante?filtro=Lendo" className="flex justify-between items-center hover:translate-x-1 transition-transform">
              <span style={{ color: 'var(--cor-texto-secundario)' }}>Lendo</span>
              <span style={{ color: 'var(--cor-texto-principal)' }}>{estante.lendo}</span>
            </Link>
            <Link href="/estante?filtro=Lido" className="flex justify-between items-center hover:translate-x-1 transition-transform">
              <span style={{ color: 'var(--cor-texto-secundario)' }}>Lidos</span>
              <span style={{ color: 'var(--cor-texto-principal)' }}>{estante.lidos}</span>
            </Link>
            <Link href="/estante?filtro=Quero Ler" className="flex justify-between items-center hover:translate-x-1 transition-transform">
              <span style={{ color: 'var(--cor-texto-secundario)' }}>Quero Ler</span>
              <span style={{ color: 'var(--cor-texto-principal)' }}>{estante.queroLer}</span>
            </Link>
          </div>
        </div>
      )}

      {/* CAIXA 2: GRUPOS */}
      <div 
        className="p-5 rounded-2xl border flex items-center justify-between transition-colors hover:bg-black/5 dark:hover:bg-white/5 cursor-pointer"
        style={{ backgroundColor: 'var(--cor-fundo-card)', borderColor: 'var(--cor-fundo-sidebar)' }}
      >
        <span className="font-bold text-sm" style={{ color: 'var(--cor-texto-principal)' }}>Grupos</span>
        <span className="text-xs font-extrabold px-2 py-1 rounded-md" style={{ backgroundColor: 'var(--cor-fundo-sidebar)', color: 'var(--cor-texto-secundario)' }}>
          {totalGrupos}
        </span>
      </div>

      {/* CAIXA 3: GUIAS */}
      <div 
        className="p-5 rounded-2xl border flex items-center justify-between transition-colors hover:bg-black/5 dark:hover:bg-white/5 cursor-pointer"
        style={{ backgroundColor: 'var(--cor-fundo-card)', borderColor: 'var(--cor-fundo-sidebar)' }}
      >
        <span className="font-bold text-sm" style={{ color: 'var(--cor-texto-principal)' }}>Guias Salvos</span>
        <span className="text-xs font-extrabold px-2 py-1 rounded-md" style={{ backgroundColor: 'var(--cor-fundo-sidebar)', color: 'var(--cor-texto-secundario)' }}>
          {totalGuias}
        </span>
      </div>

    </div>
  );
}