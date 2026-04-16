"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { authService } from "@/app/lib/authService";

export default function ContaPage() {
  const router = useRouter();
  const [carregando, setCarregando] = useState(false);
  const [erro, setErro] = useState("");

  // FUNÇÃO DE SAIR (LOGOUT)
  const handleLogout = () => {
    authService.logout(); // Limpa o token do localStorage
    router.push("/login"); // Manda pro olho da rua (com carinho)
  };

  // FUNÇÃO DE EXCLUIR CONTA (Conectada à API Real!)
  const handleExcluirConta = async () => {
    // alert nativo do navegador para evitar acidentes
    const confirmar = window.confirm("Tem certeza absoluta? Esta ação apagará permanentemente a sua estante, resenhas e foto de perfil.");
    if (!confirmar) return;

    setCarregando(true);
    setErro("");

    try {
      const token = authService.getToken();
      if (!token) throw new Error("Você não está autenticado. Faça login novamente.");

      const resposta = await fetch("https://letrify.fly.dev/api/usuario/deletar", {
        method: "DELETE",
        headers: {
          "Authorization": `Bearer ${token}`
        }
      });

      if (!resposta.ok) {
        throw new Error("Não foi possível excluir a conta. Tente novamente.");
      }

      // Se a API excluiu com sucesso, a gente desloga o usuário e manda pra tela inicial
      authService.logout();
      router.push("/cadastro"); 

    } catch (err: any) {
      setErro(err.message);
      setCarregando(false);
    }
  };

  return (
    <div className="animate-fade-in space-y-10 max-w-4xl mx-auto pb-20">
      
      {/* CABEÇALHO DA PÁGINA */}
      <div>
        <h1 className="text-3xl font-black mb-2" style={{ color: 'var(--cor-texto-principal)' }}>Minha Conta</h1>
        <p className="text-sm" style={{ color: 'var(--cor-texto-secundario)' }}>
          Gerencie as suas credenciais de acesso e o status da sua conta no Letrify.
        </p>
      </div>

      {erro && (
        <div className="p-4 rounded-xl bg-red-500/10 border border-red-500 text-red-500 text-sm font-bold">
          {erro}
        </div>
      )}

      {/* BLOCO 1: DADOS DE ACESSO (Visual por enquanto, até a API ter rota de trocar senha) */}
      <section className="space-y-4">
        <h2 className="text-sm font-bold uppercase tracking-wider" style={{ color: 'var(--cor-primaria)' }}>Dados de Acesso</h2>
        
        <div className="p-6 rounded-2xl border flex flex-col gap-4" style={{ backgroundColor: 'var(--cor-fundo-card)', borderColor: 'var(--cor-fundo-sidebar)' }}>
          <p className="text-sm" style={{ color: 'var(--cor-texto-secundario)' }}>
            A alteração de e-mail e senha estará disponível em breve.
          </p>
          {/* Futuros inputs de trocar senha entrariam aqui */}
        </div>
      </section>

      {/* BLOCO 2: SESSÃO (BOTÃO DE SAIR) */}
      <section className="space-y-4">
        <h2 className="text-sm font-bold uppercase tracking-wider" style={{ color: 'var(--cor-primaria)' }}>Sessão</h2>
        
        <div className="p-6 rounded-2xl border flex items-center justify-between" style={{ backgroundColor: 'var(--cor-fundo-card)', borderColor: 'var(--cor-fundo-sidebar)' }}>
          <div className="flex flex-col">
            <span className="font-bold text-sm" style={{ color: 'var(--cor-texto-principal)' }}>Encerrar Sessão</span>
            <span className="text-xs" style={{ color: 'var(--cor-texto-secundario)' }}>Desconecta você deste dispositivo.</span>
          </div>
          
          <button 
            onClick={handleLogout}
            className="px-6 py-2 rounded-lg font-bold text-sm transition-colors hover:bg-black/10 dark:hover:bg-white/10"
            style={{ color: 'var(--cor-texto-principal)', border: '1px solid var(--cor-texto-secundario)' }}
          >
            Sair da Conta
          </button>
        </div>
      </section>

      {/* BLOCO 3: ZONA DE PERIGO (EXCLUIR CONTA) */}
      <section className="space-y-4 mt-12 pt-8 border-t border-dashed" style={{ borderColor: 'var(--cor-fundo-sidebar)' }}>
        <h2 className="text-sm font-bold uppercase tracking-wider text-red-500">Zona de Perigo</h2>
        
        <div className="p-6 rounded-2xl border border-red-500/30 bg-red-500/5 flex flex-col md:flex-row items-start md:items-center justify-between gap-4">
          <div className="flex flex-col max-w-lg">
            <span className="font-bold text-sm text-red-500">Excluir conta permanentemente</span>
            <span className="text-xs mt-1" style={{ color: 'var(--cor-texto-secundario)' }}>
              Ao excluir sua conta, você perderá acesso a todos os seus dados, livros salvos e resenhas. Esta ação é irreversível.
            </span>
          </div>
          
          <button 
            onClick={handleExcluirConta}
            disabled={carregando}
            className="px-6 py-2 rounded-lg font-bold text-sm bg-red-500 text-white transition-transform hover:scale-105 shadow-md disabled:opacity-50 flex-shrink-0"
          >
            {carregando ? "Excluindo..." : "Excluir Conta"}
          </button>
        </div>
      </section>

    </div>
  );
}