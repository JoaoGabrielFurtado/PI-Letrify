using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace pi_projetolivros.Models.Banco;

public partial class Favorito
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public int LivroId { get; set; }

    [ForeignKey("UsuarioId")]
    public virtual Usuario Usuario { get; set; }

    [ForeignKey("LivroId")]
    public virtual Livro Livro { get; set; }

    public DateTime? DataFavoritado { get; set; }
}
