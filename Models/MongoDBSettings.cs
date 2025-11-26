// Models/MongoDBSettings.cs
namespace forum_aspcore.Models
{
    public class MongoDBSettings
    {
        public string ConnectionURI { get; set; }
        public string DatabaseName { get; set; }
        public string InfractionsCollectionName { get; set; }
        public string MessagesCollectionName { get; set; }
        public string TagsCollectionName { get; set; }
        public string ThreadsCollectionName { get; set; }
        public string UsersCollectionName { get; set; }
        public string RepliesCollectionName { get; set; }
        public string FilesCollectionName { get; set; }
        public string ReputationLogCollectionName { get; set; }
    }
}
