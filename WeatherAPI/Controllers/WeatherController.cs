using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using WeatherAPI.AttributeTags;
using WeatherAPI.Models;
using WeatherAPI.Models.DTOs;
using WeatherAPI.Repositories;

//find the reading
//highest temp for each record
//add user 
//mutliple weather enteries
//updates 
//update mulitple levels
namespace WeatherAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeatherController : Controller
    {

        // creatte a variable to hold a reference to an INoteRepoisitory object
        private readonly IWeatherRepository _repository;
        // request the class that implments the INoteRepository interface from the depency injection
        public WeatherController(IWeatherRepository repository)
        {
            _repository = repository;
        }

        // GET: api/Weather/GetMaxPrecipitationForSensor
        /// <summary>
        /// Retrieves the maximum precipitation recorded in the last 5 months for a specified sensor device.
        /// </summary>
        /// <remarks>
        /// This endpoint allows users to query the MongoDB database to find the highest precipitation value
        /// recorded by a specific sensor device within the past 5 months. The response includes the sensor's
        /// name, the date and time of the reading, and the maximum precipitation value.
        /// </remarks>
        /// <param name="deviceName">The name of the sensor device to search for.</param>
        /// <returns>
        /// An <see cref="ActionResult{T}"/> containing a <see cref="WeatherPrecipititationDTO"/> object if the
        /// maximum precipitation data is found. Returns <c>BadRequest</c> if the device name is not provided,
        /// <c>NotFound</c> if no data is found, or <c>Ok</c> with the data if found.
        /// </returns>
        /// <response code="200">OK. The maximum precipitation data was successfully retrieved.</response>
        /// <response code="400">Bad Request. The device name was not provided.</response>
        /// <response code="403">Access Denied. The user does not have the necessary permissions.</response>
        /// <response code="404">Not Found. No records found for the specified device within the last 5 months.</response>
        /// <response code="500">Internal Server Error. An unexpected error occurred while processing the request.</response>
        [HttpGet("GetMaxPrecipitationForSensor")]
        [ApiKey(Roles.TEACHER, Roles.STUDENT)]
        public ActionResult<WeatherPrecipititationDTO> GetMaxPrecipitationForSensor(string deviceName)
        {
            // Check if the device name parameter is null or empty
            // If no device name is provided, return a BadRequest response with a message
            if (string.IsNullOrEmpty(deviceName))
            {
                return BadRequest("Device name must be provided.");
            }

            // Call the repository method to retrieve the maximum precipitation data for the specified device
            // The method returns a WeatherPresentationDTO containing the highest precipitation recorded
            var maxPrecipitationData = _repository.GetMaxPrecipitationForSensor(deviceName);

            // Check if the repository returned null, meaning no records were found
            // If no data is found for the given device name in the past 5 months, return a NotFound response
            if (maxPrecipitationData == null)
            {
                return NotFound($"No records found for the device: {deviceName} in the last 5 months.");
            }

            // If data is found, return an Ok response with the WeatherPresentationDTO containing the max precipitation data
            return Ok(maxPrecipitationData);

        }



        // GET: api/Weather/GetMaxTemperatureForSensors
        /// <summary>
        /// Gets the maximum temperature recorded for all sensors within a specified date/time range.
        /// </summary>
        /// <remarks>
        /// This endpoint allows administrators to retrieve the maximum temperature recorded by each sensor 
        /// within a given date and time range. It returns a list of the sensor name, the reading date and time, 
        /// and the temperature value for each sensor.
        /// </remarks>
        /// <param name="start">The start date/time of the range to search within.</param>
        /// <param name="end">The end date/time of the range to search within.</param>
        /// <returns>
        /// An <see cref="ActionResult"/> containing a list of <see cref="WeatherTemperatureDTO"/> objects representing 
        /// the maximum temperature readings from each sensor. Returns <c>BadRequest</c> if the date range is invalid or 
        /// <c>Ok</c> with the data if found.
        /// </returns>
        /// <response code="200">OK. The maximum temperature data was successfully retrieved.</response>
        /// <response code="400">Bad Request. The start date is after the end date or invalid input.</response>
        /// <response code="403">Access Denied. The user does not have the necessary permissions.</response>
        /// <response code="500">Internal Server Error. An unexpected error occurred while processing the request.</response>
        [HttpGet("GetMaxTemperatureForSensors")]
        [ApiKey(Roles.TEACHER, Roles.STUDENT)]
        public ActionResult<List<WeatherTemperatureDTO>> GetMaxTemperatureForSensors(DateTime start, DateTime end)
        {
            // Validate the date range
            if (start >= end)
            {
                return BadRequest("The start date must be before the end date.");
            }

            // Call the repository method to get the maximum temperature per sensor in the specified date range
            var result = _repository.GetMaxTemperatureForSensors(start, end);

            // Return the result with an OK status code
            return Ok(result);
        }

        //POST: api/Weather
        /// <summary>
        /// Creates a new weather entry in the database
        /// </summary>
        /// 
        /// <remarks>
        /// This endpoint allows users to add new weather records to the MongoDB database.
        /// The request body must contain a WeatherDTO object with the required weather data.
        /// </remarks>
        /// 
        /// <param name="weatherDto">A WeatherDTO object containing the weather data to be stored</param>
        /// 
        /// <response code="201">Created. The weather record has been successfully created.</response>
        /// <response code="400">Bad Request. The request body is missing or contains invalid data.</response>
        /// <response code="401">Unauthorized. The API Key is missing or invalid.</response>
        /// <response code="403">Access Denied. The user does not have the necessary permissions.</response>
        /// <response code="500">Internal Server Error. An unexpected error occurred while processing the request.</response>
        [ProducesResponseType(typeof(List<Weather>), StatusCodes.Status200OK)]
        [HttpPost]
        [ApiKey(Roles.TEACHER, Roles.SENSOR)]
        public ActionResult CreateWeather([FromBody] WeatherDTO weatherDto)
        {
            var weather = new Weather
            {
                DeviceName = weatherDto.DeviceName,
                Precipitation = weatherDto.Precipitation,
                Time = DateTime.UtcNow,
                Latitutude = weatherDto.Latitutude,
                Longitude = weatherDto.Longitude,
                Temperature = weatherDto.Temperature,
                Atmospheric = weatherDto.Atmospheric,
                MaxWind = weatherDto.MaxWind,
                Solar = weatherDto.Solar,
                Vapor = weatherDto.Vapor,
                Humidity = weatherDto.Humidity,
                WindDirection = weatherDto.WindDirection

            };
            // PAss the request on to the repository for processing
            _repository.PostWeather(weather);
            return CreatedAtAction("CreateWeather", weather);
        }

        //POST api/Weather/CreateMany
        /// <summary>
        /// Creates multiple weather entries in the database
        /// </summary>
        /// 
        /// <remarks>
        /// This endpoint allows users to add multiple weather records to the MongoDB database in a single request.
        /// The request body must contain a list of WeatherDTO objects with the required weather data.
        /// </remarks>
        /// 
        /// <param name="weatherDtoList">A list of WeatherDTO objects containing the weather data to be stored</param>
        /// 
        /// <response code="201">Created. The weather records have been successfully created.</response>
        /// <response code="400">Bad Request. The provided list is empty or contains invalid data.</response>
        /// <response code="401">Unauthorized. The API Key is missing or invalid.</response>
        /// <response code="403">Access Denied. The user does not have the necessary permissions.</response>
        /// <response code="500">Internal Server Error. An unexpected error occurred while processing the request.</response>
        [ProducesResponseType(typeof(List<Weather>), StatusCodes.Status200OK)]
        [HttpPost("CreateManyWeather")]
        [ApiKey(Roles.TEACHER, Roles.SENSOR)]
        public ActionResult PostMany([FromBody] List<WeatherDTO> weatherDtoList)
        {
            if (weatherDtoList.Count < 1)
            {
                return BadRequest("No items in provided list");
            }
            //pass the dto list through a LINQ select query which will create a new note
            //for each item and put them into a new list
            var weatherList = weatherDtoList.Select(w => new Weather
            {

                DeviceName = w.DeviceName,
                Precipitation = w.Precipitation,
                Time = DateTime.UtcNow,
                Latitutude = w.Latitutude,
                Longitude = w.Longitude,
                Temperature = w.Temperature,
                Atmospheric = w.Atmospheric,
                MaxWind = w.MaxWind,
                Solar = w.Solar,
                Vapor = w.Vapor,
                Humidity = w.Humidity,
                WindDirection = w.WindDirection

            }).ToList();

            // Pass the list to the repository to be saved
            _repository.PostManyWeather(weatherList);
            return CreatedAtAction("PostMany", weatherList);
        }


        // PUT: api/Weather/UpdatePrecipitation
        /// <summary>
        /// Updates the precipitation value of a specific weather entry.
        /// </summary>
        /// <remarks>
        /// This endpoint allows administrators to update the precipitation value for a specific weather entry 
        /// identified by its ID. This is useful for correcting errors in the dataset.
        /// </remarks>
        /// <param name="id">The ID of the weather entry to be updated.</param>
        /// <param name="newPrecipitationValue">The new precipitation value to set.</param>
        /// <returns>
        /// An <see cref="ActionResult"/> indicating the result of the update operation. Returns <c>BadRequest</c>
        /// if the ID format is invalid or the new value is not provided, <c>Unauthorized</c> if the user is not an administrator,
        /// or <c>NoContent</c> if the operation is successful.
        /// </returns>
        /// <response code="204">No Content. The precipitation value was successfully updated.</response>
        /// <response code="400">Bad Request. The ID is invalid or the new precipitation value is not provided.</response>
        /// <response code="401">Unauthorized. The user does not have the necessary permissions.</response>
        /// <response code="403">Access Denied. The user does not have the necessary permissions.</response>
        /// <response code="404">Not Found. The weather entry with the specified ID does not exist.</response>
        /// <response code="500">Internal Server Error. An unexpected error occurred while processing the request.</response>
        [HttpPut("UpdatePrecipitation")]
        [ApiKey(Roles.TEACHER)]
        public ActionResult UpdatePrecipitation(string id, double newPrecipitationValue)
        {
            // Validate the ID format
            if (string.IsNullOrEmpty(id) || !ObjectId.TryParse(id, out _))
            {
                return BadRequest("Invalid ID format.");
            }

            // Call the repository method to update the precipitation value
            try
            {
                _repository.UpdatePrecipitationValueById(id, newPrecipitationValue);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }

            // Return NoContent status code indicating successful update
            return NoContent();
        }

        //PUT api/Weather/UpdatedByDate
        /// <summary>
        /// Updates weather entries based on a specified date range and device name
        /// </summary>
        /// 
        /// <remarks>
        /// This endpoint allows users to update weather records in the MongoDB database that fall within the specified start and end date range and match the given device name.
        /// </remarks>
        /// 
        /// <param name="start">The start date of the range to update weather records within.</param>
        /// <param name="end">The end date of the range to update weather records within.</param>
        /// <param name="deviceName">The name of the device whose weather records are to be updated.</param>
        /// 
        /// <response code="200">OK. The weather records have been successfully updated.</response>
        /// <response code="400">Bad Request. The start date is after the end date or invalid input.</response>
        /// <response code="401">Unauthorized. The API Key is missing or invalid.</response>
        /// <response code="403">Access Denied. The user does not have the necessary permissions.</response>
        /// <response code="404">Not Found. No weather records match the specified date range and device name.</response>
        /// <response code="500">Internal Server Error. An unexpected error occurred while processing the request.</response>
        [ProducesResponseType(typeof(List<Weather>), StatusCodes.Status200OK)]
        [HttpPut("UpdatedByDate")]
        [ApiKey(Roles.TEACHER)]
        public ActionResult UpdateByDate(DateTime start, DateTime end, string deviceName)
        {
            _repository.UpdateBasedOnDate(start, end, deviceName);
            return Ok();
        }

        
   
        

        // PATCH: api/Weather/BatchInsertPrecipitation
        /// <summary>
        /// Patch inserts weather data using the precipitation field for daily records.
        /// </summary>
        /// 
        /// <remarks>
        /// this endpoint allows users to perform a patch inset of waether records into the MongoDB database
        /// each record in the provided list represens a weather data etry, and all entries are inserted in a single operation
        /// the insertion is intnded to optimise the additin of daily weather data by allowig multiple records to be added at once
        /// </remarks>
        /// 
        /// <param name="weatherDtoList">A list of WeatherDTO objects to be inserted.</param>
        /// 
        /// <response code="200">OK. The weather records have been successfully inserted.</response>
        /// <response code="400">Bad Request. The provided list is null or empty, indicating no data to insert.</response>
        /// <response code="401">Unauthorized. The API Key is missing or invalid.</response>
        /// <response code="403">Access Denied. The user does not have the necessary permissions.</response>
        /// <response code="500">Internal Server Error. An unexpected error occurred while processing the request.</response>
        [HttpPatch("BatchInsertPrecipitation")]
        [ApiKey(Roles.TEACHER)]
        public ActionResult BatchInsertPrecipitation([FromBody] List<WeatherDTO> weatherDtoList)
        {
            if (weatherDtoList == null || !weatherDtoList.Any())
            {
                return BadRequest("The list of weather data cannot be null or empty.");
            }

            // Convert DTOs to Weather objects
            var weatherList = weatherDtoList.Select(w => new Weather
            {
                DeviceName = w.DeviceName,
                Precipitation = w.Precipitation,
                Time = DateTime.UtcNow,
                Latitutude = w.Latitutude,
                Longitude = w.Longitude,
                Temperature = w.Temperature,
                Atmospheric = w.Atmospheric,
                MaxWind = w.MaxWind,
                Solar = w.Solar,
                Vapor = w.Vapor,
                Humidity = w.Humidity,
                WindDirection = w.WindDirection
            }).ToList();

            // Call the repository method to insert the weather data in bulk
            _repository.PostManyWeather(weatherList);

            return Ok("Batch insert of weather data completed successfully.");
        }


        /// <summary>
        /// Finds a weather reading for a specified device and date.
        /// </summary>
        /// <remarks>
        /// This endpoint retrieves a single weather reading based on the provided device name and exact date and time.
        /// It requires both the device name and date/time to identify a unique entry.
        /// Only users with the necessary permissions can access this endpoint.
        /// </remarks>
        /// <param name="deviceName">The name of the device to filter records by (required).</param>
        /// <param name="date">The exact date and time of the desired weather reading (required).</param>
        /// <returns>A single weather reading matching the specified device name and date.</returns>
        /// <response code="200">OK. Returns the weather reading matching the specified criteria.</response>
        /// <response code="400">Bad Request. Occurs if device name or date is invalid.</response>
        /// <response code="401">Unauthorized. The API Key is missing or invalid.</response>
        /// <response code="403">Forbidden. The user does not have the necessary permissions to access this endpoint.</response>
        /// <response code="404">Not Found. No reading was found matching the specified device name and date.</response>
        /// <response code="500">Internal Server Error. An unexpected error occurred while processing the request.</response>
        [HttpGet("FindReadingByDeviceNameAndDate")]
        [ApiKey(Roles.TEACHER, Roles.STUDENT)]
        public ActionResult<WeatherReadingDTO> FindReadingByDeviceNameAndDate(string deviceName, DateTime date)
        {
            if (string.IsNullOrEmpty(deviceName))
            {
                return BadRequest("The device name must be provided.");
            }

            var reading = _repository.FindReadingByDeviceNameAndDate(deviceName, date);

            if (reading == null)
            {
                return NotFound("No matching reading found for the specified device and date.");
            }

            return Ok(reading);
        }
    }
}
