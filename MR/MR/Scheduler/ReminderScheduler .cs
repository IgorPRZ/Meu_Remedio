using MR.Interface;
using MR.Model;
using MR.Model.EnumFolder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MR.Scheduler
{
    public class ReminderScheduler : IReminderScheduler
    {
        private readonly IRepository _repo;
        private readonly ILocalClock _clock;

        public ReminderScheduler(IRepository repo, ILocalClock clock)
        {
            _repo = repo;
            _clock = clock;
        }

        // Gera instâncias futuras (não grava automaticamente no repo)
        public async Task<IEnumerable<Dose>> GenerateUpcomingDosesAsync(Guid medicineId, int daysAhead = 7)
        {
            var med = await _repo.GetMedicineAsync(medicineId)
                      ?? throw new InvalidOperationException("Medicine not found");

            var rules = (await _repo.GetRulesAsync(medicineId)).Where(r => r.Enabled);

            var now = _clock.Now;
            var end = now.Date.AddDays(daysAhead).AddDays(1).AddTicks(-1);

            var result = new List<Dose>();

            foreach (var rule in rules)
            {
                DateTime cursor = now.Date;
                while (cursor <= end)
                {
                    // valida janela de regra
                    if (rule.ValidFrom.HasValue && cursor.Date < rule.ValidFrom.Value.Date) { cursor = cursor.AddDays(1); continue; }
                    if (rule.ValidUntil.HasValue && cursor.Date > rule.ValidUntil.Value.Date) break;

                    if (rule.Type == ReminderType.SpecificTimes || rule.Type == ReminderType.Daily)
                    {
                        foreach (var t in rule.TimesOfDay)
                        {
                            var scheduled = cursor.Date + t;
                            if (scheduled >= now && scheduled <= end)
                            {
                                result.Add(new Dose { MedicineId = med.Id, ScheduledAt = scheduled });
                            }
                        }
                    }
                    else if (rule.Type == ReminderType.IntervalHours)
                    {
                        // intervalo a partir do ValidFrom ou a partir do início do dia atual
                        DateTime start = rule.ValidFrom ?? cursor.Date;
                        var s = start;
                        while (s <= end)
                        {
                            if (s >= now) result.Add(new Dose { MedicineId = med.Id, ScheduledAt = s });
                            s = s.AddHours(rule.IntervalHours);
                        }
                        break; // já varremos todo intervalo globalmente
                    }
                    else if (rule.Type == ReminderType.Weekly)
                    {
                        if (rule.WeekDays.Contains(cursor.DayOfWeek))
                        {
                            foreach (var t in rule.TimesOfDay)
                            {
                                var scheduled = cursor.Date + t;
                                if (scheduled >= now && scheduled <= end)
                                    result.Add(new Dose { MedicineId = med.Id, ScheduledAt = scheduled });
                            }
                        }
                    }

                    cursor = cursor.AddDays(1);
                }
            }

            // ordenar e deduplicar por horário
            var ordered = result
                .GroupBy(d => d.ScheduledAt)
                .Select(g => g.First())
                .OrderBy(d => d.ScheduledAt)
                .ToList();

            return ordered;
        }

        // Essas duas operações devem ser implementadas com notificações nativas na app; aqui são stubs.
        public Task ScheduleDoseNotificationAsync(Dose dose)
        {
            // TODO: integrar com plugin de notificações (dependendo da plataforma).
            return Task.CompletedTask;
        }

        public Task CancelDoseNotificationAsync(Guid doseId)
        {
            // TODO: cancelar notificação nativa.
            return Task.CompletedTask;
        }
    }

    // Pequena abstração de relógio para facilitar testes
    public interface ILocalClock { DateTime Now { get; } }
    public class SystemClock : ILocalClock { public DateTime Now => DateTime.Now; }
}
