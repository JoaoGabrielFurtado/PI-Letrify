// app/lib/usuarioService.ts

// O nosso "Plano B" para quando a API não mandar o banner
const BANNER_PADRAO = "https://images.unsplash.com/photo-1519681393784-d120267933ba?q=80&w=2070&auto=format&fit=crop";

/**
 * MAPEADOR DA API: 
 * Recebe o JSON cru da API e garante que o Front-end não quebre se faltar algo.
 */
export function mapearPerfilDaApi(dadosApi: any) {
  
  // Se a API falhar miseravelmente e devolver null/undefined, a gente devolve um esqueleto zerado
  if (!dadosApi) return null;

  return {
    id: dadosApi.id || 0,
    nome: dadosApi.nome || "Usuário Desconhecido",
    fotoPerfil: dadosApi.fotoPerfil || "",
    cidade: dadosApi.cidade || "",
    descricao: dadosApi.descricao || "",
    bannerUrl: dadosApi.bannerUrl || BANNER_PADRAO,
    
    // ESTATÍSTICAS: O Back-end que calcule. Se não vier, é 0.
    estatisticas: {
      seguindo: dadosApi.estatisticas?.seguindo || 0,
      seguidores: dadosApi.estatisticas?.seguidores || 0,
    },
    
    // ESTANTE: O Back-end tem que mandar o resumo numérico.
    estanteResumo: {
      lidos: dadosApi.estanteResumo?.lidos || 0,
      lendo: dadosApi.estanteResumo?.lendo || 0,
      queroLer: dadosApi.estanteResumo?.queroLer || 0,
    },
    
    // GRUPOS E GUIAS: O Back-end que conte.
    grupos: dadosApi.grupos || 0,
    guias: dadosApi.guias || 0,

    // PRIVACIDADE: É o Back-end que sabe se a conta é privada. O front só obedece a flag.
    isPrivado: dadosApi.isPrivado || false
  };
}