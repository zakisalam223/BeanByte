// Stores/MongoThreadStore.cs
using MongoDB.Driver;
using forum_aspcore.Models;
using forum_aspcore.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace forum_aspcore.Stores
{
    public class MongoThreadStore
    {
        private readonly IMongoCollection<FThread> _threads;

        public MongoThreadStore(DatabaseService databaseService)
        {
            _threads = databaseService.Threads;

        }

        public async Task<IEnumerable<FThread>> GetAllThreadsAsync()
        {
            return await _threads.Find(thread => true).ToListAsync();
        }

        public async Task<FThread> GetThreadByIdAsync(string threadId)
        {
            return await _threads.Find(thread => thread.ThreadID == threadId).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<FThread>> GetThreadsBySectionIdAsync(string sectionId)
        {
            return await _threads.Find(thread => thread.SectionID == sectionId).ToListAsync();
        }

        public async Task<IEnumerable<FThread>> GetThreadsBySearchAsync(string searchTerm, string sectionId)
        {
            return await _threads.Find(thread => 
                thread.SectionID == sectionId && 
                thread.Title.ToLower().Contains(searchTerm.ToLower())
            ).ToListAsync();
        }

        public async Task CreateThreadAsync(FThread thread)
        {
            await _threads.InsertOneAsync(thread);
        }

        public async Task UpdateThreadAsync(string threadId, FThread updatedThread)
        {
            await _threads.ReplaceOneAsync(thread => thread.ThreadID == threadId, updatedThread);
        }

        public async Task DeleteThreadAsync(string threadId)
        {
            await _threads.DeleteOneAsync(thread => thread.ThreadID == threadId);
        }
    }
}
