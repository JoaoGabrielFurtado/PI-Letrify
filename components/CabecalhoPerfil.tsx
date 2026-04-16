"use client";

// ----------------------------------------------------------------------
// 1. O SKELETON DO CABEÇALHO (A sombra exata do componente real)
// ----------------------------------------------------------------------
export function SkeletonCabecalho() {
  return (
    <div className="animate-pulse relative mb-8">
      {/* Banner Skeleton */}
      <div className="h-56 w-full rounded-t-2xl bg-black/10 dark:bg-white/10"></div>

      {/* Corpo Skeleton */}
      <div className="px-8 pb-8 rounded-b-2xl shadow-sm border-x border-b" style={{ backgroundColor: 'var(--cor-fundo-card)', borderColor: 'var(--cor-fundo-sidebar)' }}>
        <div className="flex flex-col md:flex-row gap-6">
          
          {/* Esquerda: Avatar e Localização */}
          <div className="flex flex-col items-center md:items-start shrink-0 -mt-16 z-10">
            {/* Avatar Squircle Skeleton */}
            <div className="w-36 h-36 border-4 bg-black/15 dark:bg-white/15" style={{ borderColor: 'var(--cor-fundo-card)', borderRadius: '2rem' }}></div>
            {/* Localização Skeleton */}
            <div className="mt-4 h-4 w-24 bg-black/10 dark:bg-white/10 rounded-md"></div>
          </div>

          {/* Direita: Nome, Botão, Bio e Conexões */}
          <div className="flex-1 mt-4 md:mt-2 flex flex-col justify-between">
            {/* Nome e Botão */}
            <div className="flex justify-between items-start md:items-center">
              <div className="h-8 w-48 bg-black/15 dark:bg-white/15 rounded-md"></div>
              <div className="h-10 w-32 bg-black/10 dark:bg-white/10 rounded-xl"></div>
            </div>

            {/* Bio e Conexões*/}
            <div className="mt-4 flex flex-col md:flex-row justify-between items-start md:items-end gap-4">
              {/* Bio Skeleton (3 linhas falsas) */}
              <div className="space-y-2 w-full max-w-xl">
                <div className="h-4 w-full bg-black/10 dark:bg-white/10 rounded-md"></div>
                <div className="h-4 w-5/6 bg-black/10 dark:bg-white/10 rounded-md"></div>
                <div className="h-4 w-4/6 bg-black/10 dark:bg-white/10 rounded-md"></div>
              </div>

              {/* Conexões Skeleton */}
              <div className="h-5 w-40 bg-black/15 dark:bg-white/15 rounded-md shrink-0"></div>
            </div>
          </div>

        </div>
      </div>
    </div>
  );
}

// ----------------------------------------------------------------------
// O COMPONENTE REAL (só renderiza o que recebe)
// ----------------------------------------------------------------------

// Definindo o "contrato" do que este componente precisa para funcionar
interface CabecalhoProps {
  nome: string;
  cidade: string;
  descricao: string;
  fotoPerfil: string;
  bannerUrl: string;
  estatisticas: { seguindo: number; seguidores: number };
  isDonoDoPerfil: boolean;
}

export default function CabecalhoPerfil({
  nome, 
  cidade, 
  descricao, 
  fotoPerfil, 
  bannerUrl, 
  estatisticas = { seguindo: 0, seguidores: 0 }, 
  isDonoDoPerfil 
}: CabecalhoProps) {
  
  // A inicial do nome caso o usuário não tenha foto
  const inicial = nome ? nome.charAt(0).toUpperCase() : "U";

  return (
    <div className="animate-fade-in relative mb-8">
      
      {/* BANNER */}
      <div 
        className="h-56 w-full rounded-t-2xl bg-cover bg-center relative"
        style={{ backgroundImage: `url("${bannerUrl}")` }}
      >
        <div className="absolute inset-0 bg-gradient-to-t from-black/50 to-transparent rounded-t-2xl"></div>
      </div>

      {/* ÁREA DE INFORMAÇÕES */}
      <div 
        className="px-8 pb-8 rounded-b-2xl shadow-sm border-x border-b relative"
        style={{ backgroundColor: 'var(--cor-fundo-card)', borderColor: 'var(--cor-fundo-sidebar)' }}
      >
        <div className="flex flex-col md:flex-row gap-6">
          
          {/* COLUNA ESQUERDA: Avatar Squircle e Localização */}
          <div className="flex flex-col items-center md:items-start shrink-0 -mt-16 z-10">
            <div 
              className="w-36 h-36 flex items-center justify-center text-5xl font-bold shadow-xl border-4 object-cover overflow-hidden"
              style={{ backgroundColor: 'var(--cor-primaria)', color: 'var(--cor-botao-texto)', borderColor: 'var(--cor-fundo-card)', borderRadius: '2rem' }}
            >
              {fotoPerfil ? (
                <img src={fotoPerfil} alt={`Avatar de ${nome}`} className="w-full h-full object-cover" />
              ) : (
                inicial
              )}
            </div>
            
            {/* Localização condicional (só renderiza se a API trouxer a cidade) */}
            {cidade && (
              <div className="mt-3 flex items-center gap-1 font-semibold text-sm" style={{ color: 'var(--cor-texto-secundario)' }}>
                📍 {cidade}
              </div>
            )}
          </div>

          {/* COLUNA DIREITA: Nome, Botão, Bio e Conexões */}
          <div className="flex-1 mt-4 md:mt-2 flex flex-col justify-between">
            
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
              {/*Nome do Perfil*/}
              <h1 className="text-3xl font-black tracking-tight" style={{ color: 'var(--cor-texto-principal)' }}>
                {nome}
              </h1>
              
              {isDonoDoPerfil ? (
                <button 
                  className="px-5 py-2 text-sm font-bold rounded-xl shadow transition-transform hover:scale-105"
                  style={{ backgroundColor: 'var(--cor-fundo-app)', color: 'var(--cor-texto-principal)', border: '2px solid var(--cor-destaque)' }}
                >
                  Editar Perfil
                </button>
              ) : (
                <button 
                  className="px-8 py-2 text-sm font-bold rounded-xl shadow transition-transform hover:scale-105"
                  style={{ backgroundColor: 'var(--cor-botao-primario)', color: 'var(--cor-botao-texto)' }}
                >
                  Seguir
                </button>
              )}
            </div>

            {/* Alinhamento da Bio e Conexões*/}
            <div className="mt-4 flex flex-col gap-3">
              
              {/* Bio */}
              <div className="text-sm leading-relaxed max-w-2xl flex-1" style={{ color: 'var(--cor-texto-principal)' }}>
                {descricao ? descricao : <span className="italic opacity-60">Este usuário ainda não escreveu uma biografia.</span>}
              </div>

              {/* Conexões*/}
              <div className="flex gap-4 text-sm font-semibold shrink-0 mb-1">
                <span className="cursor-pointer hover:underline decoration-2" style={{ textDecorationColor: 'var(--cor-primaria)' }}>
                  <strong style={{ color: 'var(--cor-texto-principal)' }}>{estatisticas.seguindo}</strong> <span style={{ color: 'var(--cor-texto-secundario)' }}>Seguindo</span>
                </span>
                <span className="cursor-pointer hover:underline decoration-2" style={{ textDecorationColor: 'var(--cor-primaria)' }}>
                  <strong style={{ color: 'var(--cor-texto-principal)' }}>{estatisticas.seguidores}</strong> <span style={{ color: 'var(--cor-texto-secundario)' }}>Seguidores</span>
                </span>
              </div>

            </div>

          </div>

        </div>
      </div>
    </div>
  );
}