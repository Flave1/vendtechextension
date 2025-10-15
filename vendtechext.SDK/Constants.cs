namespace vendtechext.SDK
{
    public static class API_MESSAGE_CONSTANTS
    {
        public const int REQUEST_TIMEOUT = 4084;
        public const int VENDING_DISABLE = 4514;
        public const int AMOUNT_TOO_LOW = 4094;
        public const int BAD_REQUEST = 4004;
        public const int OKAY_REQEUST = 2002;
        public const int REQUEST_PENDING = 2022;
        public const int ACCESS_DENIED = 4034;
        public const int AUTHENTICATION_ERROR = 4014;
        public const int NOTFOUND_ERROR = 4044;
    }

    public static class CREDENTIALS
    {
        public const string INTEGRATOR_PASSWORD = "Password@123";
        public const string AGENCY_PASSWORD = "Agency@0000";
        public const string VENDOR_PASSWORD = "Vendor@0000";
    }

    public static class APP_ROLES
    {
        public const string SuperAdmin = "Super Admin";
        public const string Integrator = "Integrator";
        public const string Vendor = "Vendor";
        public const string Agency = "Agency";
    }

    public static class CacheKeys
    {
        public const string AgencyUsers = "agency_users";
        public const string VendorUsers = "vendor_users";
    }
}
