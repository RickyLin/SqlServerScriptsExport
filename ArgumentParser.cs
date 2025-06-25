using System;

namespace SqlServerScriptsExport
{
    public static class ArgumentParser
    {
        public static DatabaseConfig? ParseArguments(string[] args)
        {
            if (args.Length == 0)
            {
                // Interactive mode - prompt for connection details
                return GetConfigInteractively();
            }

            // Command line mode
            var config = new DatabaseConfig();
            
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
                    case "-h":
                    case "--help":
                        return null;
                }
            }

            return ValidateConfig(config) ? config : null;
        }

        private static DatabaseConfig GetConfigInteractively()
        {
            var config = new DatabaseConfig();

            Console.WriteLine("Enter database connection details:");
            Console.WriteLine();

            Console.Write("SQL Server name (e.g., localhost, .\\SQLEXPRESS): ");
            config.ServerName = Console.ReadLine()?.Trim() ?? string.Empty;

            Console.Write("Database name: ");
            config.DatabaseName = Console.ReadLine()?.Trim() ?? string.Empty;

            Console.Write("Use Windows Authentication? (y/n) [y]: ");
            var useWindows = Console.ReadLine()?.Trim().ToLower();
            config.UseTrustedConnection = string.IsNullOrEmpty(useWindows) || useWindows == "y" || useWindows == "yes";

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

            Console.WriteLine();

            return ValidateConfig(config) ? config : throw new ArgumentException("Invalid configuration provided.");
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
            Console.WriteLine("  -h, --help                 Show this help message");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  SqlServerScriptsExport -s localhost -d MyDatabase -t");
            Console.WriteLine("  SqlServerScriptsExport -s .\\SQLEXPRESS -d MyDatabase -u sa -p password");
            Console.WriteLine();
            Console.WriteLine("If no arguments are provided, interactive mode will be used.");
        }
    }
}
