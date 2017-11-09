using Messenger.DataLayer;
using Messenger.DataLayer.Sql;
using Messenger.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;


namespace Messenger.Api.Controllers
{
    [RoutePrefix("api/users")]
    public class UserController : ApiController
    {
        private readonly string connectionString = InputConstraintsAndDefaultValues.ConnectionString;
        private readonly IUserRepository userRepository;

        public UserController()
        {
            userRepository = new UserRepository(connectionString);
        }



        /// <summary>
        /// Create new user.
        /// </summary>
        /// <param name="user"> New user data </param>
        /// <returns> Created user </returns>
        [Route("")]
        [HttpPost]
        public User Create(User user)
        {
            return userRepository.Create(user);
        }

        /// <summary>
        /// Get user by id.
        /// </summary>
        /// <param name="id"> User id </param>
        /// <returns> Found user data or null, if user not found </returns>
        [Route("{id:guid}")]
        [HttpGet]
        public User Get(Guid id)
        {
            return userRepository.Get(id);
        }

        [Route("lol/arbido/{id:int}")]
        [HttpGet]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
