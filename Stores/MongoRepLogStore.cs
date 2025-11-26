using forum_aspcore.Models;
using forum_aspcore.Services;
using MongoDB.Driver;

public class MongoRepLogStore
{
    private readonly IMongoCollection<FReputationLog> _logs;

    public MongoRepLogStore(DatabaseService databaseService)
    {
        _logs = databaseService.ReputationLogs;
    }

    public async Task<FReputationLog> GetLogAsync(string initiatorUserId, string targetUserId)
    {
        var filter = Builders<FReputationLog>.Filter.And(
            Builders<FReputationLog>.Filter.Eq(l => l.InitiatorUserID, initiatorUserId),
            Builders<FReputationLog>.Filter.Eq(l => l.TargetUserID, targetUserId)
        );
        return await _logs.Find(filter).FirstOrDefaultAsync();
    }

    public async Task AddOrUpdateLogAsync(FReputationLog log)
    {
        var filter = Builders<FReputationLog>.Filter.And(
            Builders<FReputationLog>.Filter.Eq(l => l.InitiatorUserID, log.InitiatorUserID),
            Builders<FReputationLog>.Filter.Eq(l => l.TargetUserID, log.TargetUserID)
        );
        await _logs.ReplaceOneAsync(filter, log, new ReplaceOptions { IsUpsert = true });
    }
}
