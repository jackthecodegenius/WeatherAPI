using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WeatherAPI.Models;
using WeatherAPI.Repositories;

namespace WeatherAPI.AttributeTags
{
    [AttributeUsage(validOn: AttributeTargets.Method | AttributeTargets.Class)]
    public class ApiKeyAttribute : Attribute, IAsyncActionFilter
    {
        //variable to store the array of roles we need to check for this filter
        public Roles[] AllowedRoles { get; set; }
        // constructor which allos the allowed roles to be passed to the filter attribute.
        public ApiKeyAttribute(params Roles[] roles)
        {
            AllowedRoles = roles;
        }

        // method that runs the filter an dpasses the request onto the newxt item in the path
        // which will either be a controller or endpoint. the context variable is the request
        // data being sent through the system, the next parameter is a reference to the next
        //item in the path
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // try to retrieve the apiKey from the header data, if it cannot be found the if 
            // statement will run
            if (context.HttpContext.Request.Headers.TryGetValue("apiKey", out var key) == false)
            {
                //create a class to set a response code and message and put it into the 
                //result/responce section of the request
                context.Result = new ContentResult
                {
                    StatusCode = 401,
                    Content = "No API key provided with request"

                };
                //return out of the method, which will cause the request to proceed to further
                // and start executing the response actions of the api
                return;
            }

            //convert the key form a string values data type to a standard string and the 
            //the leading and trailing curly braces
            var validKey = key.ToString().Trim('{','}');
            //Request our UserRepository class using an alternate method to our constructor request.
            var userRepo = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
            //pass the apiKey and allowed roles to the authenticate method to see if the user's 
            // credentials are correct are allowed to perform their desired request
            if (userRepo.AuthenticateUser(validKey, AllowedRoles) == false)
            {
                //create a class to set a response code and message and put it into the 
                //result/responce section of the request
                context.Result = new ContentResult
                {
                    StatusCode = 403,
                    Content = "The provided API Key is invalid or does not have the required permissions!"

                };
                //return out of the method, which will cause the request to proceed to further
                // and start executing the response actions of the api
                return;
            }
            //update the user details to change their last access date
            userRepo.UpdateLastLogin(validKey);
            //trigger the next variable which will run the next item in the route path
            await next();
        }

    }
}
