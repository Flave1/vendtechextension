﻿namespace vendtechext.DAL.Models
{
    public class AuditTrail
    {
        public bool Deleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set;}
    }
}
