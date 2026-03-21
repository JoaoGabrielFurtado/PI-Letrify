import Sidebar from "../../components/Sidebar";

export default function ComSidebarLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <div className="flex min-h-screen" style={{ backgroundColor: 'var(--cor-fundo)' }}>
      {/* Aqui entra a nossa barra lateral fixa */}
      <Sidebar />
      
      {/* Aqui entra a página ativa (ex: o "Bem-vindo ao Letrify") */}
      <main className="flex-1 p-8">
        {children}
      </main>
    </div>
  );
}