// app/lib/mockDados.ts (Apenas a parte da privacidade)
export const dadosFalsos = {
  privacidade: {
    // Bloco: Mestre
    contaPrivada: false, 
    ocultarDasBuscas: false,
    
    // Bloco: O que mostrar no perfil?
    mostrarEstante: true,
    mostrarResenhas: true,
    mostrarConexoes: true,
    
    // Bloco: Interações e Contato
    quemPodeEnviarMensagem: "todos", // "todos", "seguidores", "ninguem"
    quemPodeConvidarGrupos: "seguidores" // "todos", "seguidores", "ninguem"
  },
 perfil: {
    bannerUrl: "https://images.unsplash.com/photo-1519681393784-d120267933ba?q=80&w=2070&auto=format&fit=crop",
    estatisticas: {
      seguindo: 120,
      seguidores: 45,
    },
    destaques: [
      {
        id: "d1",
        tipo: "leitura_atual",
        ordem: 1,
        dados: {
          titulo: "O Senhor dos Anéis",
          autor: "J.R.R. Tolkien",
          progresso: 33,
          capaUrl: "https://exemplo.com/capa-lotr.jpg"
        }
      }
    ]
  } 
};