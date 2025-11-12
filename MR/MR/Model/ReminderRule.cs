using MR.Model.EnumFolder;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MR.Model
{
    [Table("ReminderRules")]
    public class ReminderRule
    {
        [PrimaryKey]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Indexed]
        public Guid MedicineId { get; set; }

        public ReminderType Type { get; set; }
        public string TimesOfDaySerialized { get; set; } = string.Empty; // serializado JSON
        public int IntervalHours { get; set; } = 8;
        public string WeekDaysSerialized { get; set; } = string.Empty;   // serializado JSON
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidUntil { get; set; }
        public bool Enabled { get; set; } = true;

        [Ignore]
        public List<TimeSpan> TimesOfDay
        {
            get => string.IsNullOrEmpty(TimesOfDaySerialized)
                ? new List<TimeSpan>()
                : System.Text.Json.JsonSerializer.Deserialize<List<TimeSpan>>(TimesOfDaySerialized);
            set => TimesOfDaySerialized = System.Text.Json.JsonSerializer.Serialize(value);
        }

        [Ignore]
        public HashSet<DayOfWeek> WeekDays
        {
            get => string.IsNullOrEmpty(WeekDaysSerialized)
                ? new HashSet<DayOfWeek>()
                : System.Text.Json.JsonSerializer.Deserialize<HashSet<DayOfWeek>>(WeekDaysSerialized);
            set => WeekDaysSerialized = System.Text.Json.JsonSerializer.Serialize(value);
        }
    }

}
