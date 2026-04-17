using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace pi_projetolivros.Models.Banco;

public partial class Avaliaco
{
    public int Id { get; set; }

    public int? UsuarioId { get; set; }

    public int? LivroId { get; set; }

    public int? Nota { get; set; }

    public string? Resenha { get; set; }

    [ForeignKey("usuario_id")]
    public virtual Usuario Usuario { get; set; }

    [ForeignKey("livro_id")]
    public virtual Livro Livro { get; set; }

    public DateTime? DataAvaliacao { get; set; }
}
