const BANNER_PADRAO = "https://images.unsplash.com/photo-1519681393784-d120267933ba?q=80&w=2070&auto=format&fit=crop";

export function mapearPerfilDaApi(dadosApi: any) {
  // 🕵️ ESPIÃO 1: O que chegou na porta do Mapeador?
  console.log("🕵️ MAPEADOR - O que recebi da API:", dadosApi);

  if (!dadosApi) {
    console.log("🕵️ MAPEADOR - dadosApi veio vazio! Abortando.");
    return null;
  }

  const { perfil, estatisticas, favorito } = dadosApi;

  // Função auxiliar para procurar dentro do Array de situações
  const extrairQuantidade = (lista: any[], nomeSituacao: string) => {
    if (!Array.isArray(lista)) return 0;
    const item = lista.find((s) => s.situacao === nomeSituacao);
    return item ? item.quantidade : 0;
  };

  const perfilMapeado = {
    // 1. Dados Básicos do Perfil
    nome: perfil?.nome || "Usuário",
    fotoPerfil: perfil?.foto || "",
    bannerUrl: BANNER_PADRAO, 
    cidade: dadosApi.cidade || "", 
    descricao: dadosApi.descricao || "",

    // 2. Estatísticas do Cabeçalho
    estatisticas: {
      seguidores: perfil?.seguidores || 0,
      seguindo: perfil?.seguindo || 0,
    },

    // 3. Convertendo o Array da API para o formato que a Lateral espera
    estanteResumo: {
      lidos: extrairQuantidade(estatisticas?.situacoes, 'Lido'),
      lendo: extrairQuantidade(estatisticas?.situacoes, 'Lendo'),
      queroLer: extrairQuantidade(estatisticas?.situacoes, 'Quero Ler'),
    },

    // 4. Convertendo 'autor' e 'quantidade' para 'nome' e 'valor' (Para a aba de Vitrines)
    topAutores: Array.isArray(estatisticas?.topAutores) 
      ? estatisticas.topAutores.map((a: any) => ({ nome: a.autor, valor: a.quantidade }))
      : [],

    // 5. Convertendo 'tema' e 'quantidade' para 'nome' e 'valor' (Para a aba de Vitrines)
    topTemas: Array.isArray(estatisticas?.topTemas)
      ? estatisticas.topTemas.map((t: any) => ({ nome: t.tema, valor: t.quantidade }))
      : [],

    // 6. Livro Favorito
    favorito: favorito ? {
      titulo: favorito.titulo,
      autor: favorito.autor
    } : null,

    // Variáveis que não vi na imagem, mantendo defaults seguros
    totalDeLivros: estatisticas?.totalDeLivros || 0,
    grupos: dadosApi.grupos || 0,
    guias: dadosApi.guias || 0,
    isPrivado: dadosApi.isPrivado || false
  };

  // 🕵️ ESPIÃO 2: Como o dado ficou depois de traduzido?
  console.log("🕵️ MAPEADOR - O que vou entregar para a Tela:", perfilMapeado);

  return perfilMapeado;
}