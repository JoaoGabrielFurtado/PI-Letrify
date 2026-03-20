'use client'; // Avisa ao Next.js que esta página terá interatividade de botões
import Link from 'next/link';

export default function Configuracoes() {
  
  // Função que troca o tema no HTML
  const mudarTema = (novoTema: string) => {
    document.documentElement.setAttribute('data-theme', novoTema);
  };

  return (
    <div className="min-h-screen flex flex-col items-center justify-center p-8">
      <div className="p-10 rounded-xl shadow-lg max-w-md w-full text-center" style={{ backgroundColor: 'var(--cor-fundo)', border: '1px solid var(--cor-primaria)' }}>
        <h1 className="text-3xl font-bold mb-8">⚙️ Temas do Letrify</h1>
        
        <div className="flex gap-4 justify-center mb-10">
          <button 
            onClick={() => mudarTema('laranja')}
            className="px-6 py-2 rounded-md font-bold text-white transition-transform hover:scale-105"
            style={{ backgroundColor: '#9333ea' }} // Cor fixa só pro botão
          >
            ☀️ Laranja
          </button>
          
          <button 
            onClick={() => mudarTema('escuro')}
            className="px-6 py-2 rounded-md font-bold text-white transition-transform hover:scale-105"
            style={{ backgroundColor: '#111827', border: '1px solid #c084fc' }} // Cor fixa só pro botão
          >
            🌙 Escuro
          </button>

          <button 
            onClick={() => mudarTema('verde')}
            className="px-6 py-2 rounded-md font-bold text-white transition-transform hover:scale-105"
            style={{ backgroundColor: '#9333ea' }} // Cor fixa só pro botão
          >
            🟢 Verde
          </button>

        </div>
        
        <Link 
          href="/" 
          className="px-6 py-3 rounded-md text-white font-medium transition-opacity hover:opacity-80"
          style={{ backgroundColor: 'var(--cor-primaria)' }}
        >
          Voltar para o Feed
        </Link>
      </div>
    </div>
  );
}