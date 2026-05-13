"use client";
import { useState, useRef } from "react";

interface EditorPerfilProps {
  dadosIniciais: any;
  onClose: () => void;
  onSave: (novosDados: any) => void;
}

export default function EditorPerfil({ dadosIniciais, onClose, onSave }: EditorPerfilProps) {
  const [formData, setFormData] = useState(dadosIniciais);

  // Refs para disparar o clique nos inputs de arquivo escondidos
  const fileInputRef = useRef<HTMLInputElement>(null);
  const bannerInputRef = useRef<HTMLInputElement>(null);

  // Limpeza de memória: URLs criadas com createObjectURL precisam ser revogadas
  // para evitar lentidão no navegador após muitos uploads de teste
  const getPreview = (field: any) => {
    if (!field) return "";
    if (field instanceof File) return URL.createObjectURL(field);
    return field;
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData((prev: any) => ({ ...prev, [name]: value }));
  };

  // Para capturar o arquivo de imagem
  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>, field: string) => {
    if (e.target.files && e.target.files[0]) {
      const arquivo = e.target.files[0];
    
      setFormData((prev: any) => ({ 
        ...prev, 
        [field]: arquivo // Agora ele usa o nome dinâmico (fotoPerfil ou bannerUrl)
      }));
    }
  };

  return (

      <div className="w-full animate-fade-in border rounded-3xl overflow-hidden" 
         style={{ backgroundColor: 'var(--cor-fundo-card)', borderColor: 'var(--cor-fundo-sidebar)' }}>
      
      {/* Seção do Banner Editável */}
      <div className="relative h-48 bg-zinc-800 group cursor-pointer" onClick={() => bannerInputRef.current?.click()}>
        <img 
          src={getPreview(formData.bannerUrl)} 
          className="w-full h-full object-cover opacity-60 group-hover:opacity-40 transition-opacity" 
          alt="Banner"
        />
        <div className="absolute inset-0 flex items-center justify-center">
          <span className="bg-black/50 text-white px-4 py-2 rounded-full text-xs font-bold uppercase tracking-wider backdrop-blur-md">
            Alterar Banner
          </span>
        </div>
        <input 
          type="file" 
          ref={bannerInputRef} 
          hidden 
          accept="image/*" 
          onChange={(e) => handleFileChange(e, 'bannerUrl')} 
        />
      </div>

      <div className="px-6 pb-6">
        {/* Foto de Perfil e Campo de URL do Banner lado a lado para otimizar espaço */}
        <div className="flex flex-col md:flex-row md:items-end gap-4 mb-6">
          <div className="relative -mt-16 inline-block group cursor-pointer shrink-0" onClick={() => fileInputRef.current?.click()}>
            <div className="w-36 h-36 flex items-center justify-center text-5xl font-bold shadow-xl border-4 overflow-hidden"
                style={{ backgroundColor: 'var(--cor-fundo-app)', color: 'var(--cor-botao-texto)', borderColor: 'var(--cor-fundo-card)', borderRadius: '2rem' }}
                >
              <img 
                src={getPreview(formData.fotoPerfil)} 
                className="w-full h-full object-cover group-hover:opacity-50 transition-opacity" 
                alt="Perfil" 
              />
            </div>
            <div className="absolute inset-0 flex items-center justify-center">
               <span className="bg-black/50 text-white px-2 py-1 rounded-full text-[10px] font-bold uppercase tracking-wider backdrop-blur-md">Trocar</span>
            </div>
            <input 
              type="file" 
              ref={fileInputRef} 
              hidden 
              accept="image/*" 
              onChange={(e) => handleFileChange(e, 'fotoPerfil')} 
            />
          </div>

            {/* Campo de URL Direta para o Banner integrado */}
          <div className="flex-1 pb-1">
            <label className="text-[10px] font-bold uppercase opacity-50 px-1 mb-1 block">Ou use uma URL para o Banner</label>
            <input 
              type="text"
              name="bannerUrl"
                    // Se for um arquivo, mostramos um texto informativo ou vazio
              value={formData.bannerUrl instanceof File ? "Arquivo selecionado..." : formData.bannerUrl}
              onChange={handleChange}
              placeholder="https://exemplo.com/imagem.jpg"
              className="w-full p-2 text-sm rounded-xl bg-black/5 dark:bg-white/5 border focus:border-[var(--cor-destaque)] outline-none transition-all"
            />
          </div>
        </div>

        {/* Campos de Texto */}
        <div className="space-y-5">
          <div>
            <label className="text-xs font-bold uppercase opacity-60 mb-1 block px-1">Nome</label>
            <input 
              name="nome"
              value={formData.nome}
              onChange={handleChange}
              placeholder="Seu nome"
              className="w-full p-3 rounded-2xl bg-black/5 dark:bg-white/5 border focus:border-[var(--cor-destaque)] outline-none transition-all font-semibold"
            />
          </div>

          <div>
              <label className="text-xs font-bold uppercase opacity-60 mb-1 block px-1">Cidade</label>
              <input 
                name="cidade"
                value={formData.cidade}
                onChange={handleChange}
                placeholder="Ex: São Paulo, SP"
                className="w-full p-3 rounded-2xl bg-black/5 dark:bg-white/5 border focus:border-[var(--cor-destaque)] outline-none transition-all font-semibold"
              />
            </div>

          <div>
            <label className="text-xs font-bold uppercase opacity-60 mb-1 block px-1">Biografia</label>
            <textarea 
              name="descricao"
              value={formData.descricao}
              onChange={handleChange}
              placeholder="Conte um pouco sobre você..."
              className="w-full p-3 rounded-2xl bg-black/5 dark:bg-white/5 border focus:border-[var(--cor-destaque)] outline-none h-28 resize-none transition-all"
            />
          </div>
        </div>

        {/* Ações Inferiores */}
        <div className="mt-8 pt-6 border-t flex justify-end gap-3" style={{ borderColor: 'var(--cor-fundo-sidebar)' }}>
          <button 
            onClick={onClose}
            className="px-6 py-2 font-bold opacity-60 hover:opacity-100 transition-all text-sm"
          >
            Cancelar
          </button>
          <button 
            onClick={() => onSave(formData)}
            className="px-8 py-2 rounded-xl font-bold shadow-lg transition-all hover:scale-105 active:scale-95 text-sm"
            style={{ 
              backgroundColor: 'var(--cor-destaque)', 
              color: 'white'
            }}
          >
            Salvar Perfil
          </button>
        </div>
      </div>
    </div>
  );
}
