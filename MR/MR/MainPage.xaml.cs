using MeuRemedio.Services;
using MR.Database;
using MR.Model;
using MR.Model.EnumFolder;

namespace MR
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();


            Task.Run(async() => 
            {
                var repo = new SQLiteRepository();
                await repo.InitializeAsync();

                var med = new Medicine
                {
                    Name = "Dipirona",
                    Dosage = "500 mg",
                    Form = "Comprimido"
                };
                await repo.AddOrUpdateMedicineAsync(med);

                var rule = new ReminderRule
                {
                    MedicineId = med.Id,
                    Type = ReminderType.SpecificTimes,
                    TimesOfDay = new() { new TimeSpan(10, 0, 0), new TimeSpan(23, 15, 0) }
                };
                await repo.AddOrUpdateRuleAsync(rule);

                var scheduler = new ReminderScheduler(repo, new SystemClock());
                var doses = await scheduler.GenerateUpcomingDosesAsync(med.Id, 2);

                foreach (var d in doses)
                {
                    await repo.AddOrUpdateDoseAsync(d);
                    await scheduler.ScheduleDoseNotificationAsync(d);
                }



            });
            

        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }
    }

}
