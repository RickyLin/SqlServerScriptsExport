using System;

namespace SqlServerScriptsExport
{
    public static class ArgumentParser
    {
        public static (DatabaseConfig?, AppOptions?) ParseArguments(string[] args)
        {
            if (args.Length == 0)
            {
                // Interactive mode - prompt for connection details
                return GetConfigInteractively();
            }

            // Command line mode
            var config = new DatabaseConfig();
            var options = new AppOptions();
            
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "-s":
                    case "--server":
                        if (i + 1 < args.Length)
                            config.ServerName = args[++i];
                        break;
                    case "-d":
                    case "--database":
                        if (i + 1 < args.Length)
                            config.DatabaseName = args[++i];
                        break;
                    case "-o":
                    case "--output":
                        if (i + 1 < args.Length)
                            config.OutputPath = args[++i];
                        break;
                    case "-t":
                    case "--trusted":
                        config.UseTrustedConnection = true;
                        break;
                    case "-u":
                    case "--username":
                        if (i + 1 < args.Length)
                            config.Username = args[++i];
                        break;
                    case "-p":
                    case "--password":
                        if (i + 1 < args.Length)
                            config.Password = args[++i];
                        break;
                    case "--no-encrypt":
                        config.Encrypt = false;
                        break;
                    case "--verbose":
                        options.Verbose = true;
                        break;
                    case "--quiet":
                        options.Quiet = true;
                        break;
                    case "--no-progress":
                        options.NoProgress = true;
                        break;
                    case "--log-file":
                        if (i + 1 < args.Length)
                            options.LogFilePath = args[++i];
                        break;
                    case "--timeout":
                        if (i + 1 < args.Length && int.TryParse(args[++i], out int timeout))
                            options.ConnectionTimeout = timeout;
                        break;
                    case "--header":
                        options.IncludeScriptHeader = true;
                        break;
                    case "-h":
                    case "--help":
                        options.ShowHelp = true;
                        return (null, options);
                }
            }

            return ValidateConfig(config) ? (config, options) : (null, options);
        }

        private static (DatabaseConfig, AppOptions) GetConfigInteractively()
        {
            var config = new DatabaseConfig();
            var options = new AppOptions();

            Console.WriteLine("Enter database connection details:");
            Console.WriteLine();

            Console.Write("SQL Server name (e.g., localhost, .\\SQLEXPRESS): ");
            config.ServerName = Console.ReadLine()?.Trim() ?? string.Empty;

            Console.Write("Database name: ");
            config.DatabaseName = Console.ReadLine()?.Trim() ?? string.Empty;

            Console.Write("Use Windows Authentication? (y/n) [n]: ");
            var useWindows = Console.ReadLine()?.Trim().ToLower();
            config.UseTrustedConnection = useWindows == "y" || useWindows == "yes";

            if (!config.UseTrustedConnection)
            {
                Console.Write("Username: ");
                config.Username = Console.ReadLine()?.Trim() ?? string.Empty;

                Console.Write("Password: ");
                config.Password = ReadPassword();
            }

            Console.Write("Output directory [./Scripts]: ");
            var outputPath = Console.ReadLine()?.Trim();
            config.OutputPath = string.IsNullOrEmpty(outputPath) ? "./Scripts" : outputPath;

            Console.Write("Enable encryption? (y/n) [y]: ");
            var useEncryption = Console.ReadLine()?.Trim().ToLower();
            config.Encrypt = string.IsNullOrEmpty(useEncryption) || useEncryption == "y" || useEncryption == "yes";

            Console.Write("Include script headers? (y/n) [n]: ");
            var includeHeaders = Console.ReadLine()?.Trim().ToLower();
            options.IncludeScriptHeader = includeHeaders == "y" || includeHeaders == "yes";

            Console.WriteLine();

            return ValidateConfig(config) ? (config, options) : throw new ArgumentException("Invalid configuration provided.");
        }

        private static string ReadPassword()
        {
            var password = string.Empty;
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);

                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
                else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password[0..^1];
                    Console.Write("\b \b");
                }
            }
            while (key.Key != ConsoleKey.Enter);

            Console.WriteLine();
            return password;
        }

        private static bool ValidateConfig(DatabaseConfig config)
        {
            if (string.IsNullOrWhiteSpace(config.ServerName))
            {
                Console.WriteLine("Error: Server name is required.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(config.DatabaseName))
            {
                Console.WriteLine("Error: Database name is required.");
                return false;
            }

            if (!config.UseTrustedConnection)
            {
                if (string.IsNullOrWhiteSpace(config.Username))
                {
                    Console.WriteLine("Error: Username is required when not using Windows Authentication.");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(config.Password))
                {
                    Console.WriteLine("Error: Password is required when not using Windows Authentication.");
                    return false;
                }
            }

            return true;
        }

        public static void ShowUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  SqlServerScriptsExport [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  -s, --server <server>      SQL Server name or instance");
            Console.WriteLine("  -d, --database <database>  Database name");
            Console.WriteLine("  -o, --output <path>        Output directory (default: ./Scripts)");
            Console.WriteLine("  -t, --trusted              Use Windows Authentication");
            Console.WriteLine("  -u, --username <username>  SQL Server username");
            Console.WriteLine("  -p, --password <password>  SQL Server password");
            Console.WriteLine("  --no-encrypt               Disable connection encryption (enabled by default)");
            Console.WriteLine("  --verbose                  Enable verbose logging");
            Console.WriteLine("  --quiet                    Minimize console output");
            Console.WriteLine("  --no-progress              Disable progress reporting");
            Console.WriteLine("  --log-file <path>          Write logs to file");
            Console.WriteLine("  --timeout <seconds>        Connection timeout (default: 30)");
            Console.WriteLine("  --header                   Include script headers in generated files");
            Console.WriteLine("  -h, --help                 Show this help message");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  SqlServerScriptsExport -s localhost -d MyDatabase -t");
            Console.WriteLine("  SqlServerScriptsExport -s .\\SQLEXPRESS -d MyDatabase -u sa -p password");
            Console.WriteLine("  SqlServerScriptsExport -s localhost -d MyDatabase -t --no-encrypt --verbose --log-file export.log");
            Console.WriteLine();
            Console.WriteLine("If no arguments are provided, interactive mode will be used.");
        }
    }
}
