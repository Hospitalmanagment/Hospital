using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Patient
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    [BsonElement("Name")]
    public string Name { get; set; }

    [BsonElement("Age")]
    public int Age { get; set; }

    [BsonElement("Gender")]
    public string Gender { get; set; }

    [BsonElement("Condition")]
    public string Condition { get; set; }

    public string Floor { get; set; }

    [BsonElement("Room")]
    public string Room { get; set; }
}
