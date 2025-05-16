using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FlowOrchestrator.Infrastructure.Data.MongoDB.Configuration
{
    /// <summary>
    /// Context class for MongoDB connection management.
    /// </summary>
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        private readonly ILogger<MongoDbContext> _logger;
        private readonly MongoDbOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDbContext"/> class.
        /// </summary>
        /// <param name="options">The MongoDB options.</param>
        /// <param name="logger">The logger.</param>
        public MongoDbContext(IOptions<MongoDbOptions> options, ILogger<MongoDbContext> logger)
        {
            _options = options.Value;
            _logger = logger;

            try
            {
                var clientSettings = MongoClientSettings.FromConnectionString(_options.ConnectionString);
                
                // Configure client settings
                clientSettings.MaxConnectionPoolSize = _options.MaxConnectionPoolSize;
                clientSettings.ConnectTimeout = TimeSpan.FromSeconds(_options.ConnectionTimeoutSeconds);
                clientSettings.SocketTimeout = TimeSpan.FromSeconds(_options.SocketTimeoutSeconds);
                clientSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(_options.ServerSelectionTimeoutSeconds);
                clientSettings.UseTls = _options.UseSsl;
                clientSettings.RetryWrites = _options.RetryWrites;
                clientSettings.RetryReads = _options.RetryReads;

                var client = new MongoClient(clientSettings);
                _database = client.GetDatabase(_options.DatabaseName);

                _logger.LogInformation("MongoDB connection established to database: {DatabaseName}", _options.DatabaseName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to establish MongoDB connection");
                throw;
            }
        }

        /// <summary>
        /// Gets the MongoDB database.
        /// </summary>
        public IMongoDatabase Database => _database;

        /// <summary>
        /// Gets a collection from the database.
        /// </summary>
        /// <typeparam name="T">The type of documents in the collection.</typeparam>
        /// <param name="collectionName">The name of the collection.</param>
        /// <returns>The MongoDB collection.</returns>
        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return _database.GetCollection<T>(collectionName);
        }
    }
}
