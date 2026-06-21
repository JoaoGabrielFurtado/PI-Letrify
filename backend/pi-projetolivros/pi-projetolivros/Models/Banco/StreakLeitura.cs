namespace pi_projetolivros.Models.Banco;

public class StreakLeitura
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int StreakAtual { get; set; } = 0;
    public int MaiorStreak { get; set; } = 0;
    public DateOnly? UltimoCheckIn { get; set; }
    public int CongelamentosDisponiveis { get; set; } = 1;

    public Usuario Usuario { get; set; } = null!;
}
