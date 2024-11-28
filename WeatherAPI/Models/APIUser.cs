using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Data;
using System.Text.Json.Serialization;

namespace WeatherAPI.Models
{
    public class APIUser
    {

        [BsonId]
        [JsonIgnore]
        public ObjectId _id { get; set; }
        public string ObjId => _id.ToString();
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = Roles.STUDENT.ToString();
        public string ApiKey { get; set; } = string.Empty;
        public DateTime Created { get; set; }
        public DateTime LastAccess { get; set; }
    }
}
