"use client";


interface Opcao {
  valor: string; // O que vai para o banco de dados (ex: "todos", "ninguem")
  rotulo: string; // O que o usuário lê na tela (ex: "Qualquer pessoa", "Ninguém")
}

interface CaixaSelecaoProps {
  titulo: string;
  descricao?: string;
  opcoes: Opcao[];
  valorSelecionado: string;
  aoMudar: (novoValor: string) => void;
  desativado?: boolean;
}

export default function CaixaSelecao({
  titulo,
  descricao,
  opcoes,
  valorSelecionado,
  aoMudar,
  desativado = false
}: CaixaSelecaoProps) {
  return (
    <div 
      className={`flex flex-col sm:flex-row sm:items-center justify-between p-4 rounded-xl border transition-all gap-4 ${
        desativado ? 'opacity-50 cursor-not-allowed grayscale' : 'hover:bg-black/5 dark:hover:bg-white/5'
      }`}
      style={{ 
        backgroundColor: 'var(--cor-fundo-card)', 
        borderColor: 'var(--cor-fundo-sidebar)' 
      }}
    >
      {/* Lado Esquerdo: Textos */}
      <div className="flex-1 pr-4">
        <h3 className="font-bold text-sm" style={{ color: 'var(--cor-texto-principal)' }}>
          {titulo}
        </h3>
        {descricao && (
          <p className="text-xs mt-1 leading-relaxed" style={{ color: 'var(--cor-texto-secundario)' }}>
            {descricao}
          </p>
        )}
      </div>

      {/* Lado Direito: A Caixa de Seleção */}
      <div className="shrink-0 w-full sm:w-auto">
        <select
          disabled={desativado}
          value={valorSelecionado}
          onChange={(e) => aoMudar(e.target.value)}
          className={`w-full sm:w-48 p-2 rounded-lg border text-sm font-semibold outline-none focus:ring-2 transition-shadow ${
            desativado ? 'cursor-not-allowed' : 'cursor-pointer'
          }`}
          style={{
            backgroundColor: 'var(--cor-fundo-app)',
            color: 'var(--cor-texto-principal)',
            borderColor: 'var(--cor-destaque)', // Usa a cor de destaque para a borda
          }}
        >
          {opcoes.map((opcao) => (
            <option key={opcao.valor} value={opcao.valor}>
              {opcao.rotulo}
            </option>
          ))}
        </select>
      </div>
    </div>
  );
}