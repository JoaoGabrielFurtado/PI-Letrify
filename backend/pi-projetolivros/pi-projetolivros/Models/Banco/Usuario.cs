using System;
using System.Collections.Generic;

namespace pi_projetolivros.Models.Banco;

public partial class Usuario
{
    public int Id { get; set; }

    public string Nome { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Senha { get; set; } = null!;

    public int? Idade { get; set; }
    public string? Cidade { get; set; }
    public string? Descricao { get; set; }
    public string? FotoPerfil { get; set; }

    public virtual ICollection<SituacaoLivro> SituacaoLivros { get; set; } = new List<SituacaoLivro>();
}
