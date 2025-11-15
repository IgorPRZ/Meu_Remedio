
using MR.Interface;
using MR.Model;
using MR.Model.EnumFolder;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeuRemedio.Services
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

        public async Task<IEnumerable<Dose>> GenerateUpcomingDosesAsync(Guid medicineId, int daysAhead = 7)
        {
            var med = await _repo.GetMedicineAsync(medicineId)
                      ?? throw new InvalidOperationException("Medicamento não encontrado.");

            var rules = (await _repo.GetRulesAsync(medicineId)).Where(r => r.Enabled);
            var now = _clock.Now;
            var end = now.AddDays(daysAhead);

            var result = new List<Dose>();

            foreach (var rule in rules)
            {
                DateTime cursor = now.Date;
                while (cursor <= end)
                {
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
                        DateTime start = rule.ValidFrom ?? now;
                        var s = start;
                        while (s <= end)
                        {
                            if (s >= now) result.Add(new Dose { MedicineId = med.Id, ScheduledAt = s });
                            s = s.AddHours(rule.IntervalHours);
                        }
                        break;
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

            // remove duplicatas e ordena
            var ordered = result
                .GroupBy(d => d.ScheduledAt)
                .Select(g => g.First())
                .OrderBy(d => d.ScheduledAt)
                .ToList();

            return ordered;
        }

        public async Task ScheduleDoseNotificationAsync(Dose dose)
        {
            var med = await _repo.GetMedicineAsync(dose.MedicineId);
            if (med == null) return;

            var request = new NotificationRequest
            {
                NotificationId = dose.Id.GetHashCode(),
                Title = $"Hora do remédio: {med.Name}",
                Description = $"Tomar {med.Dosage} ({med.Form})",
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = dose.ScheduledAt,
                    NotifyRepeatInterval = null
                },
                CategoryType = NotificationCategoryType.Alarm,
                Android = new AndroidOptions
                {
                    Priority = AndroidPriority.High,
                    VibrationPattern = new long[] { 0, 250, 250, 250 }
                }
            };

            await LocalNotificationCenter.Current.Show(request);
        }

        public Task CancelDoseNotificationAsync(Guid doseId)
        {
            int notifId = doseId.GetHashCode();
            LocalNotificationCenter.Current.Cancel(notifId);
            return Task.CompletedTask;
        }
    }

    public interface IReminderScheduler
    {
        Task<IEnumerable<Dose>> GenerateUpcomingDosesAsync(Guid medicineId, int daysAhead = 7);
        Task ScheduleDoseNotificationAsync(Dose dose);
        Task CancelDoseNotificationAsync(Guid doseId);
    }

    public interface ILocalClock { DateTime Now { get; } }
    public class SystemClock : ILocalClock { public DateTime Now => DateTime.Now; }
}
