"use client";

interface InterruptorProps {
  titulo: string;
  descricao?: string; 
  ativo: boolean;
  aoAlternar: () => void;
  desativado?: boolean;
}

export default function Interruptor({
  titulo,
  descricao,
  ativo,
  aoAlternar,
  desativado = false
}: InterruptorProps) {
  return (
    <div 
      className={`flex items-center justify-between p-5 rounded-xl border transition-all gap-4 ${
        desativado ? 'opacity-50 cursor-not-allowed grayscale' : 'hover:bg-black/5 dark:hover:bg-white/5'
      }`}
      style={{ 
        backgroundColor: 'var(--cor-fundo-card)', 
        borderColor: 'var(--cor-fundo-sidebar)',

        cursor: desativado ? 'not-allowed' : 'pointer'
      }}
      // Se tiver desativado, o clique na box inteira não faz nada
      onClick={!desativado ? aoAlternar : undefined}
    >
      {/* Lado Esquerdo: Textos (Empilhados) */}
      <div className="flex flex-col flex-1 pr-4">
        <h3 className="font-bold text-sm" style={{ color: 'var(--cor-texto-principal)' }}>
          {titulo}
        </h3>
        {/* Renderiza a descrição APENAS se ela existir */}
        {descricao && (
          <p className="text-xs mt-1 leading-relaxed max-w-xl" style={{ color: 'var(--cor-texto-secundario)' }}>
            {descricao}
          </p>
        )}
      </div>

      {/* Lado Direito: O Botão visual */}
      <button
        type="button"
        role="switch"
        aria-checked={ativo}
        disabled={desativado}
        className={`relative inline-flex h-7 w-12 shrink-0 rounded-full border-2 border-transparent transition-colors duration-200 ease-in-out ${
          desativado ? 'cursor-not-allowed' : 'cursor-pointer'
        }`}
        style={{
          // Se desativado, fica cinza. Se ativo, cor do tema. Se inativo, cinza escuro.
          backgroundColor: desativado ? '#D1D5DB' : (ativo ? 'var(--cor-botao-primario)' : '#9CA3AF') 
        }}
      >
        <span className="sr-only">Alternar {titulo}</span>
        
        {/* A bolinha */}
        <span
          className={`pointer-events-none inline-block h-6 w-6 transform rounded-full bg-white shadow-md transition duration-200 ease-in-out ${
            ativo ? 'translate-x-5' : 'translate-x-0'
          }`}
        />
      </button>
    </div>
  );
}