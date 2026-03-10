namespace pi_projetolivros.Models;

public class LivroClasse
{
    public string Isbn { get; set; }
    public string Titulo { get; set; }
    public string AutorPrincipal { get; set; }
    public string DataPublicacao { get; set; }
    public int Paginas { get; set; }
    public string Editora { get; set; }

    public List<string> Temas { get; set; } = new List<string>();
}
