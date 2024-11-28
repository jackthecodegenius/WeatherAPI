using Microsoft.Extensions.Options;
using WeatherAPI.Settings;
using MongoDB.Driver;

using System.Net.WebSockets;

namespace WeatherAPI.Services
{
    public class MongoConnectionBuilder
    {
        //variable to hold a reference to our settings class ovce we request if from our dependcy injection
        private readonly IOptions<MongoConnectionSettings> _settings;

        // request our settings class by putting it as a parameter on the constructior this will automatically
        //ask for it to be provuded by the dependency injection in the class is built
        public MongoConnectionBuilder(IOptions<MongoConnectionSettings> settings)
        {
            //stored the recieved item into our variable
            _settings = settings;
        }

        
        public IMongoDatabase GetDatabase()
        {
            //create class for connecting class with mongo db amd jand it connection string
            var client = new MongoClient(_settings.Value.ConnectionString);
            // once the class is created ask to connect directly to database
            return client.GetDatabase(_settings.Value.DatabaseName);
        }
        
    }
}
