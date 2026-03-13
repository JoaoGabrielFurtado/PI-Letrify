using pi_projetolivros.Models.Banco;
using System;
using System.Collections.Generic;

namespace pi_projetolivros.Models;

public partial class SituacaoLivro
{
    public int Id { get; set; }

    public int? UsuarioId { get; set; }

    public int? LivroId { get; set; }

    public string? Status { get; set; }

    public DateTime? DataAtualizacao { get; set; }

    public virtual Livro? Livro { get; set; }

    public virtual Usuario? Usuario { get; set; }
}
