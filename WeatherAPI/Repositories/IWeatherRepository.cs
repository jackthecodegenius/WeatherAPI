using WeatherAPI.Models;
using WeatherAPI.Models.DTOs;

namespace WeatherAPI.Repositories
{
    public interface IWeatherRepository
    {
        // basic CRUD operations
        List<Weather> GetAllWeather();
        Weather GetWeatherByid(string id);
        void PostWeather(Weather weather);
        void UpdateWeather(string id, Weather weather);
        void DeleteWeather(string id);
        // Weather WeatherToBeSaved { get; set; } 

        //other CRUD operations
        List<Weather> SearchWeather(string search);
        WeatherReadingDTO FindReadingByDeviceNameAndDate(string deviceName, DateTime date);

        //Bulk CRUD operations 
        void PostManyWeather(List<Weather> weatherList);
        void DeleteByDate(DateTime start, DateTime end);
        void UpdateBasedOnDate(DateTime start, DateTime end, string devicename);
        void UpdatePrecipitationValueById(string id, double newPrecipitationValue);
        WeatherPrecipititationDTO GetMaxPrecipitationForSensor(string deviceName);

        List<WeatherTemperatureDTO> GetMaxTemperatureForSensors(DateTime start, DateTime end);
        //Aggregation Operations

        List<WeatherPresentationDTO> FindNewestWeatherPerTemperature();


    }
}
