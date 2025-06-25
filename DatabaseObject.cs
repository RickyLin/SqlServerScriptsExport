using System;

namespace SqlServerScriptsExport
{
    public class DatabaseObject
    {
        public string Name { get; set; } = string.Empty;
        public string Definition { get; set; } = string.Empty;
        public string ObjectType { get; set; } = string.Empty;
        public string Schema { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string? ParentTable { get; set; } // For triggers
        public bool IsUserDefined { get; set; } = true;
    }

    public enum DatabaseObjectType
    {
        View,
        StoredProcedure,
        ScalarFunction,
        TableValuedFunction,
        Trigger
    }
}
