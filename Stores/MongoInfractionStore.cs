using MongoDB.Driver;
using forum_aspcore.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using forum_aspcore.Services;

namespace forum_aspcore.Stores
{
    public class MongoInfractionStore
    {
        private readonly IMongoCollection<FInfraction> _infractions;

        public MongoInfractionStore(DatabaseService databaseService)
        {
            _infractions = databaseService.Infractions;
        }

        public async Task CreateInfractionAsync(FInfraction infraction)
        {
            await _infractions.InsertOneAsync(infraction);
        }

        public async Task<List<FInfraction>> GetInfractionsByUserAsync(string userId)
        {
            return await _infractions.Find(i => i.GivenToUserID == userId).ToListAsync();
        }

    }
}