namespace vendtechext.Helper.Configurations
{
    public class ProviderInformation
    {
        public string ProductionUrl { get; init; }
        public string SandboxUrl { get; init; }
        public string UserName { get; init; }
        public string Password { get; init; }

        public void Validate()
        {
            if (string.IsNullOrEmpty(ProductionUrl))
                throw new ArgumentNullException(nameof(ProductionUrl));
            if (string.IsNullOrEmpty(UserName))
                throw new ArgumentNullException(nameof(UserName));
            if (string.IsNullOrEmpty(Password))
                throw new ArgumentNullException(nameof(Password));
        }
    }
}
