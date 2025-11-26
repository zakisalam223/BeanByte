// Stores/MongoUserStore.cs
using MongoDB.Driver;
using forum_aspcore.Models;
using forum_aspcore.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace forum_aspcore.Stores
{
    public class MongoUserStore
    {
        private readonly IMongoCollection<FUser> _users;

        public MongoUserStore(DatabaseService databaseService)
        {
            _users = databaseService.Users;
        }

        public async Task<IEnumerable<FUser>> GetAllUsersAsync()
        {
            return await _users.Find(user => true).ToListAsync();
        }

        public async Task<IEnumerable<FUser>> GetUsersByIdsAsync(IEnumerable<string> userIds)
        {
            var filter = Builders<FUser>.Filter.In(user => user.UserID, userIds);
            return await _users.Find(filter).ToListAsync();
        }


        public async Task<FUser> GetUserByIdAsync(string userId)
        {
            return await _users.Find(user => user.UserID == userId).FirstOrDefaultAsync();
        }

        public async Task<FUser> GetUserByUsernameAsync(string username)
        {
            return await _users.Find(user => user.Username == username).FirstOrDefaultAsync();
        }

        public async Task<FUser> GetUserByEmailAsync(string email)
        {
            return await _users.Find(user => user.Email == email).FirstOrDefaultAsync();
        }

        public async Task CreateUserAsync(FUser user)
        {
            await _users.InsertOneAsync(user);
        }

        public async Task UpdateUserAsync(string userId, FUser updatedUser)
        {
            await _users.ReplaceOneAsync(user => user.UserID == userId, updatedUser);
        }

        public async Task DeleteUserAsync(string userId)
        {
            await _users.DeleteOneAsync(user => user.UserID == userId);
        }

        public async Task UpdateUserAsync(FUser user)
        {
            var filter = Builders<FUser>.Filter.Eq(u => u.UserID, user.UserID);
            await _users.ReplaceOneAsync(filter, user);
        }
    }
}
