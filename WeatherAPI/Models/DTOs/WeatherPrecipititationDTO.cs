using MongoDB.Bson.Serialization.Attributes;

namespace WeatherAPI.Models.DTOs
{
    public class WeatherPrecipititationDTO
    {
        [BsonElement("Device Name")]
        public string DeviceName { get; set; } = string.Empty;
        [BsonElement("Time")]
        public DateTime Created { get; set; }
        [BsonElement("Precipitation mm/h")]
        public double Precipitation { get; set; }

        public WeatherPrecipititationDTO(string deviceName, DateTime time, double precipitation)
        {
            DeviceName = deviceName;
            Created = time;
            Precipitation = precipitation;
        }
    }

}
