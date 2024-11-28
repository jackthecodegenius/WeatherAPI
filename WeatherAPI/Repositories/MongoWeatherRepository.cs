using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.RegularExpressions;
using WeatherAPI.Models;
using WeatherAPI.Models.DTOs;
using WeatherAPI.Services;

namespace WeatherAPI.Repositories
{
    public class MongoWeatherRepository : IWeatherRepository
    {

        //Variable to store a reference to our Mongo DB collection.
        private readonly IMongoCollection<Weather> _weather;
        //Request the connection builder from the dependency injection by
        // specifying it in the constructor 
        public MongoWeatherRepository(MongoConnectionBuilder connection)
        {
            //use the connection builder object to connect tot the database and to the weather collection

            _weather = connection.GetDatabase().GetCollection<Weather>("WeatherDB");
        }
        public void DeleteByDate(DateTime start, DateTime end)
        {
            //create a filter builder for building our filter rules
            var builder = Builders<Weather>.Filter;
            // set a greater and less than or equal to filters to check the created date
            // against our provided start and end dates
            var filter = builder.Gte(n => n.Time, start) &
                         builder.Lte(n => n.Time, end);
            // pass the filter to the database and get the result in a list
            _weather.DeleteMany(filter);
        }

        public void DeleteWeather(string id)
        {
            //convert the string version of our id back to its original objectid format
            ObjectId objectId = ObjectId.Parse(id);
            // create a filter to natch the object id again the _id field is the colection
            var filter = Builders<Weather>.Filter.Eq(n => n._id, objectId);
            //Pass the filter to Mongo and tell it to delete the record.
            _weather.DeleteOne(filter);
        }

        public List<WeatherPresentationDTO> FindNewestWeatherPerTemperature()
        {
            var collection = _weather.AsQueryable();

            var result = collection.OrderByDescending(n => n.Time)
                                   .GroupBy(n => n.Temperature)
                                   .Select(g => new WeatherPresentationDTO
                                   {
                                       DeviceName = g.First().DeviceName,
                                       Precipitation = g.First().Precipitation,
                                       Created = g.First().Time,
                                       Latitutude = g.First().Latitutude,
                                       Longitude = g.First().Longitude,
                                       Temperature = g.First().Temperature,
                                       Atmospheric = g.First().Atmospheric,
                                       MaxWind = g.First().MaxWind,
                                       Solar = g.First().Solar,
                                       Vapor = g.First().Vapor,
                                       Humidity = g.First().Humidity,
                                       WindDirection = g.First().WindDirection
                                   })
                                   .ToList();
            return result;
        }

        public List<Weather> GetAllWeather()
        {
            //request a filter builder from the mongo builders class and store it
            var builder = Builders<Weather>.Filter;
            //use the filter builder to setup an empty filter
            var filter = builder.Empty;
            //pass the filter to mongo using the find command to find all the entries and put them in a list
            return _weather.Find(filter).ToList();
        }

        public Weather GetWeatherByid(string id)
        {
            //convert the string version of our id back to its original objectid format
            ObjectId objectId = ObjectId.Parse(id);
            // create a filter to natch the object id again the _id field is the colection
            var filter = Builders<Weather>.Filter.Eq(n => n._id, objectId);
            // pass the filter to the database and get the first result that matches
            return _weather.Find(filter).FirstOrDefault();
        }

        public WeatherReadingDTO FindReadingByDeviceNameAndDate(string deviceName, DateTime date)
        {
            var builder = Builders<Weather>.Filter;
            var filter = builder.Eq(w => w.DeviceName, deviceName) &
                         builder.Eq(w => w.Time, date);

            var result = _weather.Find(filter).FirstOrDefault();

            if (result == null)
            {
                return null;
            }

            return new WeatherReadingDTO(
                result.Temperature,
                result.Atmospheric,
                result.Solar,
                result.Precipitation
            );
        }

        public List<WeatherPresentationDTO> GetWeatherUsingLINQProjection()
        {
            throw new NotImplementedException();
        }

        public List<WeatherPresentationDTO> GetWeatherUsingProjection()
        {
            throw new NotImplementedException();
        }

        public void PostManyWeather(List<Weather> weatherList)
        {
            _weather.InsertMany(weatherList);
        }

        public void PostWeather(Weather weather)
        {
            //pass the weather to the database to be saved
            _weather.InsertOne(weather);
        }

        public List<Weather> SearchWeather(string search)
        {
            //pass our search term through the regex class to escape out any special characters
            var regexTerm = Regex.Escape(search); //add-migration
            // Create a filter builder so we can build our filter rules
            var builder = Builders<Weather>.Filter;
            // set filter rules using a regex filter type which takes takes our processed search
            //term and applies it to both the title and body fields. The "i" parameter indicates
            //the searcg is case insensitive. The pipe (|)  inidcate the filters are being applied
            // as an OR statement
            var filter = builder.Regex(n => n.DeviceName, new BsonRegularExpression(regexTerm, "i")) |
                         builder.Regex(n => n.Temperature, new BsonRegularExpression(regexTerm, "i"));

            return _weather.Find(filter).ToList();
        }

