using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WeatherAPI.AttributeTags;
using WeatherAPI.Models.DTOs;
using WeatherAPI.Models;
using WeatherAPI.Repositories;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authorization;

namespace WeatherAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        //create a readonly field to store a reference to our repository
        private readonly IUserRepository _repository;
        //request access tp the repository from the dependcy injection by naming
        // it as an input parameter in our constructor
        public UserController(IUserRepository repository)
        {
            _repository = repository;
        }

        //POST: api/Users
        /// <summary>
        /// Creates a new user in the system based on the provided user data.
        /// </summary>
        /// <remarks>
        /// This endpoint allows a user with the 'TEACHER' role to create a new user account. 
        /// It checks if the provided role in the DTO is valid and maps the details to the APIUser model 
        /// before passing it to the repository for saving. If the user already exists, it returns an error.
        /// </remarks>
        /// <param name="userDTO">A <see cref="UserDTO"/> object containing the details of the user to be created.</param>
        /// <returns>
        /// An <see cref="ActionResult"/> indicating the result of the operation. Returns <c>BadRequest</c> 
        /// if the user role is invalid or if a user with the same email already exists, or <c>CreatedAtAction</c>
        /// if the user is successfully created.
        /// </returns>
        /// <response code="201">Created. The user was successfully created.</response>
        /// <response code="400">Bad Request. The provided user role is invalid or the user already exists.</response>
        /// <response code="401">Unauthorized. The API key is missing or does not have the required permissions.</response>
        /// <response code="403">Access Denied. The user does not have the necessary permissions.</response>
        /// <response code="500">Internal Server Error. An unexpected error occurred while processing the request.</response>
        [HttpPost]
        [ApiKey(Roles.TEACHER)]
        public ActionResult PostUser(UserDTO userDTO)
        {

            //check the provided role in the DTO is valid and respond with an error
            // code and message if is not
            if (!Enum.TryParse(userDTO.Role.ToUpper(), out Roles userRoleType))
            {
                return BadRequest("Invalid user role provided");
            }
            //mal the details from the dto and output of the enum check to our
            //proper ApiUser model so it can be passed to the repository
            var user = new APIUser
            {
                UserName = userDTO.UserName,
                Email = userDTO.Email,
                Role = userRoleType.ToString(),
            };
            //pass the model to th repository to ve saved and store the response
            var result = _repository.CreateUser(user);
            //if the result is false, meaning the user didnt save. Return an error.
            if (result == false)
            {
                return BadRequest("Error. A user with this email already exists");
            }
            //return a 201 response if successfully saved
            return CreatedAtAction("PostUser", user);
        }

        // DELETE: api/DeleteUser
        /// <summary>
        /// Deletes a user from the system based on the provided user ID.
        /// </summary>
        /// <remarks>
        /// This endpoint allows a user with the 'TEACHER' role to delete an existing user account. 
        /// It verifies the user's access permissions via the API key and then deletes the user by their ID.
        /// </remarks>
        /// <param name="id">The ID of the user to be deleted.</param>
        /// <returns>
        /// An <see cref="ActionResult"/> indicating the result of the deletion operation. 
        /// Returns <c>NoContent</c> (204) if the user is successfully deleted.
        /// </returns>
        /// <response code="204">No Content. The user was successfully deleted.</response>
        /// <response code="400">Bad Request. The request is invalid or the user could not be deleted.</response>
        /// <response code="401">Unauthorized. The API key is missing or does not have the required permissions.</response>
        /// <response code="403">Forbidden. The user does not have the necessary permissions to delete users.</response>
        /// <response code="404">Not Found. The user with the specified ID does not exist.</response>
        /// <response code="500">Internal Server Error. An unexpected error occurred while processing the request.</response>
        [HttpDelete("DeleteUser")]
        [ApiKey(Roles.TEACHER)]
        public ActionResult DeleteUser(string id)
        {
            try
            {
                _repository.DeleteUser(id);
                return NoContent(); // Return 204 No Content on successful deletion
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message); // Return 400 Bad Request for invalid ID format
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message); // Return 404 Not Found if the user is not found
            }
        }

        // DELETE: api/User/DeleteStudentsByLastLoginDateRange
        /// <summary>
        /// Deletes users with the 'Student' role who last logged in between the specified start and end dates.
        /// </summary>
        /// <remarks>
        /// This endpoint allows administrators to delete user accounts that have the 'Student' role and last logged in
        /// within a specified date range. Only administrators are authorized to perform this operation.
        /// </remarks>
        /// <param name="start">The start date of the last login date range.</param>
        /// <param name="end">The end date of the last login date range.</param>
        /// <returns>
        /// An <see cref="ActionResult"/> indicating the result of the deletion operation. Returns <c>BadRequest</c>
        /// if the date range is invalid, <c>Unauthorized</c> if the user is not an administrator, or <c>NoContent</c>
        /// if the operation is successful.
        /// </returns>
        /// <response code="204">No Content. The users were successfully deleted.</response>
        /// <response code="400">Bad Request. The start date is after the end date, or the date range is invalid.</response>
        /// <response code="401">Unauthorized. The user does not have the necessary permissions.</response>
        /// <response code="403">Forbidden. The user does not have the necessary permissions to delete users.</response>
        /// <response code="500">Internal Server Error. An unexpected error occurred while processing the request.</response>
        [HttpDelete("DeleteStudentsByLastLoginDateRange")]
        [ApiKey(Roles.TEACHER)]
        public ActionResult DeleteStudentsByLastLoginDateRange(DateTime start, DateTime end)
        {
            try
            {
                _repository.DeleteStudentsByLastLoginDateRange(start, end);
                return NoContent(); // Return 204 if successful
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message); // Return 400 if something goes wrong
            }
        }

        // PUT: api/User/UpdateUserAccessLevelByCreationDateRange
        /// <summary>
        /// Updates the access level for users created within a specified date range.
        /// </summary>
        /// <remarks>
        /// This endpoint allows administrators to update the access level for user accounts that were created
        /// within a specified date range. Only administrators are authorized to perform this operation.
        /// </remarks>
        /// <param name="start">The start date of the creation date range.</param>
        /// <param name="end">The end date of the creation date range.</param>
        /// <param name="newAccessLevel">The new access level to be set for the users.</param>
        /// <returns>
        /// An <see cref="ActionResult"/> indicating the result of the update operation. Returns <c>BadRequest</c>
        /// if the date range is invalid, <c>Unauthorized</c> if the user is not an administrator, or <c>NoContent</c>
        /// if the operation is successful.
        /// </returns>
        /// <response code="204">No Content. The users' access levels were successfully updated.</response>
        /// <response code="400">Bad Request. The start date is after the end date, or the date range is invalid.</response>
        /// <response code="401">Unauthorized. The user does not have the necessary permissions.</response>
        /// <response code="403">Forbidden. The user does not have the necessary permissions to update users.</response>
        /// <response code="500">Internal Server Error. An unexpected error occurred while processing the request.</response>
        [HttpPut("UpdateUserAccessLevelByCreationDateRange")]
        [ApiKey(Roles.TEACHER)]
        public ActionResult UpdateUserAccessLevelByCreationDateRange(DateTime start, DateTime end, string newAccessLevel)
        {
            // Validate the date range
            if (start >= end)
            {
                return BadRequest("The start date must be before the end date.");
            }

            // Validate the new access level
            if (string.IsNullOrEmpty(newAccessLevel))
            {
                return BadRequest("The new access level must be provided.");
            }

            // Call the repository method to update the access level for users by creation date range
            _repository.UpdateUserAccessLevelByCreationDateRange(start, end, newAccessLevel);

            // Return NoContent status code indicating successful update
            return NoContent();
        }
    }
}
