import Link from "next/link";

export interface UsuarioMatch {
  id: string | number;
  nome: string;
  cidade: string;
  fotoPerfil: string | null;
}

interface RadarAfinidadeProps {
  usuario: UsuarioMatch;
}

export default function RadarAfinidade({ usuario }: RadarAfinidadeProps) {
  return (
    <div 
      className="p-6 rounded-2xl border flex flex-col items-center text-center transition-transform hover:-translate-y-1 hover:shadow-xl bg-card-limpo group"
      style={{ borderColor: 'var(--cor-fundo-sidebar)' }}
    >
      {/* FOTO DE PERFIL */}
      <div 
        className="w-24 h-24 rounded-full border-4 shadow-md bg-cover bg-center overflow-hidden mb-4"
        style={{ 
          borderColor: 'var(--cor-primaria)', 
          backgroundImage: usuario.fotoPerfil ? `url(${usuario.fotoPerfil})` : 'none',
          backgroundColor: 'var(--cor-fundo-sidebar)'
        }}
      >
        {!usuario.fotoPerfil && (
          <div className="w-full h-full flex items-center justify-center">
            <span className="text-3xl opacity-50">👤</span>
          </div>
        )}
      </div>

      {/* DADOS DO USUÁRIO */}
      <h3 className="font-black text-lg line-clamp-1 w-full" style={{ color: 'var(--cor-texto-principal)' }} title={usuario.nome}>
        {usuario.nome}
      </h3>
      
      <p className="text-sm mt-1 font-medium mb-6" style={{ color: 'var(--cor-texto-secundario)' }}>
        📍 {usuario.cidade || "Localização oculta"}
      </p>

      {/* BOTÃO DE AÇÃO */}
      <Link 
        href={`/perfil?id=${usuario.id}`}
        className="w-full py-2 rounded-lg font-bold text-sm transition-opacity opacity-90 group-hover:opacity-100 shadow flex items-center justify-center gap-2"
        style={{ backgroundColor: 'var(--cor-botao-primario)', color: 'var(--cor-botao-texo)' }}
      >
        📖 Ver Perfil
      </Link>
    </div>
  );
}