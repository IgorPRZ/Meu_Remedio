using MR.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MR.Interface
{
    public interface IRepository
    {
        // Medicine
        Task<IEnumerable<Medicine>> GetMedicinesAsync();
        Task<Medicine?> GetMedicineAsync(Guid id);
        Task AddOrUpdateMedicineAsync(Medicine medicine);
        Task DeleteMedicineAsync(Guid id);

        // Rule
        Task<IEnumerable<ReminderRule>> GetRulesAsync(Guid? medicineId = null);
        Task<ReminderRule?> GetRuleAsync(Guid id);
        Task AddOrUpdateRuleAsync(ReminderRule rule);
        Task DeleteRuleAsync(Guid id);

        // Dose logs
        Task<IEnumerable<Dose>> GetDosesAsync(Guid? medicineId = null, DateTime? from = null, DateTime? to = null);
        Task<Dose?> GetDoseAsync(Guid id);
        Task AddOrUpdateDoseAsync(Dose dose);
        Task DeleteDoseAsync(Guid id);
    }
}
