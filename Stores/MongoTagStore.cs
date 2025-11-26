using MongoDB.Driver;
using forum_aspcore.Models;
using forum_aspcore.Services;

namespace forum_aspcore.Stores
{
    public class MongoTagStore
    {
        private readonly IMongoCollection<FTag> _tags;

        public MongoTagStore(DatabaseService databaseService)
        {
            _tags = databaseService.Tags;
        }

        public async Task<IEnumerable<FTag>> GetAllTagsAsync()
        {
            return await _tags.Find(tag => true).ToListAsync();
        }

        public async Task<FTag> GetTagByIdAsync(string tagId)
        {
            return await _tags.Find(tag => tag.TagID == tagId).FirstOrDefaultAsync();
        }

        public async Task<FTag> GetTagByNameAsync(string name)
        {
            return await _tags.Find(tag => tag.Name == name).FirstOrDefaultAsync();
        }

        public async Task CreateTagAsync(FTag tag)
        {
            await _tags.InsertOneAsync(tag);
        }

        public async Task DeleteTagAsync(string tagId)
        {
            await _tags.DeleteOneAsync(tag => tag.TagID == tagId);
        }
    }
}