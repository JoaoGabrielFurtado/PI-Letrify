"use client";

import { useEffect, useState, useRef } from "react";
import { useRouter } from "next/navigation";

// Função para abrir o Token
function extrairDadosDoToken(token: string) {
  try {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(window.atob(base64).split('').map(function(c) {
        return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
    }).join(''));
    return JSON.parse(jsonPayload);
  } catch (e) {
    return null;
  }
}

export default function EditarPerfilPage() {
  const router = useRouter();
  
  // Referências para os inputs de arquivo (escondidos)
  const fotoInputRef = useRef<HTMLInputElement>(null);
  const bannerInputRef = useRef<HTMLInputElement>(null);

  // Estados dos Dados Reais (API)
  const [nome, setNome] = useState("Carregando...");
  const [cidade, setCidade] = useState("");
  const [idade, setIdade] = useState("");
  const [descricao, setDescricao] = useState("");
  const [fotoPerfilUrl, setFotoPerfilUrl] = useState("");
  const [fotoArquivo, setFotoArquivo] = useState<File | null>(null); // O arquivo real para enviar

  // Estados dos Dados Falsos (Mockados)
  const [bannerUrl, setBannerUrl] = useState("https://images.unsplash.com/photo-1519681393784-d120267933ba?q=80&w=2070&auto=format&fit=crop");
  const [destaques, setDestaques] = useState<any[]>([]); // Lista de vitrines salvas

  // Controle de UI
  const [carregando, setCarregando] = useState(true);
  const [salvando, setSalvando] = useState(false);
  const [modalAberto, setModalAberto] = useState(false);
  const [tokenSessao, setTokenSessao] = useState("");

  // 1. Busca os dados reais ao entrar
  useEffect(() => {
    const rawAuth = localStorage.getItem("letrify-auth");
    if (!rawAuth) {
      router.replace("/login");
      return;
    }

    try {
      const { token } = JSON.parse(rawAuth);
      setTokenSessao(token);
      const dadosDoToken = extrairDadosDoToken(token);
      const chaveId = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier';
      const usuarioId = dadosDoToken?.[chaveId]; 

      if (usuarioId) {
        fetch(`https://letrify.fly.dev/api/usuario/${usuarioId}`, {
          headers: { "Authorization": `Bearer ${token}` }
        })
          .then(res => res.ok ? res.json() : null)
          .then(dados => {
            if (dados) {
              setNome(dados.nome || "");
              setCidade(dados.cidade || "");
              setIdade(dados.idade || "");
              setDescricao(dados.descricao || "");
              setFotoPerfilUrl(dados.fotoPerfil || "");
            }
          })
          .finally(() => setCarregando(false));
      } else {
        setCarregando(false);
      }
    } catch (error) {
      setCarregando(false);
    }
  }, [router]);

  // Lida com a escolha de uma nova imagem (Preview)
  const handleMudancaFoto = (e: React.ChangeEvent<HTMLInputElement>, tipo: "avatar" | "banner") => {
    const file = e.target.files?.[0];
    if (file) {
      const imageUrl = URL.createObjectURL(file);
      if (tipo === "avatar") {
        setFotoPerfilUrl(imageUrl);
        setFotoArquivo(file); // Guarda o arquivo para o FormData
      } else {
        setBannerUrl(imageUrl); // Banner é só mock por enquanto
      }
    }
  };

  // 2. Salva as alterações reais na API!
  const handleSalvarPerfil = async () => {
    setSalvando(true);
    try {
      // Como a API pede FormData (por causa da imagem), criamos um:
      const formData = new FormData();
      formData.append("cidade", cidade);
      formData.append("idade", idade.toString());
      formData.append("descricao", descricao);
      if (fotoArquivo) {
        formData.append("foto", fotoArquivo);
      }

      const resposta = await fetch("https://letrify.fly.dev/api/usuario/editar", {
        method: "PUT",
        headers: {
          "Authorization": `Bearer ${tokenSessao}`
          // Não coloque "Content-Type" aqui, o navegador define automaticamente como multipart/form-data
        },
        body: formData
      });

      if (resposta.ok) {
        alert("Perfil atualizado com sucesso!");
        router.push("/perfil"); // Volta pro perfil normal para ver o resultado
      } else {
        alert("Erro ao salvar as informações na API.");
      }
    } catch (error) {
      console.error(error);
      alert("Erro de conexão.");
    } finally {
      setSalvando(false);
    }
  };

  if (carregando) return <div className="p-10 text-center font-bold animate-pulse">Preparando seu estúdio de edição... 🎨</div>;

  return (
    <div className="max-w-6xl mx-auto mt-4 px-4 pb-20 animate-fade-in relative">
      
      {/* BARRA FLUTUANTE DE SALVAR */}
      <div className="sticky top-4 z-50 flex justify-between items-center p-4 rounded-xl shadow-lg border mb-6 backdrop-blur-md bg-white/80 dark:bg-black/80" style={{ borderColor: 'var(--cor-primaria)' }}>
        <div>
          <h2 className="font-bold text-lg" style={{ color: 'var(--cor-texto-principal)' }}>Modo de Edição</h2>
          <p className="text-xs" style={{ color: 'var(--cor-texto-secundario)' }}>Altere o que quiser e clique em salvar.</p>
        </div>
        <div className="flex gap-3">
          <button onClick={() => router.push('/perfil')} className="px-4 py-2 text-sm font-semibold rounded-md hover:bg-gray-200 dark:hover:bg-gray-800 transition-colors" style={{ color: 'var(--cor-texto-secundario)' }}>
            Cancelar
          </button>
          <button 
            onClick={handleSalvarPerfil}
            disabled={salvando}
            className="px-6 py-2 text-sm font-bold rounded-md shadow-md transition-transform hover:scale-105 disabled:opacity-50"
            style={{ backgroundColor: 'var(--cor-botao-primario)', color: 'var(--cor-botao-texto)' }}
          >
            {salvando ? "Salvando..." : "Salvar Alterações"}
          </button>
        </div>
      </div>

      {/* 1. O CABEÇALHO EDITÁVEL */}
      <div className="rounded-xl shadow-md border mb-6 overflow-hidden relative" style={{ backgroundColor: 'var(--cor-fundo-card)', borderColor: 'var(--cor-fundo-sidebar)' }}>
        
        {/* Banner Editável */}
        <div className="h-48 w-full relative group cursor-pointer" onClick={() => bannerInputRef.current?.click()}>
          <div className="absolute inset-0 bg-cover bg-center" style={{ backgroundImage: `url("${bannerUrl}")`, opacity: 0.9 }}></div>
          <div className="absolute inset-0 bg-black/40 group-hover:bg-black/60 transition-colors flex items-center justify-center">
            <span className="text-white font-bold opacity-0 group-hover:opacity-100 transition-opacity flex items-center gap-2">
              ✏️ Alterar Capa (Mock)
            </span>
          </div>
          <input type="file" hidden ref={bannerInputRef} accept="image/*" onChange={(e) => handleMudancaFoto(e, "banner")} />
        </div>

        <div className="px-8 pb-8 relative">
          <div className="flex flex-col md:flex-row items-center md:items-end gap-6 -mt-16">
            
            {/* Avatar Editável */}
            <div className="relative group cursor-pointer z-10" onClick={() => fotoInputRef.current?.click()}>
              <div className="w-32 h-32 rounded-xl flex items-center justify-center text-5xl font-bold shadow-2xl border-4 overflow-hidden" style={{ backgroundColor: 'var(--cor-primaria)', color: 'var(--cor-botao-texto)', borderColor: 'var(--cor-fundo-card)' }}>
                {fotoPerfilUrl ? (
                  <img src={fotoPerfilUrl} alt="Avatar" className="w-full h-full object-cover" />
                ) : (
                  nome.charAt(0).toUpperCase()
                )}
              </div>
              <div className="absolute inset-0 rounded-xl bg-black/50 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center border-4 border-transparent">
                <span className="text-white text-2xl">📸</span>
              </div>
              <input type="file" hidden ref={fotoInputRef} accept="image/*" onChange={(e) => handleMudancaFoto(e, "avatar")} />
            </div>
            
            {/* Informações Pessoais (Nome é fixo por aqui, mas os outros editam) */}
            <div className="mb-2 w-full max-w-md space-y-2">
              <h1 className="text-4xl font-black drop-shadow-md tracking-wide" style={{ color: 'var(--cor-texto-principal)' }}>{nome}</h1>
              <div className="flex gap-2">
                <input 
                  type="text" 
                  value={cidade} 
                  onChange={(e) => setCidade(e.target.value)} 
                  placeholder="Sua cidade atual (Ex: Santos - SP)"
                  className="w-full p-2 text-sm font-semibold rounded-md border focus:ring-2 outline-none"
                  style={{ backgroundColor: 'var(--cor-fundo-app)', color: 'var(--cor-texto-principal)', borderColor: 'var(--cor-destaque)' }}
                />
                {/* O Campo da Idade para o Match! */}
                <input 
                  type="number" 
                  value={idade} 
                  onChange={(e) => setIdade(e.target.value)} 
                  placeholder="Idade"
                  className="w-24 p-2 text-sm text-center font-semibold rounded-md border focus:ring-2 outline-none"
                  title="Sua idade ajuda no sistema de matches literários!"
                  style={{ backgroundColor: 'var(--cor-fundo-app)', color: 'var(--cor-texto-principal)', borderColor: 'var(--cor-destaque)' }}
                />
              </div>
            </div>
          </div>

          {/* Bio Editável */}
          <div className="mt-6">
            <label className="block text-sm font-bold mb-1" style={{ color: 'var(--cor-texto-secundario)' }}>Sobre mim (Bio):</label>
            <textarea 
              value={descricao}
              onChange={(e) => setDescricao(e.target.value)}
              placeholder="Conte um pouco sobre você e seus gêneros literários favoritos..."
              className="w-full p-4 text-sm rounded-lg border focus:ring-2 outline-none resize-none h-28"
              style={{ backgroundColor: 'var(--cor-fundo-app)', color: 'var(--cor-texto-principal)', borderColor: 'var(--cor-destaque)' }}
            />
          </div>
        </div>
      </div>

      {/* 2. O GERENCIADOR DE DESTAQUES (Vitrines) */}
      <h2 className="text-xl font-bold mb-4 mt-10" style={{ color: 'var(--cor-texto-principal)' }}>Minhas Vitrines (Destaques)</h2>
      
      <div className="grid grid-cols-1 gap-6">
        {/* Aqui renderizaria os destaques que ele já tem... */}

        {/* O BOTÃO TRACEJADO MÁGICO */}
        <button 
          onClick={() => setModalAberto(true)}
          className="w-full h-32 rounded-xl border-2 border-dashed flex flex-col items-center justify-center transition-all hover:bg-black/5 dark:hover:bg-white/5 group"
          style={{ borderColor: 'var(--cor-destaque)' }}
        >
          <div className="w-12 h-12 rounded-full border-2 flex items-center justify-center text-2xl font-bold mb-2 group-hover:scale-110 transition-transform" style={{ borderColor: 'var(--cor-primaria)', color: 'var(--cor-primaria)' }}>
            +
          </div>
          <span className="font-semibold" style={{ color: 'var(--cor-texto-secundario)' }}>Adicionar nova vitrine</span>
        </button>
      </div>

      {/* 3. O MODAL DE ESCOLHA (Pop-up) */}
      {modalAberto && (
        <div className="fixed inset-0 z-[100] flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm animate-fade-in">
          <div 
            className="w-full max-w-lg rounded-2xl shadow-2xl p-6 relative"
            style={{ backgroundColor: 'var(--cor-fundo-card)', borderColor: 'var(--cor-fundo-sidebar)', border: '1px solid' }}
          >
            <button 
              onClick={() => setModalAberto(false)}
              className="absolute top-4 right-4 text-2xl opacity-50 hover:opacity-100"
            >
              &times;
            </button>
            
            <h3 className="text-2xl font-bold mb-2" style={{ color: 'var(--cor-texto-principal)' }}>Escolha o Destaque</h3>
            <p className="text-sm mb-6" style={{ color: 'var(--cor-texto-secundario)' }}>Qual bloco de conteúdo quer adicionar ao seu perfil?</p>

            <div className="space-y-3">
              <button className="w-full text-left p-4 rounded-lg border hover:scale-[1.02] transition-transform" style={{ backgroundColor: 'var(--cor-fundo-app)', borderColor: 'var(--cor-fundo-sidebar)' }}>
                <div className="font-bold text-lg mb-1" style={{ color: 'var(--cor-primaria)' }}>📖 Leitura Atual</div>
                <div className="text-sm" style={{ color: 'var(--cor-texto-secundario)' }}>Mostra o livro que você está lendo com a barra de progresso.</div>
              </button>

              <button className="w-full text-left p-4 rounded-lg border hover:scale-[1.02] transition-transform" style={{ backgroundColor: 'var(--cor-fundo-app)', borderColor: 'var(--cor-fundo-sidebar)' }}>
                <div className="font-bold text-lg mb-1" style={{ color: 'var(--cor-primaria)' }}>🏆 Livro Favorito</div>
                <div className="text-sm" style={{ color: 'var(--cor-texto-secundario)' }}>Escolha aquele livro 5 estrelas para ficar em evidência.</div>
              </button>

              <button className="w-full text-left p-4 rounded-lg border hover:scale-[1.02] transition-transform" style={{ backgroundColor: 'var(--cor-fundo-app)', borderColor: 'var(--cor-fundo-sidebar)' }}>
                <div className="font-bold text-lg mb-1" style={{ color: 'var(--cor-primaria)' }}>🎭 Top 3 por Gênero</div>
                <div className="text-sm" style={{ color: 'var(--cor-texto-secundario)' }}>Exiba as 3 capas dos seus favoritos em Fantasia, Romance, etc.</div>
              </button>
            </div>
          </div>
        </div>
      )}

    </div>
  );
}