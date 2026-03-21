"use client";

import { useEffect, useState } from "react";
import RadarAfinidade, { UsuarioMatch } from "@/components/RadarAfinidade";
import { authService } from "@/app/lib/authService";

export default function MatchPage() {
  const [matches, setMatches] = useState<UsuarioMatch[]>([]);
  const [carregando, setCarregando] = useState(false);
  const [erro, setErro] = useState<string | null>(null);
  const [buscou, setBuscou] = useState(false);

  // 1. RECUPERAR DO CACHE AO ABRIR
  useEffect(() => {
    const cache = sessionStorage.getItem("letrify-last-matches");
    if (cache) {
      setMatches(JSON.parse(cache));
      setBuscou(true);
    }
  }, []);

  const acionarRadar = async () => {
    setCarregando(true);
    setErro(null);
    setBuscou(true);

    try {
      const token = authService.getToken();
      if (!token) throw new Error("Logue para usar o Radar.");

      const resposta = await fetch("https://letrify.fly.dev/api/match", {
        method: "POST",
        headers: {
          "Authorization": `Bearer ${token}`,
          "Content-Type": "application/json"
        }
      });

      if (!resposta.ok) throw new Error(`Erro ${resposta.status} na API.`);

      const dadosApi = await resposta.json();
      
      if (dadosApi.usuariosParecidos && Array.isArray(dadosApi.usuariosParecidos)) {
        const leitoresMapeados = dadosApi.usuariosParecidos.map((item: any) => ({
          id: item.usuario?.id,
          nome: item.usuario?.nome,
          cidade: item.usuario?.cidade,
          fotoPerfil: item.usuario?.fotoPerfil
        }));

        setMatches(leitoresMapeados);
        
        // 2. SALVAR NO CACHE DE SESSÃO
        sessionStorage.setItem("letrify-last-matches", JSON.stringify(leitoresMapeados));

      } else {
        setMatches([]);
        sessionStorage.removeItem("letrify-last-matches");
      }

    } catch (err: any) {
      setErro(err.message);
    } finally {
      setCarregando(false);
    }
  };

  return (
    <div className="max-w-5xl mx-auto p-4 pt-8 animate-fade-in pb-20 space-y-8">
      
      {/* CABEÇALHO */}
      <div className="pb-6 border-b flex flex-col md:flex-row md:items-center justify-between gap-6" style={{ borderColor: 'var(--cor-fundo-sidebar)' }}>
        <div>
          <h1 className="text-3xl font-black mb-2 flex items-center gap-3" style={{ color: 'var(--cor-texto-principal)' }}>
            Radar de Afinidade
          </h1>
          <p style={{ color: 'var(--cor-texto-secundario)' }}>
            Encontre leitores com base nos seus gostos literários.
          </p>
        </div>

        <button 
          onClick={acionarRadar}
          disabled={carregando}
          className="px-8 py-4 rounded-xl font-bold shadow-lg transition-transform hover:scale-105 disabled:opacity-50 disabled:hover:scale-100 flex items-center justify-center gap-3"
          style={{ backgroundColor: 'var(--cor-botao-primario)', color: 'var(--cor-botao-texo)' }}
        >
          {carregando ? (
            <span className="animate-spin text-xl">🧭</span>
          ) : (
            <span className="animate-pulse text-xl">📡</span>
          )}
          {carregando ? "Buscando leitores..." : "Acionar Radar"}
        </button>
      </div>

      {/* TELA INICIAL */}
      {!buscou && !carregando && (
         <div className="text-center py-20 opacity-50 border-2 border-dashed rounded-2xl" style={{ borderColor: 'var(--cor-fundo-sidebar)' }}>
           <span className="text-5xl block mb-4">🌍</span>
           <p className="font-bold text-xl" style={{ color: 'var(--cor-texto-principal)' }}>O Radar está desligado.</p>
           <p className="text-sm" style={{ color: 'var(--cor-texto-secundario)' }}>Clique no botão acima para procurar pessoas compatíveis com a sua estante.</p>
         </div>
      )}

      {/* ERROS */}
      {erro && !carregando && (
        <div className="text-center p-8 text-red-500 bg-red-500/10 rounded-2xl border border-red-500/20 shadow-sm">
          <span className="text-4xl block mb-2">⚠️</span>
          <p className="font-bold">{erro}</p>
        </div>
      )}

      {/* RESULTADO VAZIO */}
      {buscou && !carregando && !erro && matches.length === 0 && (
        <div className="text-center py-20 opacity-70 border-2 border-dashed rounded-2xl" style={{ borderColor: 'var(--cor-fundo-sidebar)' }}>
          <span className="text-5xl block mb-4">🏜️</span>
          <p className="font-bold text-xl" style={{ color: 'var(--cor-texto-principal)' }}>Nenhum leitor encontrado hoje.</p>
          <p className="text-sm" style={{ color: 'var(--cor-texto-secundario)' }}>Parece que você tem gostos muito exclusivos! Tente mais tarde.</p>
        </div>
      )}

      {/* A GRADE DE MATCHES */}
      {!carregando && !erro && matches.length > 0 && (
        <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6 animate-fade-in">
          {matches.map((usuario, index) => (
            <RadarAfinidade key={usuario.id || index} usuario={usuario} />
          ))}
        </div>
      )}

    </div>
  );
}