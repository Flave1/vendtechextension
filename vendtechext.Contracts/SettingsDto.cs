namespace vendtechext.Contracts
{
    public class SettingDto
    {
        public string Value { get; set; }
    }
    public class SettingsPayload
    {
        public NotificationSettings Notification { get; set; }
        public bool DisableElectricitySales { get; set; }
        public List<Commission> Commission { get; set; }
        public Thresholds Threshholds { get; set; }
    }

    public class NotificationSettings
    {
        public bool Deposits { get; set; }
        public bool LowBalance { get; set; }
        public bool ServiceDisabled { get; set; }
    }

    public class Commission
    {
        public int Id { get; set; }
        public double Percentage { get; set; }
    }

    public class Thresholds
    {
        public int MinimumVend { get; set; }
    }

}
