using Abra_RestApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace Abra_RestApi.Services.Users
{
    public interface IUsersService
    {
        public User CreateNewUser(User user);
        public User? GetNewUser(int id);
        public User UpdateUserData(User updatedUser);
    }
}
