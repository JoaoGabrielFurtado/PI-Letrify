export default function NotificacoesPage() {
  return (
    <div className="max-w-4xl mx-auto p-8 animate-fade-in">
      <h1 className="text-3xl font-black mb-4" style={{ color: 'var(--cor-texto-principal)' }}>
        Notificações 🔔
      </h1>
      <p className="mb-8" style={{ color: 'var(--cor-texto-secundario)' }}>
        Fique por dentro do que acontece na sua rede.
      </p>

      <div className="text-center py-20 opacity-50 border-2 border-dashed rounded-2xl" style={{ borderColor: 'var(--cor-fundo-sidebar)' }}>
        <span className="text-5xl block mb-4">🤫</span>
        <p className="font-bold text-xl" style={{ color: 'var(--cor-texto-principal)' }}>Tudo silêncio por aqui.</p>
        <p className="text-sm">Você não tem novas notificações no momento.</p>
      </div>
    </div>
  );
}