"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";

const AUTH_KEY = "letrify-auth";

// O "Segurança" do Feed
function hasValidSession() {
  if (typeof window === "undefined") return false;
  const raw = localStorage.getItem(AUTH_KEY);
  if (!raw) return false;
  try {
    const parsed = JSON.parse(raw) as { token: string; expiresAt: number };
    return Boolean(parsed.token && typeof parsed.expiresAt === "number" && Date.now() < parsed.expiresAt);
  } catch {
    return false;
  }
}

export default function Home() {
  const router = useRouter();
  const [autorizado, setAutorizado] = useState(false);

  useEffect(() => {
    if (!hasValidSession()) {
      router.replace("/login");
    } else {
      // Só libera a tela se o token estiver
      setAutorizado(true);
    }
  }, [router]);

  // Se ainda não verificou, não renderiza nada (evita o flicker)
  if (!autorizado) return null; 

  return (
    <div className="flex items-center justify-center h-full min-h-[80vh]">
      <div className="text-center">
        <h1 className="text-4xl font-bold" style={{ color: 'var(--cor-primaria)' }}>
          Bem-vindo ao Letrify! 📚
        </h1>
        <p className="mt-3 opacity-80">
          Você está logado com sessão ativa e pronto para navegar.
        </p>
      </div>
    </div>
  );
}