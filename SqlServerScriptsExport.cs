using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace SqlServerScriptsExport
{
    public class SqlServerScriptsExport
    {
        private readonly DatabaseConfig _config;
        private readonly Logger _logger;
        private readonly ProgressReporter _progressReporter;

        public SqlServerScriptsExport(DatabaseConfig config, Logger logger, ProgressReporter progressReporter)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _progressReporter = progressReporter ?? throw new ArgumentNullException(nameof(progressReporter));
        }

        public async Task GenerateAllScriptsAsync()
        {
            _logger.LogInfo($"Connecting to SQL Server: {_config.ServerName}");
            _logger.LogInfo($"Database: {_config.DatabaseName}");
            _logger.LogInfo($"Output directory: {_config.OutputPath}");
            _logger.LogInfo("");

            try
            {
                // Test connection first
                await TestConnectionAsync();

                // Create output directory structure
                CreateDirectoryStructure();

                // Extract all database objects
                var databaseObjects = await ExtractDatabaseObjectsAsync();

                // Generate script files
                await GenerateScriptFilesAsync(databaseObjects);

                // Report summary
                ReportSummary(databaseObjects);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to generate scripts", ex);
                throw;
            }
        }

        private async Task TestConnectionAsync()
        {
            _logger.LogInfo("Testing database connection...");
            
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString());
                await connection.OpenAsync();
                
                _logger.LogInfo("Database connection successful");
                _logger.LogDebug($"Server version: {connection.ServerVersion}");
                _logger.LogDebug($"Database: {connection.Database}");
            }
            catch (SqlException ex)
            {
                var errorMessage = ex.Number switch
                {
                    2 => "SQL Server not found or not accessible. Check server name and network connectivity.",
                    18456 => "Login failed. Check username and password.",
                    4060 => "Database not found. Check database name.",
                    -2 => "Connection timeout. Check server name and network connectivity.",
                    _ => $"SQL Server error {ex.Number}: {ex.Message}"
                };
                
                throw new InvalidOperationException(errorMessage, ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to connect to database: {ex.Message}", ex);
            }
        }

        private void CreateDirectoryStructure()
        {
            _logger.LogInfo("Creating output directory structure...");
            
            var directories = new[]
            {
                _config.OutputPath,
                Path.Combine(_config.OutputPath, "Views"),
                Path.Combine(_config.OutputPath, "StoredProcedures"),
                Path.Combine(_config.OutputPath, "Functions_TableValued"),
                Path.Combine(_config.OutputPath, "Functions_ScalarValued")
            };

            foreach (var directory in directories)
            {
                try
                {
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                        _logger.LogDebug($"Created directory: {directory}");
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to create directory '{directory}': {ex.Message}", ex);
                }
            }
        }

        private async Task<List<DatabaseObject>> ExtractDatabaseObjectsAsync()
        {
            _logger.LogInfo("Extracting database object definitions...");
            
            var objects = new List<DatabaseObject>();
            
            using var connection = new SqlConnection(_config.GetConnectionString());
            await connection.OpenAsync();

            // Extract different types of objects
            objects.AddRange(await ExtractViewsAsync(connection));
            objects.AddRange(await ExtractStoredProceduresAsync(connection));
            objects.AddRange(await ExtractFunctionsAsync(connection));
            objects.AddRange(await ExtractTriggersAsync(connection));

            _logger.LogInfo($"Found {objects.Count} database objects");
            return objects;
        }

        private async Task<List<DatabaseObject>> ExtractViewsAsync(SqlConnection connection)
        {
            const string query = @"
                SELECT 
                    o.name,
                    s.name as schema_name,
                    m.definition,
                    o.create_date,
                    o.modify_date
                FROM sys.views v
                INNER JOIN sys.objects o ON v.object_id = o.object_id
                INNER JOIN sys.schemas s ON o.schema_id = s.schema_id
                INNER JOIN sys.sql_modules m ON o.object_id = m.object_id
                WHERE o.is_ms_shipped = 0
                ORDER BY s.name, o.name";

            return await ExecuteObjectQuery(connection, query, DatabaseObjectType.View, "views");
        }

        private async Task<List<DatabaseObject>> ExtractStoredProceduresAsync(SqlConnection connection)
        {
            const string query = @"
                SELECT 
                    o.name,
                    s.name as schema_name,
                    m.definition,
                    o.create_date,
                    o.modify_date
                FROM sys.procedures p
                INNER JOIN sys.objects o ON p.object_id = o.object_id
                INNER JOIN sys.schemas s ON o.schema_id = s.schema_id
                INNER JOIN sys.sql_modules m ON o.object_id = m.object_id
                WHERE o.is_ms_shipped = 0
                ORDER BY s.name, o.name";

            return await ExecuteObjectQuery(connection, query, DatabaseObjectType.StoredProcedure, "stored procedures");
        }

        private async Task<List<DatabaseObject>> ExtractFunctionsAsync(SqlConnection connection)
        {
            const string query = @"
                SELECT 
                    o.name,
                    s.name as schema_name,
                    m.definition,
                    o.create_date,
                    o.modify_date,
                    o.type
                FROM sys.objects o
                INNER JOIN sys.schemas s ON o.schema_id = s.schema_id
                INNER JOIN sys.sql_modules m ON o.object_id = m.object_id
                WHERE o.type IN ('FN', 'TF', 'IF') -- Scalar, Table-valued, Inline Table-valued
                AND o.is_ms_shipped = 0
                ORDER BY s.name, o.name";

            var objects = new List<DatabaseObject>();
            
            _progressReporter.StartOperation("Extracting functions", 0);
            
            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                var objectType = reader.GetString("type") == "FN" ? 
                    DatabaseObjectType.ScalarFunction : 
                    DatabaseObjectType.TableValuedFunction;

                var obj = new DatabaseObject
                {
                    Name = reader.GetString("name"),
                    Schema = reader.GetString("schema_name"),
                    Definition = reader.GetString("definition"),
                    CreatedDate = reader.GetDateTime("create_date"),
                    ModifiedDate = reader.GetDateTime("modify_date"),
                    ObjectType = objectType.ToString()
                };

                objects.Add(obj);
                _progressReporter.ReportProgress(obj.Name);
            }
            
            _progressReporter.CompleteOperation();
            _logger.LogDebug($"Extracted {objects.Count} functions");
            
            return objects;
        }

        private async Task<List<DatabaseObject>> ExtractTriggersAsync(SqlConnection connection)
        {
            const string query = @"
                SELECT 
                    o.name,
                    s.name as schema_name,
                    m.definition,
                    o.create_date,
                    o.modify_date,
                    OBJECT_NAME(t.parent_id) as parent_table
                FROM sys.triggers t
                INNER JOIN sys.objects o ON t.object_id = o.object_id
                INNER JOIN sys.schemas s ON o.schema_id = s.schema_id
                INNER JOIN sys.sql_modules m ON o.object_id = m.object_id
                WHERE o.is_ms_shipped = 0
                ORDER BY OBJECT_NAME(t.parent_id), o.name";

            var objects = new List<DatabaseObject>();
            
            _progressReporter.StartOperation("Extracting triggers", 0);
            
            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                var obj = new DatabaseObject
                {
                    Name = reader.GetString("name"),
                    Schema = reader.GetString("schema_name"),
                    Definition = reader.GetString("definition"),
                    CreatedDate = reader.GetDateTime("create_date"),
                    ModifiedDate = reader.GetDateTime("modify_date"),
                    ObjectType = DatabaseObjectType.Trigger.ToString(),
                    ParentTable = reader.IsDBNull("parent_table") ? null : reader.GetString("parent_table")
                };

                objects.Add(obj);
                _progressReporter.ReportProgress(obj.Name);
            }
            
            _progressReporter.CompleteOperation();
            _logger.LogDebug($"Extracted {objects.Count} triggers");
            
            return objects;
        }

        private async Task<List<DatabaseObject>> ExecuteObjectQuery(SqlConnection connection, string query, DatabaseObjectType objectType, string objectTypeName)
        {
            var objects = new List<DatabaseObject>();
            
            _progressReporter.StartOperation($"Extracting {objectTypeName}", 0);
            
            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                var obj = new DatabaseObject
                {
                    Name = reader.GetString("name"),
                    Schema = reader.GetString("schema_name"),
                    Definition = reader.GetString("definition"),
                    CreatedDate = reader.GetDateTime("create_date"),
                    ModifiedDate = reader.GetDateTime("modify_date"),
                    ObjectType = objectType.ToString()
                };

                objects.Add(obj);
                _progressReporter.ReportProgress(obj.Name);
            }
            
            _progressReporter.CompleteOperation();
            _logger.LogDebug($"Extracted {objects.Count} {objectTypeName}");
            
            return objects;
        }

        private async Task GenerateScriptFilesAsync(List<DatabaseObject> objects)
        {
            _logger.LogInfo("Generating script files...");
            
            _progressReporter.StartOperation("Generating script files", objects.Count);
            
            var filesCreated = 0;
            
            foreach (var obj in objects)
            {
                try
                {
                    var objectType = Enum.Parse<DatabaseObjectType>(obj.ObjectType);
                    var outputPath = FileHelper.GetOutputPathForObjectType(_config.OutputPath, objectType, obj.ParentTable);
                    var fileName = FileHelper.SanitizeFileName(obj.Name) + ".sql";
                    var filePath = Path.Combine(outputPath, fileName);

                    await FileHelper.WriteScriptFileAsync(filePath, obj.Definition, obj.Name, obj.ObjectType, _config.DatabaseName, _logger);
                    
                    filesCreated++;
                    _progressReporter.ReportProgress(obj.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to generate script for {obj.ObjectType} '{obj.Name}'", ex);
                    throw;
                }
            }
            
            _progressReporter.CompleteOperation();
            _logger.LogInfo($"Successfully created {filesCreated} script files");
        }

        private void ReportSummary(List<DatabaseObject> objects)
        {
            var viewsCount = objects.Count(o => o.ObjectType == DatabaseObjectType.View.ToString());
            var proceduresCount = objects.Count(o => o.ObjectType == DatabaseObjectType.StoredProcedure.ToString());
            var functionsCount = objects.Count(o => o.ObjectType == DatabaseObjectType.ScalarFunction.ToString() || 
                                                   o.ObjectType == DatabaseObjectType.TableValuedFunction.ToString());
            var triggersCount = objects.Count(o => o.ObjectType == DatabaseObjectType.Trigger.ToString());
            
            _progressReporter.ReportSummary(viewsCount, proceduresCount, functionsCount, triggersCount, objects.Count);
        }
    }
}
