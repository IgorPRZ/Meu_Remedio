using MR.Interface;
using MR.Model;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MR.Database
{
    public class SQLiteRepository : IRepository
    {
        private readonly SQLiteAsyncConnection _db;

        public SQLiteRepository()
        {

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "mrdata.db3");
            _db = new SQLiteAsyncConnection(dbPath);
            InitializeAsync();
        }

        public async Task InitializeAsync()
        {
            await _db.CreateTableAsync<Medicine>();
            await _db.CreateTableAsync<ReminderRule>();
            await _db.CreateTableAsync<Dose>();
        }

        // --- Medicines ---
        public Task<IEnumerable<Medicine>> GetMedicinesAsync() =>
            _db.Table<Medicine>().ToListAsync().ContinueWith(t => t.Result.AsEnumerable());

        public async Task<Medicine?> GetMedicineAsync(Guid id) =>
            await _db.Table<Medicine>().Where(m => m.Id == id).FirstOrDefaultAsync();

        public Task AddOrUpdateMedicineAsync(Medicine medicine) =>
            _db.InsertOrReplaceAsync(medicine);

        public Task DeleteMedicineAsync(Guid id) =>
            _db.DeleteAsync<Medicine>(id);

        // --- Rules ---
        public async Task<IEnumerable<ReminderRule>> GetRulesAsync(Guid? medicineId = null)
        {
            if (medicineId == null)
                return await _db.Table<ReminderRule>().ToListAsync();
            return await _db.Table<ReminderRule>().Where(r => r.MedicineId == medicineId.Value).ToListAsync();
        }

        public Task<ReminderRule?> GetRuleAsync(Guid id) =>
            _db.Table<ReminderRule>().Where(r => r.Id == id).FirstOrDefaultAsync();

        public Task AddOrUpdateRuleAsync(ReminderRule rule) =>
            _db.InsertOrReplaceAsync(rule);

        public Task DeleteRuleAsync(Guid id) =>
            _db.DeleteAsync<ReminderRule>(id);

        // --- Doses ---
        public async Task<IEnumerable<Dose>> GetDosesAsync(Guid? medicineId = null, DateTime? from = null, DateTime? to = null)
        {
            var query = _db.Table<Dose>();

            if (medicineId.HasValue)
                query = query.Where(d => d.MedicineId == medicineId.Value);
            if (from.HasValue)
                query = query.Where(d => d.ScheduledAt >= from.Value);
            if (to.HasValue)
                query = query.Where(d => d.ScheduledAt <= to.Value);

            return await query.ToListAsync();
        }

        public Task<Dose?> GetDoseAsync(Guid id) =>
            _db.Table<Dose>().Where(d => d.Id == id).FirstOrDefaultAsync();

        public Task AddOrUpdateDoseAsync(Dose dose) =>
            _db.InsertOrReplaceAsync(dose);

        public Task DeleteDoseAsync(Guid id) =>
            _db.DeleteAsync<Dose>(id);
    }
}
