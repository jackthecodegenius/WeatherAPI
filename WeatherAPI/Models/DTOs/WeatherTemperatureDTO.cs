using MongoDB.Bson.Serialization.Attributes;

namespace WeatherAPI.Models.DTOs
{
    public class WeatherTemperatureDTO
    {
        [BsonElement("Device Name")]
        public string DeviceName { get; set; } = string.Empty;
        [BsonElement("Time")]
        public DateTime Created { get; set; }
        [BsonElement("Temperature (°C)")]
        public double? Temperature { get; set; }

        public WeatherTemperatureDTO(string deviceName, DateTime time, double temperature)
        {
            DeviceName = deviceName;
            Created = time;
            Temperature = temperature;
        }


    }
}
