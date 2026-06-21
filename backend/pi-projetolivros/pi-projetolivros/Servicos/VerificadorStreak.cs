// Servicos/VerificadorStreak.cs
using Microsoft.EntityFrameworkCore;
using pi_projetolivros_banco;

namespace pi_projetolivros.Servicos;

public class VerificadorStreak : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public VerificadorStreak(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromHours(24));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            using var scope = _scopeFactory.CreateScope();
            var contexto = scope.ServiceProvider.GetRequiredService<Banco>();

            var ontem = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
            var anteontem = ontem.AddDays(-1);

            // Streaks que não fizeram check-in nem ontem nem anteontem = quebrados
            var streaksQuebrados = await contexto.StreaksLeitura
                .Where(s => s.StreakAtual > 0 && s.UltimoCheckIn != null && s.UltimoCheckIn < anteontem)
                .ToListAsync();

            foreach (var streak in streaksQuebrados)
                streak.StreakAtual = 0;

            await contexto.SaveChangesAsync();
        }
    }
}