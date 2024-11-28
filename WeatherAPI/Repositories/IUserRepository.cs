using WeatherAPI.Models;

namespace WeatherAPI.Repositories
{
    public interface IUserRepository
    {
        bool CreateUser(APIUser user);
        void UpdateLastLogin(string apiKey);
        bool AuthenticateUser(string apiKey, params Roles[] requiredRoles);
        bool DeleteUser(string id);
        void DeleteStudentsByLastLoginDateRange(DateTime start, DateTime end);
        void UpdateUserAccessLevelByCreationDateRange(DateTime start, DateTime end, string newAccessLevel);

    }
}
