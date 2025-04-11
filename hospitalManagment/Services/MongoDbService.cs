using MongoDB.Driver;
using MongoDB.Bson;

public class MongoDbService
{
    private readonly IMongoCollection<User> _usersCollection;
    private readonly IMongoCollection<Patient> _patientsCollection;

    public MongoDbService(IMongoDatabase database)
    {
        _usersCollection = database.GetCollection<User>("user");
        _patientsCollection = database.GetCollection<Patient>("patient");

    }
    public IMongoCollection<User> GetUserCollection()
    {
        return _usersCollection;
    }

    public async Task AddUserAsync(User model)
    {
        await _usersCollection.InsertOneAsync(model);
    }

    public async Task AddPatientAsync(Patient model)
    {
        await _patientsCollection.InsertOneAsync(model);
    }

    public async Task<List<User>> GetUsersAsync()
    {
        return await _usersCollection.Find(FilterDefinition<User>.Empty).ToListAsync();

    }

    public async Task<List<Patient>> GetPatientsAsync()
    {
        return await _patientsCollection.Find(FilterDefinition<Patient>.Empty).ToListAsync();

    }

    public async Task<bool> UpdateExitTimeAsync(string visitId, string exitTime)
    {
        if (string.IsNullOrEmpty(exitTime))
        {
            Console.WriteLine("Error: Exit time is empty.");
            return false;
        }

        var filter = Builders<User>.Filter.Eq("_id", ObjectId.Parse(visitId));
        var update = Builders<User>.Update.Set(p => p.ExitTime, exitTime);


        var result = await _usersCollection.UpdateOneAsync(filter, update);
        return result.ModifiedCount > 0;
    }

    public async Task<List<User>> GetPendingVisitsAsync(string fullName)
    {
        var filter = Builders<User>.Filter.And(
            Builders<User>.Filter.Eq(u => u.FullName, fullName),
            Builders<User>.Filter.Or(
                Builders<User>.Filter.Eq(u => u.ExitTime, null),
                Builders<User>.Filter.Eq(u => u.ExitTime, "")
            )
        );

        return await _usersCollection.Find(filter).ToListAsync();
    }

    public async Task<List<User>> GetSortedUsersAsync(string sortBy, string order, string searchName, string exitTimeStatus, string? floor = null, string? room = null)
    {
        var filter = Builders<User>.Filter.Empty; // Default: No filter

        // Apply name search filter if provided
        if (!string.IsNullOrEmpty(searchName))
        {
            filter &= Builders<User>.Filter.Regex(u => u.FullName, new BsonRegularExpression(searchName, "i")); // Case-insensitive search
        }

        // Apply exit time filter
        if (exitTimeStatus == "pending")
        {
            filter &= Builders<User>.Filter.Or(
                Builders<User>.Filter.Eq(u => u.ExitTime, null),
                Builders<User>.Filter.Eq(u => u.ExitTime, "")
            );
        }
        else if (exitTimeStatus == "completed")
        {
            filter &= Builders<User>.Filter.Ne(u => u.ExitTime, null);
            filter &= Builders<User>.Filter.Ne(u => u.ExitTime, "");
        }
        if (!string.IsNullOrEmpty(floor))
        {
            filter &= Builders<User>.Filter.Eq(u => u.Floor, floor);
        }

        if (!string.IsNullOrEmpty(room))
        {
            filter &= Builders<User>.Filter.Eq(u => u.Room, room);
        }

        var sortDefinition = order.ToLower() == "desc"
            ? Builders<User>.Sort.Descending(sortBy)
            : Builders<User>.Sort.Ascending(sortBy);

        return await _usersCollection.Find(filter)
                                     .Sort(sortDefinition)
                                     .ToListAsync();
    }




    public async Task<List<Patient>> GetSortedPatientsAsync(
    string? searchName, string? room, string? condition, string? gender,
    int? minAge, int? maxAge, string sortBy, string order)
    {
        var filter = Builders<Patient>.Filter.Empty;

        // 🔹 Filter by name (case-insensitive)
        if (!string.IsNullOrEmpty(searchName))
        {
            filter &= Builders<Patient>.Filter.Regex("Name", new BsonRegularExpression(searchName, "i"));
        }

        // 🔹 Filter by room number
        if (!string.IsNullOrEmpty(room))
        {
            filter &= Builders<Patient>.Filter.Eq("Room", room);
        }

        // 🔹 Filter by condition (Safe / Not Safe)
        if (!string.IsNullOrEmpty(condition))
        {
            filter &= Builders<Patient>.Filter.Eq("Condition", condition);
        }

        // 🔹 Filter by gender
        if (!string.IsNullOrEmpty(gender))
        {
            filter &= Builders<Patient>.Filter.Eq("Gender", gender);
        }

        // 🔹 Filter by age range
        if (minAge.HasValue)
        {
            filter &= Builders<Patient>.Filter.Gte("Age", minAge.Value);
        }
        if (maxAge.HasValue)
        {
            filter &= Builders<Patient>.Filter.Lte("Age", maxAge.Value);
        }

        // 🔹 Sorting Logic
        var sortDefinition = order.ToLower() == "desc"
            ? Builders<Patient>.Sort.Descending(sortBy)
            : Builders<Patient>.Sort.Ascending(sortBy);

        return await _patientsCollection.Find(filter)
                                        .Sort(sortDefinition)
                                        .ToListAsync();
    }

    public async Task<bool> DeletePatientAsync(string id)
    {
        var result = await _patientsCollection.DeleteOneAsync(p => p.Id == id);
        return result.DeletedCount > 0;
    }




}
