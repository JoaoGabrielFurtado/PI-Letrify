// DTO/Grupos/CriarGrupoDto.cs
public class CriarGrupoDto
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? Status { get; set; } // "Aberto" | "Fechado"
    public IFormFile? Foto { get; set; }
}

// DTO/Grupos/EditarGrupoDto.cs
public class EditarGrupoDto
{
    public string? Nome { get; set; }
    public string? Descricao { get; set; }
    public string? Status { get; set; }
    public IFormFile? Foto { get; set; }
}

// DTO/Grupos/ResponderSolicitacaoDto.cs
public class ResponderSolicitacaoDto
{
    public bool Aceitar { get; set; }
}

// DTO/Grupos/AlterarRoleDto.cs
public class AlterarRoleDto
{
    public string Role { get; set; } = string.Empty; // "Admin" | "Membro"
}

// DTO/Grupos/CriarPostGrupoDto.cs
public class CriarPostGrupoDto
{
    public string Conteudo { get; set; } = string.Empty;
    public int? PostPaiId { get; set; }
}

// DTO/Grupos/EnviarMensagemGrupoDto.cs
public class EnviarMensagemGrupoDto
{
    public string Conteudo { get; set; } = string.Empty;
}