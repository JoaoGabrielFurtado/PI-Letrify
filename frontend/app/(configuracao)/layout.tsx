import Link from "next/link";

export default function ConfiguracoesLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="min-h-screen flex" style={{ backgroundColor: 'var(--cor-fundo-app)', color: 'var(--cor-texto-principal)' }}>
      
      {/* BARRA LATERAL DE CONFIGURAÇÕES */}
      <aside 
        className="w-64 border-r p-6 flex flex-col gap-2"
        style={{ borderColor: 'var(--cor-fundo-sidebar)', backgroundColor: 'var(--cor-fundo-card)' }}
      >
        <h2 className="text-2xl font-bold mb-6" style={{ color: 'var(--cor-primaria)' }}>
          Configurações
        </h2>

       <nav className="flex flex-col gap-2">
          {/* Note que agora os links vão direto para /conta, /privacidade, etc */}
          <Link href="/conta" className="p-3 rounded-md hover:bg-gray-500/10 transition-colors font-medium">
            👤 Conta
          </Link>
          <Link href="/privacidade" className="p-3 rounded-md hover:bg-gray-500/10 transition-colors font-medium">
            🔒 Privacidade
          </Link>
          <Link href="/personalizacao" className="p-3 rounded-md hover:bg-gray-500/10 transition-colors font-medium">
            🎨 Personalização
          </Link>
          <Link href="/notificacoes" className="p-3 rounded-md hover:bg-gray-500/10 transition-colors font-medium">
            🔔 Notificações
          </Link>
          <Link href="/ajuda" className="p-3 rounded-md hover:bg-gray-500/10 transition-colors font-medium">
            ❓ Ajuda
          </Link>
        </nav>

        {/* Botão de voltar pro app principal lá embaixo */}
        <div className="mt-auto pt-6 border-t" style={{ borderColor: 'var(--cor-fundo-sidebar)' }}>
          <Link href="/" className="flex items-center gap-2 p-3 rounded-md hover:bg-gray-500/10 transition-colors text-sm font-bold">
            ⬅ Voltar para o Letrify
          </Link>
        </div>
      </aside>

      {/* ÁREA DE CONTEÚDO DINÂMICO (Onde as páginas vão renderizar) */}
      <main className="flex-1 p-10 overflow-y-auto">
        <div className="max-w-3xl mx-auto">
          {children}
        </div>
      </main>

    </div>
  );
}