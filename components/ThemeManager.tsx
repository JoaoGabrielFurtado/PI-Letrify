"use client";

import { useEffect } from "react";

<<<<<<< HEAD
/**
 * Componente invisível que roda no boot do App.
 * Ele lê o localStorage e aplica as variáveis CSS do tema salvo.
 */
export default function ThemeManager() {
  useEffect(() => {
    // Como estamos no navegador, garantimos que document existe
    if (typeof window === "undefined") return;

    // 1. Tenta buscar o tema salvo no navegador (Ex: { paleta: 1, escura: true })
=======
/*
 Componente invisível que roda no boot do App.
 Lê o localStorage e aplica as variáveis CSS do tema salvo.
*/
export default function ThemeManager() {
  useEffect(() => {
    // Garante que isso só rode no navegador, já que localStorage não existe no servidor
    if (typeof window === "undefined") return;

    // Tenta buscar o tema salvo no navegador (Ex: { paleta: 1, escura: true })
>>>>>>> front
    const temaSalvo = localStorage.getItem("letrify_theme");
    
    if (temaSalvo) {
      try {
        const { paleta, escura } = JSON.parse(temaSalvo);
        
<<<<<<< HEAD
        // 2. Aplica as classes e atributos no <html> (Root) do site
        // Supondo que vocês usem data-theme ou classes do Tailwind:
=======
        // Aplica as classes e atributos no <html> (Root) do site
        // Supondo que usem data-theme ou classes do Tailwind:
>>>>>>> front
        document.documentElement.setAttribute("data-theme-palette", String(paleta)); // 0, 1, 2
        
        if (escura) {
          document.documentElement.classList.add("dark");
        } else {
          document.documentElement.classList.remove("dark");
        }
        
        console.log(`🎨 Tema '${paleta}${escura ? '-dark':''}' carregado do cache do navegador.`);
      } catch (e) {
        // Se o JSON estiver quebrado, limpa e usa o padrão do CSS
        localStorage.removeItem("letrify_theme");
      }
    }
  }, []);

<<<<<<< HEAD
  return null; // Não desenha nada
=======
  return null;
>>>>>>> front
}