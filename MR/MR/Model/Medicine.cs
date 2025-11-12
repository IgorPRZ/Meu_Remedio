using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MR.Model
{
    [Table("Medicines")]
    public class Medicine
    {
        [PrimaryKey]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Indexed]
        public string Name { get; set; } = string.Empty;

        public string Dosage { get; set; } = string.Empty;
        public string Form { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTime? TreatmentStart { get; set; }
        public DateTime? TreatmentEnd { get; set; }

        public bool IsActive(DateTime now) =>
            (!TreatmentStart.HasValue || TreatmentStart.Value.Date <= now.Date) &&
            (!TreatmentEnd.HasValue || TreatmentEnd.Value.Date >= now.Date);
    }
}
