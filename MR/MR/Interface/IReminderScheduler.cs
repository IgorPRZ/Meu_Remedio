using MR.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MR.Interface
{
    public interface IReminderScheduler
    {
        // Calcula próximos N doses (em memória) para exibição
        Task<IEnumerable<Dose>> GenerateUpcomingDosesAsync(Guid medicineId, int daysAhead = 7);

        // Schedule a notificacao nativa (implementação plataforma)
        Task ScheduleDoseNotificationAsync(Dose dose);

        // Cancela notificacao
        Task CancelDoseNotificationAsync(Guid doseId);
    }
}
