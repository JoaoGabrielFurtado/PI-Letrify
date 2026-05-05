using Microsoft.EntityFrameworkCore;
using pi_projetolivros_banco;

namespace pi_projetolivros.Servicos
{
    public class LimpezaBancoChat : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public LimpezaBancoChat(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var tempoDeEspera = TimeSpan.FromHours(24);
            using var timer = new PeriodicTimer(tempoDeEspera);

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                using var scope = _scopeFactory.CreateScope();
                var contexto = scope.ServiceProvider.GetRequiredService<Banco>();

                var dataLimite = DateTime.UtcNow.AddDays(-5);

                var mensagensVelhas = await contexto.MensagensChat
                    .Include(m => m.Respostas)
                    .Where(m => m.DataPostagem < dataLimite)
                    .ToListAsync(stoppingToken);

                if (mensagensVelhas.Any())
                {
                    foreach (var msg in mensagensVelhas)
                    {
                        if (msg.Respostas.Any())
                        {
                            contexto.MensagensChat.RemoveRange(msg.Respostas);
                        }
                    }

                    contexto.MensagensChat.RemoveRange(mensagensVelhas);

                    await contexto.SaveChangesAsync(stoppingToken);
                }
            }
        }
    }
}
