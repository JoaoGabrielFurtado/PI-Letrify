export default function AjudaPage() {
  return (
    <div className="max-w-4xl mx-auto p-8 animate-fade-in">
      <h1 className="text-3xl font-black mb-4" style={{ color: 'var(--cor-texto-principal)' }}>
        Central de Ajuda 🆘
      </h1>
      <p className="mb-8" style={{ color: 'var(--cor-texto-secundario)' }}>
        Precisa de uma mãozinha com o Letrify? Estamos aqui para ajudar.
      </p>

      <div className="grid gap-4">
        {["Como adicionar livros?", "Esqueci minha senha", "Privacidade dos dados"].map((item) => (
          <div 
            key={item}
            className="p-4 rounded-xl border border-dashed opacity-60"
            style={{ borderColor: 'var(--cor-fundo-sidebar)' }}
          >
            <span className="font-bold" style={{ color: 'var(--cor-texto-principal)' }}>{item}</span>
            <p className="text-xs mt-1">Conteúdo em breve...</p>
          </div>
        ))}
      </div>
    </div>
  );
}