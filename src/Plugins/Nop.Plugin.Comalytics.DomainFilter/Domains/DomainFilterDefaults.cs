namespace Nop.Plugin.Comalytics.DomainFilter.Domains
{
    public static class DomainFilterDefaults
    {
        public static string DomainOrEmailBlacklistTableName => "CP_Domain";
        public static string SqlServerStoredProceduresCreateFilePath => "~/Plugins/Comalytics.DomainFilter/DBScripts/SqlServer.StoredProcedures.Create.sql";
        public static string SqlServerStoredProceduresDropFilePath => "~/Plugins/Comalytics.DomainFilter/DBScripts/SqlServer.StoredProcedures.Drop.sql";
        public static string SqlServerTablesCreateFilePath => "~/Plugins/Comalytics.DomainFilter/DBScripts/SqlServer.ImportTable.Create.sql";
        public static string SqlServerTablesDropFilePath => "~/Plugins/Comalytics.DomainFilter/DBScripts/SqlServer.ImportTable.Drop.sql";
    }
}
