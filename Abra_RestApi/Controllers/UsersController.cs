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
        private static List<User> _users = new List<User>();// to be removed when finished service integration
        private readonly HttpClient _httpClient;// a better practice would be to have this in a service
        public UsersController(IUsersService userService)
        {
            _usersService = userService;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://api.randomuser.me");
        }


        // ------------- API Call Func--------------- //
        private async Task<RootObject?> RandomUsersApiCall(string query)
        {
            if(query == null)
            {
                query = "?results=100";
            }
            HttpResponseMessage response = await _httpClient.GetAsync("https://randomuser.me/api/"+ query);
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
        [HttpGet("{gender}")]
        [HttpGet]
        public async Task<ActionResult<string>> GetMostPupalarCountry()
        {
            string query = "?results=5000";

            var resRootObj = await RandomUsersApiCall(query);
            if(resRootObj == null)
            {
                return NotFound();
            }
            // quary thr most popular country
            var result = resRootObj.results
                .GroupBy(u => u.nat)
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
        [HttpPost]
        public ActionResult<User> CreateNewUser(User user)
        {
            // validate data
            if (user == null)
            {
                return NotFound();
            }
            // assuming id is key check if already existing
            if(_users.Any(u=>u.Id==user.Id))
            {
                return BadRequest();
            }
            _users.Add(user);
            return CreatedAtAction(nameof(GetNewUser), new { id = user.Id }, user);
        }
        [HttpGet("{id}")]
        public ActionResult<User> GetNewUser(int id)
        {
            var res = _users.FirstOrDefault(u=>u.Id==id);
            if(res == null)
            {
                return NotFound();
            }
            return Ok(res);
        }
        [HttpPatch]
        public ActionResult<User> UpdateUserData(User updatedUser)
        {
            var DbUser = _users.FirstOrDefault(u => u.Id == updatedUser.Id);
            if (DbUser == null)
            {
                return NotFound();
            }
            _users.Remove(DbUser);
            _users.Add(updatedUser);
            return Ok(updatedUser);
        }
    }
}
//Notes: 1) A better archtectute would be create a generic http respond object that is
//         responsible of sending a message in case of errors and have a generic T data
//         of the response data (User Or RandomUser or List of RandomUsers)
//       2) Haveing 2 seprate controllers is better for seperation