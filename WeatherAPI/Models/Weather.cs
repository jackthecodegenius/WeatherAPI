using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace WeatherAPI.Models
{
    public class Weather
    {
        [BsonId]
        [JsonIgnore]

        public ObjectId _id { get; set; }
        public string ObjId => _id.ToString();
        [BsonElement("Device Name")]
        public string DeviceName { get; set; } = string.Empty;
        [BsonElement("Precipitation mm/h")]
        public double Precipitation { get; set; }
        [BsonElement("Time")]
        public DateTime Time { get; set; }
        [BsonElement("Latitude")]
        public double Latitutude { get; set; }
        [BsonElement("Longitude")]
        public double Longitude { get; set; }
        [BsonElement("Temperature (°C)")]
        public double? Temperature { get; set; }
        [BsonElement("Atmospheric Pressure (kPa)")]
        public double? Atmospheric { get; set; }
        [BsonElement("Max Wind Speed (m/s)")]
        public double? MaxWind { get; set; }
        [BsonElement("Solar Radiation (W/m2)")]
        public double? Solar { get; set; }
        [BsonElement("Vapor Pressure (kPa)")]
        public double? Vapor { get; set; }
        [BsonElement("Humidity (%)")]
        public double? Humidity { get; set; }
        [BsonElement("Wind Direction (°)")]
        public double? WindDirection { get; set; } 
        


    }
}
