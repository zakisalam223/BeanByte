using MongoDB.Driver;
using forum_aspcore.Models;
using forum_aspcore.Services;
using MongoDB.Driver.GridFS;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace forum_aspcore.Stores
{
    public class MongoFileStore
    {
        private readonly IMongoCollection<FFile> _files;
        private readonly IGridFSBucket _gridFS;

        public MongoFileStore(DatabaseService databaseService)
        {
            _files = databaseService.Files;
            _gridFS = databaseService.GridFSBucket;
        }

        public async Task<IEnumerable<FFile>> GetAllFilesAsync()
        {
            return await _files.Find(file => true).ToListAsync();
        }

        public async Task<FFile> GetFileByIdAsync(string fileId)
        {
            return await _files.Find(file => file.FileID == fileId).FirstOrDefaultAsync();
        }

        public async Task<ObjectId> SaveToGridFSAsync(Stream fileStream, string filename, string contentType)
        {
            try
            {
                var uploadOptions = new GridFSUploadOptions
                {
                    Metadata = new BsonDocument
            {
                { "ContentType", contentType },
                { "UploadDate", DateTime.UtcNow }
            }
                };

                return await _gridFS.UploadFromStreamAsync(filename, fileStream, uploadOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GridFS upload error: {ex.Message}");
                throw;
            }
        }


        public async Task SaveFileDocumentAsync(FFile fileDoc)
        {
            try
            {
                await _files.InsertOneAsync(fileDoc);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"File document save error: {ex.Message}");
    
                if (fileDoc.GFSID != default)
                {
                    try
                    {
                        await _gridFS.DeleteAsync(fileDoc.GFSID);
                    }
                    catch { // Error 
                     }
                }
                throw;
            }
        }

        public async Task<byte[]> DownloadFileAsync(ObjectId gfsId)
        {
            return await _gridFS.DownloadAsBytesAsync(gfsId);
        }

        public async Task DeleteFileAsync(string fileId, ObjectId gfsId)
        {
            await _files.DeleteOneAsync(file => file.FileID == fileId);
            await _gridFS.DeleteAsync(gfsId);
        }

        public async Task<IEnumerable<FFile>> GetFilesByUserIdAsync(string userId)
        {
            return await _files.Find(file => file.UploadedBy == userId).ToListAsync();
        }

        public async Task<IEnumerable<FFile>> GetPendingFilesAsync()
        {
            var filter = Builders<FFile>.Filter.Eq(file => file.Status, false);
            return await _files.Find(filter).ToListAsync();
        }

        public async Task<bool> ApproveFileAsync(string fileId)
        {
            var filter = Builders<FFile>.Filter.Eq(f => f.FileID, fileId);
            var update = Builders<FFile>.Update.Set(f => f.Status, true);
            var result = await _files.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DenyFileAsync(string fileId)
        {
            var file = await GetFileByIdAsync(fileId);
            if (file == null)
            {
                return false;
            }

            await _gridFS.DeleteAsync(file.GFSID);

            var deleteResult = await _files.DeleteOneAsync(f => f.FileID == fileId);
            return deleteResult.DeletedCount > 0;
        }

        public async Task<FFile> GetFileByGFSIdAsync(ObjectId gfsId)
        {
            return await _files.Find(file => file.GFSID == gfsId).FirstOrDefaultAsync();
        }
    }
}