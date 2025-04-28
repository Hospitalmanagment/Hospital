using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("fullName")]
    public string? FullName { get; set; }

    [BsonElement("gender")]
    public string? Gender { get; set; }

    [BsonElement("phoneNumber")]
    public string? PhoneNumber { get; set; }

    [BsonElement("floor")]
    public string? Floor { get; set; }

    [BsonElement("block")]
    public string? Block { get; set; }


    [BsonElement("room")]
    public string? Room { get; set; }

    [BsonElement("reason")]
    public string? Reason { get; set; }

    [BsonElement("entryTime")]
    public string? EntryTime { get; set; }

    [BsonElement("exitTime")]
    public string? ExitTime { get; set; }

    public Feedback? Feedback { get; set; }
}


public class Feedback
{
    public int? StarRating { get; set; } // Nullable, range 1-5
    public string Message { get; set; }
}

// ✅ Model for updating exit time
public class ExitTimeUpdateModel
{
    [JsonProperty("visitId")]
    public string VisitId { get; set; }

    [JsonProperty("exitTime")]
    public string ExitTime { get; set; }
}
