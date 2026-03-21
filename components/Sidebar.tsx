import Link from 'next/link';

export default function Sidebar() {
  return (
    <aside className="w-24 border-r border-gray-300 dark:border-gray-800 sticky top-0 self-start flex flex-col items-center py-8 min-h-screen justify-between" style={{ backgroundColor: 'var(--cor-fundo-sidebar)' }}>
      
      {/* TOPO: Logo que leva pro Feed */}
      <Link href="/login" className="font-bold text-lg text-center hover:scale-105 transition-transform" style={{ backgroundColor: 'var(--cor-fundo-sidebar)', color: 'var(--cor-texto-sidebar)' }}>
        Letrify
      </Link>
      
      {/* MEIO: Navegação Principal */}
      <nav className="flex flex-col gap-8 text-2xl">
        <Link href="/feed" title="Meu Feed" className="hover:opacity-70 transition-opacity hover:-translate-y-1 transform duration-200" style={{ color: 'var(--cor-destaque)' }}>
          🏠
        </Link>
        <Link href="/grupos" title="Comunidade" className="hover:opacity-70 transition-opacity hover:-translate-y-1 transform duration-200" style={{ color: 'var(--cor-destaque)' }}>
          👥
        </Link>
        <Link href="/match" title="Match" className="hover:opacity-70 transition-opacity hover:-translate-y-1 transform duration-200" style={{ color: 'var(--cor-destaque)' }}>
          ❤️
        </Link>
        <Link href="/busca" title="Buscar Livros" className="hover:opacity-70 transition-opacity hover:-translate-y-1 transform duration-200" style={{ color: 'var(--cor-destaque)' }}>
          🔍
        </Link>
        <Link href="/estante" title="Estante" className="hover:opacity-70 transition-opacity hover:-translate-y-1 transform duration-200" style={{ color: 'var(--cor-destaque)' }}>
          📚
        </Link>
        <Link href="/perfil" title="Meu Perfil" className="hover:opacity-70 transition-opacity hover:-translate-y-1 transform duration-200" style={{ color: 'var(--cor-destaque)' }}>
          👤
        </Link>
      </nav>

      {/* FINAL: Configurações (Aponta direto pra Conta, que é a tela inicial!) */}
      <Link href="/conta" title="Configurações" className="text-2xl hover:rotate-90 transition-transform duration-300">
        ⚙️
      </Link>

    </aside>
  );
}