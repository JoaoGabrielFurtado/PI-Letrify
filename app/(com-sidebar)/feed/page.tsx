"use client";

import { useEffect, useState } from "react";
import { decodificarToken } from "@/app/lib/jwt";
import { authService } from "@/app/lib/authService";

export default function FeedPage() {
  const [nomeUsuario, setNomeUsuario] = useState("Leitor");

  useEffect(() => {
    const token = authService.getToken();
    if (token) {
      const payload = decodificarToken(token);
      const nomeEncontrado = payload?.name || payload?.given_name || payload?.unique_name;
      if (nomeEncontrado) setNomeUsuario(nomeEncontrado);
    }
  }, []);

  return (
    <div className="max-w-4xl mx-auto p-4 pt-8 animate-fade-in">
      <div className="mb-10 pb-6 border-b" style={{ borderColor: 'var(--cor-fundo-sidebar)' }}>
        <h1 className="text-3xl font-black mb-2" style={{ color: 'var(--cor-texto-principal)' }}>
          Bem-vindo de volta, {nomeUsuario}! 👋
        </h1>
        <p style={{ color: 'var(--cor-texto-secundario)' }}>Aqui estão as atualizações da sua rede literária de hoje.</p>
      </div>

      <div className="text-center py-20 opacity-70 border-2 border-dashed rounded-2xl" style={{ borderColor: 'var(--cor-fundo-sidebar)' }}>
        <span className="text-5xl block mb-4">📰</span>
        <p className="font-bold text-xl" style={{ color: 'var(--cor-texto-principal)' }}>O seu Feed está sendo preparado.</p>
        <p className="text-sm" style={{ color: 'var(--cor-texto-secundario)' }}>Em breve, resenhas e atualizações dos seus amigos aparecerão aqui.</p>
      </div>
    </div>
  );
}