        public List<WeatherPresentationDTO> SearchWeatherUsingAggretation(string search)
        {
            throw new NotImplementedException();
        }

        public List<WeatherPresentationDTO> SearchWeatherUsingLINQAggretation(string search)
        {
            throw new NotImplementedException();
        }

        public void UpdateBasedOnDate(DateTime start, DateTime end, string devicename)
        {
            //create a filter builder for building our filter rules
            var builder = Builders<Weather>.Filter;
            // set a greater and less than or equal to filters to check the created date
            // against our provided start and end dates
            var filter = builder.Gte(n => n.Time, start) &
                         builder.Lte(n => n.Time, end);
            //Build an update rule to set the author field in all entries to the
            // provided author value
            var update = Builders<Weather>.Update.Set(n => n.DeviceName, devicename);
            //Pass the filter and update rule to mongo db to be applied.
            _weather.UpdateMany(filter, update);
        }



        public void UpdateWeather(string id, Weather weather)
        {
            //convert the string version of our id back to its original objectid format
            ObjectId objectId = ObjectId.Parse(id);
            // create a filter to natch the object id again the _id field is the colection
            var filter = Builders<Weather>.Filter.Eq(n => n._id, objectId);
            //create a builder to generate our update rules
            var builder = Builders<Weather>.Update;
            // create a set of rules for what fields need to be updated and what they need to be set to
            var update = builder.Set(n => n.DeviceName, weather.DeviceName)
                                .Set(n => n.Precipitation, weather.Precipitation)
                                .Set(n => n.Latitutude, weather.Latitutude)
                                .Set(n => n.Longitude, weather.Longitude)
                                .Set(n => n.Temperature, weather.Temperature)
                                .Set(n => n.Atmospheric, weather.Atmospheric)
                                .Set(n => n.MaxWind, weather.MaxWind)
                                .Set(n => n.Solar, weather.Solar)
                                .Set(n => n.Vapor, weather.Vapor)
                                .Set(n => n.Humidity, weather.Humidity)
                                .Set(n => n.WindDirection, weather.WindDirection);

            //pass the filter and update rules to the database to be processed
            _weather.UpdateOne(filter, update);
        }

        public WeatherPrecipititationDTO GetMaxPrecipitationForSensor(string deviceName)
        {
            // Calculate the date range (last 5 months)
            DateTime fiveMonthsAgo = DateTime.Now.AddMonths(-5);

            // Filter the weather data for the given sensor within the last 5 months
            var result = _weather
                .AsQueryable()
                .Where(w => w.DeviceName == deviceName && w.Time >= fiveMonthsAgo)
                .OrderByDescending(w => w.Precipitation) // Order by precipitation to get the max
                .FirstOrDefault(); // Get the highest precipitation entry

            if (result == null)
            {
                return null; // Return null if no data is found
            }

            // Return the WeatherPrecipititationDTO using the constructor
            return new WeatherPrecipititationDTO(result.DeviceName, result.Time, result.Precipitation);
        }



        public void UpdatePrecipitationValueById(string id, double newPrecipitationValue)
        {
            // Convert the string ID to MongoDB ObjectId
            ObjectId objectId;
            if (!ObjectId.TryParse(id, out objectId))
            {
                throw new ArgumentException("Invalid ObjectId format.", nameof(id));
            }

            // Create a filter to find the weather entry by its ObjectId
            var filter = Builders<Weather>.Filter.Eq(w => w._id, objectId);

            // Define the update operation to set the new precipitation value
            var update = Builders<Weather>.Update.Set(w => w.Precipitation, newPrecipitationValue);

            // Update the weather entry matching the filter
            _weather.UpdateOne(filter, update);
        }

        public List<WeatherTemperatureDTO> GetMaxTemperatureForSensors(DateTime start, DateTime end)
        {
            // Fetch the data from MongoDB
            var weatherData = _weather.AsQueryable()
                                       .Where(w => w.Time >= start && w.Time <= end)
                                       .ToList(); // Retrieve all data in-memory

            // Process the data in-memory: Group by DeviceName and find the maximum temperature for each sensor
            var results = weatherData
                            .GroupBy(w => w.DeviceName)
                            .Select(g => new
                            {
                                DeviceName = g.Key,
                                MaxTemperature = g.Max(w => w.Temperature),
                                Time = g.Where(w => w.Temperature == g.Max(w => w.Temperature))
                                        .Select(w => w.Time).FirstOrDefault() 
                            }).ToList();

            // Return the final DTO
            return results.Select(r => new WeatherTemperatureDTO(
                r.DeviceName,
                r.Time,
                r.MaxTemperature ?? 0 
            )).ToList();
        }
    }
}
