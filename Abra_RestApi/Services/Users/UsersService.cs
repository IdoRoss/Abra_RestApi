using Abra_RestApi.Data;
using Abra_RestApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Abra_RestApi.Services.Users
{
    public class UsersService : IUsersService
    {
        private readonly ApiContext _context;
        // DI the context
        public UsersService(ApiContext context)
        {
            _context = context;
        }
        public User CreateNewUser(User user)
        {

            // assuming id is key check if already existing
            if (!IsUserInDb(user))
            {
                throw new Exception("Not Found");
            }
            _context.Users.Add(user);
            return user;
        }


        public User? GetNewUser(int id)
        {
            // find user and return it
            var res = _context.Users.FirstOrDefault(u => u.Id == id);

            return res;
        }

        public User UpdateUserData(User updatedUser)
        {
            // modify the user
            _context.Entry(updatedUser).State = EntityState.Modified;

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!!IsUserInDb(updatedUser))
                {
                    throw new Exception("Not Found");
                }
                else
                {
                    throw;
                }
            }

            return updatedUser;
        }
        // helper
        private bool IsUserInDb(User user)
        {
            return _context.Users.Any(u => u.Id == user.Id);
        }
    }
}
