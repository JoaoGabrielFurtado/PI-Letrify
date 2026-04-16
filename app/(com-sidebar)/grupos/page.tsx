export default function GruposPage() {
  return (
    <div className="max-w-4xl mx-auto p-4 pt-8 animate-fade-in">
      <div className="mb-10 pb-6 border-b" style={{ borderColor: 'var(--cor-fundo-sidebar)' }}>
        <h1 className="text-3xl font-black mb-2" style={{ color: 'var(--cor-texto-principal)' }}>
          Clubes e Grupos 💬
        </h1>
        <p style={{ color: 'var(--cor-texto-secundario)' }}>Participe de discussões e encontre leitores com os mesmos interesses.</p>
      </div>

      <div className="text-center py-20 opacity-70 border-2 border-dashed rounded-2xl" style={{ borderColor: 'var(--cor-fundo-sidebar)' }}>
        <span className="text-5xl block mb-4">⛺</span>
        <p className="font-bold text-xl" style={{ color: 'var(--cor-texto-principal)' }}>Área de Grupos em Construção.</p>
        <p className="text-sm" style={{ color: 'var(--cor-texto-secundario)' }}>O ponto de encontro da comunidade Letrify será inaugurado em breve.</p>
      </div>
    </div>
  );
}