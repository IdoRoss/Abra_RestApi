using Abra_RestApi.Models;
using Abra_RestApi.Services.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Reflection;
//Name: Ido Rosenberger
//Email ido.ross98@gmail.com

namespace Abra_RestApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UsersController : ControllerBase
    {

        private readonly IUsersService _usersService;

        private readonly HttpClient _httpClient;// a better practice would be to have this in a service
        public UsersController(IUsersService userService)
        {
            _usersService = userService;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://api.randomuser.me");
        }


        // ------------- API Call Func--------------- //
        // ideally would be a function in a RandomUsersService
        private async Task<RootObject?> RandomUsersApiCall(string query)
        {
            if (query == null)
            {
                query = "?results=100";
            }
            HttpResponseMessage response = await _httpClient.GetAsync("https://randomuser.me/api/" + query);
            response.EnsureSuccessStatusCode();
            var stringResult = await response.Content.ReadAsStringAsync();

            // convert to root object
            return JsonConvert.DeserializeObject<RootObject>(stringResult);
        }
        // ------------ PART 1 -------------//
        /// <summary>
        /// Returns data for 10 random users with specified gender
        /// </summary>
        /// <param name="gender">gender male or female</param>
        /// <returns>List of RandomUser objects</returns>
        [HttpGet("{gender}")]
        public async Task<ActionResult<IList<RandomUser>>> GetUserData(string gender)
        {
            // varify correct input
            if (gender != "male" && gender != "female")
                return NotFound();
            // request data from randomuser
            string query = $"?results=10&gender={gender}";

            var res = await RandomUsersApiCall(query);

            return Ok(res);
        }
        /// <summary>
        /// Query the most popular country out of 5000 users
        /// </summary>
        /// <returns>string of the mosy popular country</returns>
        [HttpGet]
        public async Task<ActionResult<string>> GetMostPupalarCountry()
        {
            string query = "?results=5000";

            var resRootObj = await RandomUsersApiCall(query);
            if (resRootObj == null)
            {
                return NotFound();
            }
            // quary thr most popular country
            var result = resRootObj.results
                .GroupBy(u => u.location.country)
                .OrderByDescending(g => g.Count())
                .First()
                .Key;

            return Ok(result);
        }
        /// <summary>
        /// Gets the email of 30 random users
        /// </summary>
        /// <returns>List of email strings</returns>
        [HttpGet]
        public async Task<ActionResult<List<string>>> GetListOfMails()
        {
            string query = "?results=30";

            var resRootObj = await RandomUsersApiCall(query);
            if (resRootObj == null)
            {
                return NotFound();
            }
            var result = resRootObj.results.Select(u => u.email).ToList();
            return Ok(result);
        }
        /// <summary>
        /// Query the oldest user out of 100 users
        /// </summary>
        /// <returns>OldestUser obj</returns>
        [HttpGet]
        public async Task<ActionResult<OldestUser>> GetTheOldestUser()
        {
            string query = "?results=100";

            var resRootObj = await RandomUsersApiCall(query);
            if (resRootObj == null)
            {
                return NotFound();
            }
            var result = resRootObj.results
                .OrderByDescending(u => u.dob.age)
                .First();
            return Ok(new OldestUser { name = result.name.first + " " + result.name.last, age = result.dob.age });
        }
        // ------------ PART 2 -------------//
        /// <summary>
        /// Takes a user and adds it to DB using the service
        /// </summary>
        /// <param name="user">new User</param>
        /// <returns>The created user</returns>
        [HttpPost]
        public ActionResult<User> CreateNewUser(User user)
        {
            // validate data
            if (user == null)
            {
                return NotFound();
            }
            try
            {
                var newUser = _usersService.CreateNewUser(user);
                return CreatedAtAction(nameof(GetNewUser), new { id = newUser.Id }, newUser);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        /// <summary>
        /// Gets a user from db using the service
        /// </summary>
        /// <param name="id">Id of requested user</param>
        /// <returns>The User</returns>
        [HttpGet("{id}")]
        public ActionResult<User> GetNewUser(int id)
        {
            var res = _usersService.GetNewUser(id);
            if (res == null)
            {
                return NotFound();
            }
            return Ok(res);
        }
        /// <summary>
        /// Gets a user to update and updates it by id useing the service
        /// </summary>
        /// <param name="userToUpdate">User to update</param>
        /// <returns>Updated user</returns>
        [HttpPut]
        public ActionResult<User> UpdateUserData(User userToUpdate)
        {
            try
            {
                var updatedUser = _usersService.UpdateUserData(userToUpdate);
                return Ok(updatedUser);
            }
            catch (Exception ex)
            {
                if (ex.Message == "Not Found")
                {
                    return NotFound(ex.Message);
                }
                return BadRequest(ex.Message);
            }
        }
    }
}
//Notes: 1) A better archtectute would be create a generic http respond class that is
//         responsible for sending the requested data and a message and in case of errors. this class would 
//         have a generic T data of the response data (User Or RandomUser or List of RandomUsers)
//       2) Haveing 2 seprate controllers is better for seperation of resources