﻿using System.ComponentModel;

namespace vendtechext.DAL.Common
{
    public enum PlatformTypeEnum
    {
        All = 0,
        AIRTIME = 1,
        CABLE_TV = 2,
        DATA = 3,
        ELECTRICITY = 4,
    }

    public enum TransactionStatus
    {
        InProgress = 0,
        [Description("Success")]
        Success = 1,
        Pending = 2,
        Failed = 3,
        Error = 4,
    }

    public enum LogType
    {
        Error = 0,
        Infor = 1,
        Warning = 2,
    }

}
