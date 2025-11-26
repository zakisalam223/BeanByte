using MongoDB.Driver;
using forum_aspcore.Models;
using forum_aspcore.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace forum_aspcore.Stores
{
    public class MongoPrivateMessageStore
    {
        private readonly IMongoCollection<FPrivateMessage> _messages;

       public MongoPrivateMessageStore(DatabaseService databaseService){
            _messages = databaseService.Messages;
       }

       public async Task<IEnumerable<FPrivateMessage>> GetMessagesForUserAsync(string userId)
       {
           return await _messages.Find(message => message.RecipientID == userId).ToListAsync();
       }

        public async Task<FPrivateMessage> GetMessageByIdAsync(string messageId)
        {
            return await _messages.Find(message => message.MessageID == messageId).FirstOrDefaultAsync();
        }

       public async Task CreateMessageAsync(FPrivateMessage message)
        {
            await _messages.InsertOneAsync(message);
        }

        public async Task UpdateMessageAsync(FPrivateMessage message)
        {
            await _messages.ReplaceOneAsync(m => m.MessageID == message.MessageID, message);
        }

        public async Task<IEnumerable<FPrivateMessage>> GetSentMessagesAsync(string userId)
        {
            return await _messages.Find(message => message.SenderID == userId).ToListAsync();
        }
    }
}