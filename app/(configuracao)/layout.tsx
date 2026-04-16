import Link from "next/link";

export default function ConfiguracoesLayout({ children }: { children: React.ReactNode }) {
  return (
    // h-screen prende a tela no tamanho exato do monitor
    <div className="flex h-screen overflow-hidden" style={{ backgroundColor: 'var(--cor-fundo-app)', color: 'var(--cor-texto-principal)' }}>
      
      {/* BARRA LATERAL FIXA */}
      <aside 
        className="w-64 border-r p-6 flex flex-col h-full"
        style={{ borderColor: 'var(--cor-fundo-sidebar)', backgroundColor: 'var(--cor-fundo-card)' }}
      >
        <h2 className="text-2xl font-bold mb-6" style={{ color: 'var(--cor-primaria)' }}>
          Configurações
        </h2>

        {/* O flex-1 aqui empurra o botão de voltar lá para baixo */}
        <nav className="flex flex-col gap-2 flex-1">
          <Link href="/conta" className="p-3 rounded-md hover:bg-black/5 dark:hover:bg-white/5 transition-colors font-medium">
            👤 Conta
          </Link>
          <Link href="/privacidade" className="p-3 rounded-md hover:bg-black/5 dark:hover:bg-white/5 transition-colors font-medium">
            🔒 Privacidade
          </Link>
          <Link href="/personalizacao" className="p-3 rounded-md hover:bg-black/5 dark:hover:bg-white/5 transition-colors font-medium">
            🎨 Personalização
          </Link>
          <Link href="/notificacoes" className="p-3 rounded-md hover:bg-black/5 dark:hover:bg-white/5 transition-colors font-medium">
            🔔 Notificações
          </Link>
          <Link href="/ajuda" className="p-3 rounded-md hover:bg-black/5 dark:hover:bg-white/5 transition-colors font-medium">
            ❓ Ajuda
          </Link>
        </nav>

        {/* BOTÃO DE VOLTAR - FIXO NO RODAPÉ DA SIDEBAR */}
        <div className="mt-auto pt-6 border-t" style={{ borderColor: 'var(--cor-fundo-sidebar)' }}>
          <Link href="/perfil" className="flex items-center gap-2 p-3 rounded-md hover:bg-black/5 dark:hover:bg-white/5 transition-colors text-sm font-bold">
            ⬅ Voltar
          </Link>
        </div>
      </aside>

      {/* ÁREA DE CONTEÚDO (A única parte que faz scroll!) */}
      <main className="flex-1 p-10 overflow-y-auto h-full">
        <div className="max-w-3xl mx-auto pb-20">
          {children}
        </div>
      </main>

    </div>
  );
}