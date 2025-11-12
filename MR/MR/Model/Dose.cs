using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MR.Model
{
    [Table("Doses")]
    public class Dose
    {
        [PrimaryKey]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Indexed]
        public Guid MedicineId { get; set; }

        public DateTime ScheduledAt { get; set; }
        public bool Taken { get; set; } = false;
        public DateTime? TakenAt { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
