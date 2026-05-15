"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { authService } from "@/app/lib/authService";

interface UsuarioConexao {
  id: string;
  nome: string;
  foto: string;
  cidade?: string;
}

interface ModalConexoesProps {
  aberto: boolean;
  onClose: () => void;
  tipo: "seguidores" | "seguindo";
  usuarioId: string;
}

export default function ModalConexoes({ aberto, onClose, tipo, usuarioId }: ModalConexoesProps) {
  const router = useRouter();
  const [lista, setLista] = useState<UsuarioConexao[]>([]);
  const [carregando, setCarregando] = useState(false);

  useEffect(() => {
    if (aberto && usuarioId) {
      const buscarDados = async () => {
        setCarregando(true);
        try {
          const token = authService.getToken();
          const rota = tipo === "seguidores" 
            ? `/api/seguidores/seguidores/${usuarioId}` 
            : `/api/seguidores/seguindo/${usuarioId}`;

          const resposta = await fetch(`https://letrify.fly.dev${rota}`, {
            headers: { "Authorization": `Bearer ${token}` }
          });

          if (resposta.ok) {
            const dados = await resposta.json();
            setLista(Array.isArray(dados) ? dados : []);
          }
        } catch (err) {
          console.error("Erro ao buscar conexões:", err);
        } finally {
          setCarregando(false);
        }
      };

      buscarDados();
    }
  }, [aberto, tipo, usuarioId]);

  if (!aberto) return null;

  return (
    <div className="fixed inset-0 z-[100] flex items-center justify-center p-4 bg-black/80 backdrop-blur-sm animate-fade-in">
      <div className="w-full max-w-md bg-zinc-900 border border-white/10 rounded-3xl overflow-hidden shadow-2xl flex flex-col max-h-[70vh]">
        
        {/* Header */}
        <div className="flex items-center justify-between p-6 border-b border-white/5">
          <h3 className="text-xl font-black capitalize text-white">
            {tipo}
          </h3>
          <button 
            onClick={onClose}
            className="w-10 h-10 rounded-full bg-white/5 hover:bg-red-500/20 hover:text-red-500 text-white transition-all flex items-center justify-center"
          >
            ✕
          </button>
        </div>

        {/* Lista */}
        <div className="flex-1 overflow-y-auto p-4 space-y-2 custom-scrollbar">
          {carregando ? (
            <div className="py-10 text-center space-y-3">
              <div className="w-8 h-8 border-4 border-blue-500 border-t-transparent rounded-full animate-spin mx-auto"></div>
              <p className="text-xs opacity-50 text-white">Buscando leitores...</p>
            </div>
          ) : lista.length > 0 ? (
            lista.map((user) => (
              <div 
                key={user.id} 
                onClick={() => {
                  onClose();
                  router.push(`/perfil?id=${user.id}`);
                }}
                className="flex items-center gap-4 p-3 rounded-2xl hover:bg-white/5 cursor-pointer transition-all border border-transparent hover:border-white/5 group"
              >
                <img 
                  src={user.foto || "https://via.placeholder.com/100"} 
                  alt={user.nome} 
                  className="w-12 h-12 rounded-xl object-cover border-2 border-white/5 group-hover:border-blue-500/50 transition-all"
                />
                <div className="flex-1 min-w-0">
                  <p className="font-bold text-white truncate group-hover:text-blue-400 transition-colors">
                    {user.nome || "Usuário Anonimo"}
                  </p>
                  <p className="text-[11px] opacity-40 truncate text-white uppercase tracking-widest font-semibold">
                    {user.cidade || "Leitor Letrify"}
                  </p>
                </div>
                <div className="text-blue-500 opacity-0 group-hover:opacity-100 transition-opacity pr-2">
                  →
                </div>
              </div>
            ))
          ) : (
            <div className="py-10 text-center">
              <span className="text-4xl block mb-2 opacity-20">📖</span>
              <p className="text-sm opacity-30 italic text-white">Nenhum leitor encontrado.</p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}