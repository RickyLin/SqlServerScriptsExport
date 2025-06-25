using System;

namespace SqlServerScriptsExport
{
    public class ProgressReporter
    {
        private readonly bool _showProgress;
        private readonly Logger _logger;
        private int _totalItems;
        private int _processedItems;
        private string _currentOperation = string.Empty;
        private DateTime _startTime;

        public ProgressReporter(bool showProgress, Logger logger)
        {
            _showProgress = showProgress;
            _logger = logger;
        }

        public void StartOperation(string operation, int totalItems)
        {
            _currentOperation = operation;
            _totalItems = totalItems;
            _processedItems = 0;
            _startTime = DateTime.UtcNow;

            if (_showProgress)
            {
                _logger.LogInfo($"Starting {operation} ({totalItems} items)...");
            }
        }

        public void ReportProgress(string? itemName = null)
        {
            _processedItems++;

            if (_showProgress && _totalItems > 0)
            {
                var percentage = (_processedItems * 100) / _totalItems;
                var elapsed = DateTime.UtcNow - _startTime;
                var estimatedTotal = _processedItems > 0 ? 
                    TimeSpan.FromTicks(elapsed.Ticks * _totalItems / _processedItems) : 
                    TimeSpan.Zero;
                var remaining = estimatedTotal - elapsed;

                var progressBar = GenerateProgressBar(percentage);
                var message = $"{progressBar} {percentage}% ({_processedItems}/{_totalItems})";
                
                if (!string.IsNullOrEmpty(itemName))
                {
                    message += $" - {itemName}";
                }

                if (remaining.TotalSeconds > 0 && _processedItems > 1)
                {
                    message += $" - ETA: {remaining:mm\\:ss}";
                }

                // Use \r to overwrite the same line
                Console.Write($"\r{message}".PadRight(Console.WindowWidth - 1));
            }
        }

        public void CompleteOperation()
        {
            if (_showProgress)
            {
                var elapsed = DateTime.UtcNow - _startTime;
                Console.WriteLine(); // Move to next line
                _logger.LogInfo($"Completed {_currentOperation} in {elapsed:mm\\:ss\\.fff}");
            }
        }

        public void ReportSummary(int viewsProcessed, int proceduresProcessed, int functionsProcessed, int triggersProcessed, int filesCreated)
        {
            _logger.LogInfo("Summary:");
            _logger.LogInfo($"  Views processed: {viewsProcessed}");
            _logger.LogInfo($"  Stored procedures processed: {proceduresProcessed}");
            _logger.LogInfo($"  Functions processed: {functionsProcessed}");
            _logger.LogInfo($"  Triggers processed: {triggersProcessed}");
            _logger.LogInfo($"  Total files created: {filesCreated}");
        }

        private static string GenerateProgressBar(int percentage)
        {
            const int barLength = 20;
            var filledLength = (percentage * barLength) / 100;
            var bar = new string('█', filledLength) + new string('░', barLength - filledLength);
            return $"[{bar}]";
        }
    }
}
