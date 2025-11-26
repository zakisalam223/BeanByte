using MongoDB.Driver;
using forum_aspcore.Models;
using forum_aspcore.Services;

namespace forum_aspcore.Stores
{
    public class MongoSectionStore
    {
        private readonly IMongoCollection<FSection> _sections;

        public MongoSectionStore(DatabaseService databaseService)
        {
            _sections = databaseService.Sections;
        }

        public async Task<IEnumerable<FSection>> GetAllSectionsAsync()
        {
            return await _sections.Find(section => true).ToListAsync();
        }

        public async Task<FSection> GetSectionByIdAsync(string sectionId)
        {
            return await _sections.Find(section => section.SectionID == sectionId).FirstOrDefaultAsync();
        }

         public async Task<IEnumerable<FSection>> GetSectionBySearchAsync(string searchTerm)
        {
            return await _sections.Find(section => section.SectionName.ToLower().Contains(searchTerm.ToLower())).ToListAsync();
        }

        public async Task CreateSectionAsync(FSection section)
        {
            await _sections.InsertOneAsync(section);
        }

        public async Task UpdateSectionAsync(string sectionId, FSection updatedSection)
        {
            await _sections.ReplaceOneAsync(section => section.SectionID == sectionId, updatedSection);
        }

        public async Task DeleteSectionAsync(string sectionId)
        {
            await _sections.DeleteOneAsync(section => section.SectionID == sectionId);
        }
    }
}
