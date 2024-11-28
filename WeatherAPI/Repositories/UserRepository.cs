using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using WeatherAPI.Models;
using WeatherAPI.Services;

namespace WeatherAPI.Repositories
{
    public class UserRepository : IUserRepository
    {
        //create a readonly fields for stroging a references to the 
        // connection to our database collection
        private readonly IMongoCollection<APIUser> _users;
        //request access to the mongo connection builder class by naming it as a
        // paramter in our constrcutor
        public UserRepository(MongoConnectionBuilder connection)
        {
            //use the rpvoided conecction builder to get access to our collection
            // and set it up to map to the ApiUser model.
            _users = connection.GetDatabase().GetCollection<APIUser>("ApiUsers");
        }

        //[Route("AuthenticateUser")]
        //[HttpGet("AuthenticateUser")]
        public bool AuthenticateUser(string apiKey, params Roles[] requiredRoles)
        {
            //create a filter to check each user's api key against the key provided 
            // in the parameter
            var filter = Builders<APIUser>.Filter.Eq(u => u.ApiKey, apiKey);
            //pass the filter to find method to find the matching user and 
            //store the result
            var user = _users.Find(filter).FirstOrDefault();
            //if no users was found return false to indicate an error
            if (user == null)
            {
                return false;
            }
            //run the method to check the user crendiatials against the required
            //roles for the nedpoint they are trying to run
            if (IsAllowedRole(user.Role, requiredRoles) == false)
            {
                //if the user failts the check, return false to indicate an error
                return false;
            }
            //if the user details pass all the checks return true
            return true;
        }
        //[HttpGet("CreateUser")]
        public bool CreateUser(APIUser user)
        {
            //create a filter to check the provided user email against all the documents in the collection
            var filter = Builders<APIUser>.Filter.Eq(u => u.Email, user.Email);
            //pass the filter to the database with a find comand and store any
            //results if a maycj is found
            var existingUser = _users.Find(filter).FirstOrDefault();
            //if the user already has an account return false to indicated this,
            if (existingUser != null)
            {
                return false;
            }
            //generate a unique 36 character string to be used as the ApiKey, a GUID
            // is a global unique indeinfier which is considered to have an almost
            //impossible chance of being generated twice
            user.ApiKey = Guid.NewGuid().ToString();
            //set the date time fields using the system clock
            user.Created = DateTime.UtcNow;
            user.LastAccess = DateTime.UtcNow;
            //pass the user details to the dtabase to be saved then return true
            _users.InsertOne(user);
            return true;
        }
        //[HttpGet("UpdateLastLogin")]
        public void UpdateLastLogin(string apiKey)
        {
            //get the current date time so we can pass it to the database
            var currentdate = DateTime.UtcNow;
            //create a filter to find the user entry that matches the provided ApiKey
            var filter = Builders<APIUser>.Filter.Eq(u => u.ApiKey, apiKey);
            //Create an update rule to change the users' last access field to the current
            //date
            var update = Builders<APIUser>.Update.Set(u => u.LastAccess, currentdate);
            //Pass the filter and update rule to the database to be processed
            _users.UpdateOne(filter, update);
        }
        //[Route("IsAllowedRole")]
        //[HttpGet("IsAllowedRole")]
        private bool IsAllowedRole(string userRole, Roles[] allowedRoles)
        {
            //use an if statement to run the tryparse on our enum to see if the
            //provided user role matches one of our pre-defined options
            if (!Enum.TryParse(userRole, out Roles userRoleType))
            {
                //if not, return false to indicate a failure
                return false;
            }
            //cycle through all the roles in our array and compare each one against the
            // the provided user role.
            foreach (var role in allowedRoles)
            {
                if (userRoleType.Equals(role))
                {
                    //if a match is found, return true to indicate success
                    return true;
                }
            }
            //if not, return false to indicate a failure
            return false;
        }

        public bool DeleteUser(string id)
        {
            // Try to parse the string id into an ObjectId
            if (!ObjectId.TryParse(id, out ObjectId objectId))
            {
                throw new ArgumentException("Invalid ID format.");
            }

            // Create a filter to match the user by _id
            var filter = Builders<APIUser>.Filter.Eq(u => u._id, objectId);

            // Attempt to delete the user
            var result = _users.DeleteOne(filter);

            // Return true if a user was deleted, false if none were found
            return result.DeletedCount > 0;
        }

        public void DeleteStudentsByLastLoginDateRange(DateTime start, DateTime end)
        {
            // Ensure the Role is correctly checked (assuming the Role is a string)
            var builder = Builders<APIUser>.Filter;

            // Create a filter to check for students with the "STUDENT" role and LastAccess within the date range
            var filter = builder.Eq(u => u.Role, Roles.STUDENT.ToString()) &
                         builder.Gte(u => u.LastAccess, start) &
                         builder.Lte(u => u.LastAccess, end);

            // Perform the deletion
            var result = _users.DeleteMany(filter);

            // Optionally handle the result, e.g., log how many users were deleted
            Console.WriteLine($"{result.DeletedCount} students deleted.");
        }

        public void UpdateUserAccessLevelByCreationDateRange(DateTime start, DateTime end, string newAccessLevel)
        {
            // Create filter to find users created within the given date range
            var builder = Builders<APIUser>.Filter;
            var filter = builder.Gte(u => u.Created, start) &
                         builder.Lte(u => u.Created, end);

            // Define the update operation to set the new access level
            var update = Builders<APIUser>.Update.Set(u => u.Role, newAccessLevel);

            // Update all users matching the filter
            _users.UpdateMany(filter, update);
        }
    }
}
