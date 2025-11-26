// Services/DatabaseService.cs
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using forum_aspcore.Models;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver.GridFS;
using Microsoft.VisualBasic;

namespace forum_aspcore.Services
{
    public class DatabaseService
    {
        private readonly IMongoDatabase _database;
        private readonly IGridFSBucket _gridFSBucket;
        private readonly MongoDBSettings settings;

        public DatabaseService(IOptions<MongoDBSettings> mongoDBSettings)
        {
            try
            {
                settings = mongoDBSettings.Value;
                var client = new MongoClient(settings.ConnectionURI);
                _database = client.GetDatabase(settings.DatabaseName);
                _gridFSBucket = new GridFSBucket(_database);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to initialize DatabaseService.", ex);
            }
        }


        public IMongoCollection<FUser> Users => _database.GetCollection<FUser>(settings.UsersCollectionName);
        public IMongoCollection<FThread> Threads => _database.GetCollection<FThread>(settings.ThreadsCollectionName);
        public IMongoCollection<FReply> Replies => _database.GetCollection<FReply>(settings.RepliesCollectionName);
        public IMongoCollection<FInfraction> Infractions => _database.GetCollection<FInfraction>(settings.InfractionsCollectionName);
        public IMongoCollection<FReputationLog> ReputationLogs => _database.GetCollection<FReputationLog>(settings.ReputationLogCollectionName);
        public IMongoCollection<FPrivateMessage> Messages => _database.GetCollection<FPrivateMessage>(settings.MessagesCollectionName);
        public IGridFSBucket GridFSBucket => _gridFSBucket;
        public IMongoCollection<FFile> Files => _database.GetCollection<FFile>(settings.FilesCollectionName);
        public IMongoCollection<FTag> Tags => _database.GetCollection<FTag>(settings.TagsCollectionName);
        public IMongoCollection<FSection> Sections => _database.GetCollection<FSection>("sections");

    }
}
