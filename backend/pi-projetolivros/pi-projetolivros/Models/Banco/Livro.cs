using System;
using System.Collections.Generic;

namespace pi_projetolivros.Models.Banco;

public partial class Livro
{
    public int Id { get; set; }
    public string Titulo { get; set; } = null!;
    public string? Autor { get; set; }
    public string? Isbn { get; set; }
    public string? Temas { get; set; }
    public virtual ICollection<SituacaoLivro> SituacaoLivros { get; set; } = new List<SituacaoLivro>();
}
