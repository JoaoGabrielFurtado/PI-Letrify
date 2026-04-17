// app/lib/jwt.ts

/**
 * Decodifica um token JWT e retorna o payload (os dados internos).
 * Usa apenas funções nativas do navegador (atob).
 */
export function decodificarToken(token: string) {
  try {
    // 1. O JWT é dividido em: HEADER.PAYLOAD.SIGNATURE
    // Nós queremos a parte do meio (index 1)
    const payloadBase64Url = token.split(".")[1];
    
    // 2. Corrige a formatação do Base64Url para Base64 padrão
    const payloadBase64 = payloadBase64Url.replace(/-/g, "+").replace(/_/g, "/");
    
    // 3. Decodifica o Base64 para string de texto
    const payloadJson = window.atob(payloadBase64);
    
    // 4. Converte a string de texto (JSON) para um objeto JavaScript
    return JSON.parse(payloadJson);
  } catch (erro) {
    console.error("Erro ao decodificar o token JWT. Token malformado?", erro);
    return null;
  }
}

/**
 * Tenta extrair o ID do usuário de dentro do token.
 * Como é .NET, o ID geralmente está numa chave complexa chamada 'nameid' ou 'sub'.
 */
export function extrairIdDoToken(token: string): string | null {
  const payload = decodificarToken(token);
  if (!payload) return null;

  // 🕵️‍♂️ MODO FOFOCA ATIVADO: Mostra no F12 o que tem dentro do Token!
  console.log("🕵️‍♂️ Payload do Token revelado:", payload);

  // Adicionei "id", "UserId" e "uid" na nossa rede de captura!
  const id = 
    payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] ||
    payload["nameid"] || 
    payload["sub"] ||
    payload["id"] || 
    payload["UserId"] ||
    payload["uid"];

  if (!id) {
    console.error("🚨 O script hacker não encontrou nenhuma chave de ID no token!");
  }

  return id ? String(id) : null;
}