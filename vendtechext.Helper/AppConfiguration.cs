using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using vendtechext.Contracts;
using vendtechext.DAL.Models;

namespace vendtechext.Helper
{
    public class AppConfiguration
    {
        private static SettingsPayload _settingsPayload;

        public async Task SaveSettings(SettingDto dto)
        {
            using(DataContext dataContext = new DataContext())
            {
                AppSetting setting = await dataContext.AppSettings.FirstOrDefaultAsync();
                setting.Value = dto.Value;
                await dataContext.SaveChangesAsync();
                _settingsPayload = null;
            }
            new AppConfiguration();
        }

        public static async Task UpdateSettings(SettingsPayload payload)
        {
            using (DataContext dataContext = new DataContext())
            {
                AppSetting setting = await dataContext.AppSettings.FirstOrDefaultAsync();
                setting.Value = JsonConvert.SerializeObject(payload);
                await dataContext.SaveChangesAsync();
                _settingsPayload = null;
            }
            new AppConfiguration();
        }

        public AppConfiguration()
        {
            if (_settingsPayload == null)
            {
                using (DataContext dataContext = new DataContext())
                {
                    var setting = dataContext.AppSettings.FirstOrDefaultAsync().Result;
                    _settingsPayload = JsonConvert.DeserializeObject<SettingsPayload>(setting.Value);
                }
               
            }
        }

        public static SettingsPayload GetSettings()
        {
            if( _settingsPayload == null)
                new AppConfiguration();
            return _settingsPayload;
        }

        public static async Task DisableSales()
        {
            var settings = GetSettings();
            settings.DisableElectricitySales = true;
            await UpdateSettings(settings);
        }

        public static decimal ProcessCommsion(decimal amount, int commissionId)
        {
            try
            {
                SettingsPayload settings = GetSettings();
                string commissionLevel = settings.Commission.FirstOrDefault(d => d.Id == commissionId).Percentage.ToString();
                decimal.TryParse(commissionLevel, out decimal percentage);

                return Math.Round(amount * percentage / 100, 2);
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}
