using Abra_RestApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Reflection;

namespace Abra_RestApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private static List<User> _users = new List<User>();
        private readonly HttpClient _httpClient;

        public UsersController()
        {
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
