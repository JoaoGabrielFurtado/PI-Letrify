// app/lib/usuarioService.ts

// Banner padrão para perfis sem banner
const BANNER_PADRAO = "https://images.unsplash.com/photo-1519681393784-d120267933ba?q=80&w=2070&auto=format&fit=crop";

/*
 MAPEADOR DA API: 
 Recebe o JSON cru da API e garante que o Front-end não quebre se faltar algo.
*/
export function mapearPerfilDaApi(dadosApi: any) {
  
  // Se a API falhar e devolver null/undefined, devolve um esqueleto zerado
  if (!dadosApi) return null;

  return {
    id: dadosApi.id || 0,
    nome: dadosApi.nome || "Usuário Desconhecido",
    fotoPerfil: dadosApi.fotoPerfil || "",
    cidade: dadosApi.cidade || "",
    descricao: dadosApi.descricao || "",
    bannerUrl: dadosApi.bannerUrl || BANNER_PADRAO,
    
    estatisticas: {
      seguindo: dadosApi.estatisticas?.seguindo || 0,
      seguidores: dadosApi.estatisticas?.seguidores || 0,
    },
    
    estanteResumo: {
      lidos: dadosApi.estanteResumo?.lidos || 0,
      lendo: dadosApi.estanteResumo?.lendo || 0,
      queroLer: dadosApi.estanteResumo?.queroLer || 0,
    },
    
    grupos: dadosApi.grupos || 0,
    guias: dadosApi.guias || 0,

    isPrivado: dadosApi.isPrivado || false
  };
}