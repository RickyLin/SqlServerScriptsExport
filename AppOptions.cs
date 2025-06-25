namespace SqlServerScriptsExport
{
    public class AppOptions
    {
        public bool Verbose { get; set; } = false;
        public bool Quiet { get; set; } = false;
        public bool NoProgress { get; set; } = false;
        public string? LogFilePath { get; set; }
        public int ConnectionTimeout { get; set; } = 30;
        public bool ShowHelp { get; set; } = false;
    }
}
