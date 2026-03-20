// app/lib/usuarioService.ts

import { dadosFalsos } from "./mockDados";

export function obterPerfilSeguro(dadosApiReal: any, isDonoDoPerfil: boolean) {
// O FIREWALL AGORA LÊ O QUE VOCÊ CLICOU NA TELA DE PRIVACIDADE!
  let privacidade = dadosFalsos.privacidade;
  
  // Como estamos no Next.js, garantimos que o navegador existe antes de ler o localStorage
  if (typeof window !== 'undefined') {
    const salvos = localStorage.getItem("letrify-privacidade");
    if (salvos) privacidade = JSON.parse(salvos);
  }

  const perfilFalso = dadosFalsos.perfil;

  // 1. O BFF calcula as estatísticas da estante usando os dados reais da API!
  // Se não vier nada da API, nasce tudo zerado.
  const estanteCalculada = {
    lidos: dadosApiReal?.estante?.filter((l: any) => l.status === 'lido').length || 0,
    lendo: dadosApiReal?.estante?.filter((l: any) => l.status === 'lendo').length || 0,
    queroLer: dadosApiReal?.estante?.filter((l: any) => l.status === 'queroLer').length || 0,
  };

  // 2. Monta o Perfil Base
  let perfilSeguro = {
    nome: dadosApiReal?.nome || "Usuário",
    fotoPerfil: dadosApiReal?.fotoPerfil || "",
    cidade: dadosApiReal?.cidade || "",
    descricao: dadosApiReal?.descricao || "", // Bio vem da API!
    bannerUrl: perfilFalso.bannerUrl, // Banner ainda é falso
    
    // Nascem bloqueados/vazios por padrão
    estatisticas: { seguindo: 0, seguidores: 0 },
    // O 'as any' cala a boca do TypeScript para parar de dar erro no null
    estante: null as any, 
    resenhas: [],
    destaques: [] as any[],
    isPrivado: false 
  };

  // 3. Se for o dono, libera tudo
  if (isDonoDoPerfil) {
    perfilSeguro.estatisticas = perfilFalso.estatisticas;
    perfilSeguro.estante = estanteCalculada; // Usa o cálculo real!
    perfilSeguro.destaques = perfilFalso.destaques;
    return perfilSeguro;
  }

  // 4. Se for visitante, passa pela roleta russa da privacidade
  if (privacidade.contaPrivada) {
    perfilSeguro.isPrivado = true;
    perfilSeguro.descricao = "🔒 Este perfil é privado."; 
    return perfilSeguro; 
  }

  // Libera granularmente
  if (privacidade.mostrarConexoes) perfilSeguro.estatisticas = perfilFalso.estatisticas;
  if (privacidade.mostrarEstante) {
    perfilSeguro.estante = estanteCalculada; // Usa o cálculo real se a privacidade deixar!
    perfilSeguro.destaques = perfilFalso.destaques; 
  }

  return perfilSeguro;
}


/**
 * 2. PROTEÇÃO DE MENSAGENS: Valida se o botão "Enviar Mensagem" deve sequer existir
 */
export function podeEnviarMensagem(isDonoDoPerfil: boolean, isSeguidor: boolean): boolean {
  if (isDonoDoPerfil) return false; // Não pode mandar mensagem para si mesmo

  const regra = dadosFalsos.privacidade.quemPodeEnviarMensagem;

  if (regra === "todos") return true;
  if (regra === "seguidores" && isSeguidor) return true;
  if (regra === "ninguem") return false;

  return false;
}


/**
 * 3. PROTEÇÃO DE BUSCA: Remove o usuário da barra de pesquisa do site
 */
export function filtrarBuscaSegura(resultadosDaApi: any[]) {
  // Num cenário real, a API nem deveria trazer do banco. 
  // Como estamos no mock, filtramos aqui:
  const regraOcultar = dadosFalsos.privacidade.ocultarDasBuscas;
  
  if (regraOcultar) {
    return []; // Finge que não achou ninguém
  }
  
  return resultadosDaApi;
}