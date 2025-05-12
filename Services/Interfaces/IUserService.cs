using System.Collections.Generic;
using System.Threading.Tasks;
using JiraApp.Models.Authentication;

namespace JiraApp.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> GetUserByIdAsync(int id);
        Task<User> GetUserByUsernameAsync(string username);
        Task<User> CreateUserAsync(User user);
        Task<User> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> AuthenticateUserAsync(string username, string password);
    }
}
