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
    }
}
