using Microsoft.Data.SqlClient;

namespace SqlServerScriptsExport
{
    public class DatabaseConfig
    {
        public string ServerName { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string OutputPath { get; set; } = "./Scripts";
        public bool UseTrustedConnection { get; set; } = false;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool Encrypt { get; set; } = true;
        public int ConnectionTimeout { get; set; } = 30;

        public string GetConnectionString()
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = ServerName,
                InitialCatalog = DatabaseName,
                IntegratedSecurity = UseTrustedConnection,
                ConnectTimeout = ConnectionTimeout,
                Encrypt = Encrypt
            };

            if (!UseTrustedConnection)
            {
                builder.UserID = Username;
                builder.Password = Password;
            }

            return builder.ConnectionString;
        }
    }
}
