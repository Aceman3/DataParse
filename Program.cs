using System;
using System.IO;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using DataParse;

namespace ReadATextFile
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddLogging(config =>
                    {
                        // For simplicity, we're adding console logging. In a real-world scenario, you might use other loggers.
                        config.AddConsole();
                    });
                    services.AddHostedService<FileProcessorService>();
                });
    }

    public class FileProcessorService : BackgroundService
    {
        private const string rootFolder = @"C:\Users\hfarrell\Desktop\TestTextFiles\";
        private const string connectionString = "Data Source=oh-dc1;Initial Catalog=SEC_Common;User Id=PIFUser;Password=UserPIF;MultipleActiveResultSets=True";
        private readonly ILogger<FileProcessorService> _logger;

        public FileProcessorService(ILogger<FileProcessorService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("FileProcessorService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    foreach (string filePath in Directory.EnumerateFiles(rootFolder, "*.txt"))
                    {
                        Product product = ParseProductFromFile(filePath);

                        if (product != null)
                        {
                            SaveProductToDatabase(product);
                            File.Delete(filePath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"An error occurred: {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }

            _logger.LogInformation("FileProcessorService is stopping.");
        }

        private Product ParseProductFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            string fileContent = File.ReadAllText(filePath);
            _logger.LogInformation($"Text file being read: {fileContent}");

            string[] values = fileContent.Split(',');

            if (values.Length != 5)
            {
                _logger.LogWarning($"Invalid data format in {filePath}. Expected 5 values.");
                return null;
            }

            return new Product
            {
                GoodBad = values[0],
                DateTime = DateTime.Parse(values[1]),
                Color = values[2],
                Processed = Boolean.Parse(values[3]),
                PercentGood = Decimal.Parse(values[4])
            };
        }

        private void SaveProductToDatabase(Product product)
        {
            _logger.LogInformation($"Saving product to database: Good/Bad? {product.GoodBad}, Date/Time: {product.DateTime}, Color: {product.Color}, Processed: {product.Processed}, PercentageGood: {product.PercentGood}");

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand("dbo.Procedure", connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@GoodBad", product.GoodBad);
                command.Parameters.AddWithValue("@DateTime", product.DateTime);
                command.Parameters.AddWithValue("@Color", product.Color);
                command.Parameters.AddWithValue("@Processed", product.Processed);
                command.Parameters.AddWithValue("@PercentGood", product.PercentGood);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}
