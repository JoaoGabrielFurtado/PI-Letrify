# 📌 Para desenvolver
- Raiz de Páginas: `app/*` 
- Adicionar componente: `components/*`

## Estrutura Atual
- Pastas com () = Não são usadas para URL, podem ter um layout.tsx que as pastas internas herdam.
- Pastas com page.tsx = São as que darão nome a URL, e abrigam em si o arquivo que comporta os childrens.
- page.tsx = Arquivos de montagem, normalmente importam pedaços de `components/*` para se integrar
- Pasta `components/*` = Uma pasta para abrigar arquivos de códigos que serão reutilizados, ou extensos.
- Pasta `app/lib/*` = Abriga arquivos de função, como descobrir o id, armazenar tolken, entre outras.

*Regras:* 
### 1º Construa na `components/*` arquivos e depois os importes como childrens em arquivos de layout.tsx e page.tsx sempre que o código da página for muito extensou ou ser reutilizavel.
### 2º Arquivos de função começam minusculo e tem maiscula no meio, exemplo: `usuario.ts`. Arquivos de modulares começam maisculo, exemplo: `EstanteUsuario`.