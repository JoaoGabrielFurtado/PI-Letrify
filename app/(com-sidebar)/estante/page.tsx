import EstanteUsuario from "@/components/EstanteUsuario";

export default function MatchPage() {
  return (
    <div className="max-w-7xl mx-auto w-full pt-8 pb-20 relative animate-fade-in px-4">
      
      {/* Aqui você pode adicionar um cabeçalho de página se quiser, 
        mas como o próprio Radar já tem um título lindão ("Conexões Literárias"), 
        nós podemos apenas plugar o componente direto! 
      */}
      
      <EstanteUsuario />
      
    </div>
  );
}