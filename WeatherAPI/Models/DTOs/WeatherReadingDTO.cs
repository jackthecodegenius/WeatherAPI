using MongoDB.Bson.Serialization.Attributes;

namespace WeatherAPI.Models.DTOs
{
    public class WeatherReadingDTO
    {
        [BsonElement("Precipitation mm/h")]
        public double? Precipitation { get; set; }
        [BsonElement("Temperature (°C)")]
        public double? Temperature { get; set; }

        [BsonElement("Atmospheric Pressure (kPa)")]
        public double? Atmospheric { get; set; }

        [BsonElement("Solar Radiation (W/m2)")]
        public double? Solar { get; set; }

        public WeatherReadingDTO(double? temperature, double? atmospheric, double? solar, double? precipitation)
        {
            Temperature = temperature;
            Atmospheric = atmospheric;
            Solar = solar;
            Precipitation = precipitation;
        }
    }
}
