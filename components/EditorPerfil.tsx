"use client";
import { useState } from "react";

interface EditorPerfilProps {
  dadosIniciais: any;
  onClose: () => void;
  onSave: (novosDados: any) => void;
}

export default function EditorPerfil({ dadosIniciais, onClose, onSave }: EditorPerfilProps) {
  const [formData, setFormData] = useState(dadosIniciais);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData((prev: any) => ({ ...prev, [name]: value }));
  };

  // Para capturar o arquivo de imagem
  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files[0]) {
      const arquivo = e.target.files[0];
      
      setFormData((prev: any) => ({ 
        ...prev, 
        fotoPerfil: arquivo // Aqui salvamos o arquivo real (binário)
      }));
    }
  };

  return (
    <div className="fixed inset-0 z-[100] flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm animate-fade-in">
      <div 
        className="w-full max-w-2xl rounded-3xl shadow-2xl overflow-hidden border"
        style={{ backgroundColor: 'var(--cor-fundo-card)', borderColor: 'var(--cor-fundo-sidebar)' }}
      >
        {/* Cabeçalho do Modal */}
        <div className="p-6 border-b flex justify-between items-center" style={{ borderColor: 'var(--cor-fundo-sidebar)' }}>
          <h2 className="text-xl font-black" style={{ color: 'var(--cor-texto-principal)' }}>Editar Perfil</h2>
          <button onClick={onClose} className="opacity-50 hover:opacity-100 text-2xl">✕</button>
        </div>

        {/* Formulário */}
        <div className="p-6 space-y-4 max-h-[70vh] overflow-y-auto">
          <div>
            <label className="text-xs font-bold uppercase opacity-60 mb-1 block">Nome</label>
            <input 
              name="nome"
              value={formData.nome}
              onChange={handleChange}
              className="w-full p-3 rounded-xl bg-black/5 dark:bg-white/5 border focus:border-[var(--cor-destaque)] outline-none transition-all"
            />
          </div>

          <div>
            <label className="text-xs font-bold uppercase opacity-60 mb-1 block">Biografia</label>
            <textarea 
              name="descricao"
              value={formData.descricao}
              onChange={handleChange}
              className="w-full p-3 rounded-xl bg-black/5 dark:bg-white/5 border focus:border-[var(--cor-destaque)] outline-none h-24 resize-none transition-all"
            />
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="text-xs font-bold uppercase opacity-60 mb-1 block">Foto de Perfil</label>
              <input 
                type="file" 
                accept="image/*" // Aceita apenas imagens
                onChange={handleFileChange} 
                className="w-full p-2 text-sm bg-black/5 dark:bg-white/5 rounded-xl border cursor-pointer file:mr-4 file:py-1 file:px-4 file:rounded-full file:border-0 file:text-xs file:font-semibold file:bg-[var(--cor-fundo-card)] file:text-[var(--cor-botao-texto)] hover:file:opacity-80"
                />
                {/* Dica: se for um arquivo, mostra o nome dele, se for string mostra o link */}
                <p className="text-[10px] mt-1 opacity-50 truncate px-1">
                    {formData.fotoPerfil instanceof File 
                        ? `✅ Selecionado: ${formData.fotoPerfil.name}` 
                        : "⚠️ Nenhum arquivo selecionado"}
                </p>
            </div>
            <div>
              <label className="text-xs font-bold uppercase opacity-60 mb-1 block">URL Banner</label>
              <input name="bannerUrl" value={formData.bannerUrl} onChange={handleChange} className="w-full p-3 rounded-xl bg-black/5 dark:bg-white/5 border text-sm focus:border-[var(--cor-destaque)] outline-none" />
            </div>
          </div>
        </div>

        {/* Ações */}
        <div className="p-6 flex justify-end gap-3 bg-black/5 dark:bg-white/5">
        <button 
          onClick={onClose}
          className="px-6 py-2 font-bold opacity-60 hover:opacity-100 transition-all text-sm"
        >
          Cancelar
        </button>
        <button 
          onClick={() => onSave(formData)} // Envia o formData com o arquivo dentro
          className="px-8 py-2 rounded-xl font-bold shadow-lg transition-all hover:scale-105 active:scale-95 text-sm"
            style={{ backgroundColor: 'var(--cor-fundo-app)', color: 'var(--cor-texto-principal)', border: '2px solid var(--cor-destaque)' }}
        >
          Salvar Alterações
          </button>
        </div>
      </div>
    </div>
  );
}
