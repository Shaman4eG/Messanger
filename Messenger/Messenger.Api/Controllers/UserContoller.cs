using Messenger.Constants;
using Messenger.DataLayer;
using Messenger.DataLayer.Sql;
using Messenger.Model;
using System;
using System.Data.SqlClient;
using System.Net;
using System.Web.Http;

namespace Messenger.Api.Controllers
{
    [System.Diagnostics.DebuggerStepThrough]
    [RoutePrefix("api/users")]
    public class UserController : ApiController
    {
        private readonly string connectionString = MiscConstants.ConnectionString;
        private readonly IUserRepository userRepository;

        public UserController()
        {
            userRepository = new UserRepository(connectionString);
        }



        /// <summary>
        /// Creates new user.
        /// </summary>
        /// <param name="user"> 
        /// New user data. 
        /// Should contain:
        /// - Required: Name, LastName, Email, Password 
        /// - Optional: AvatarId (To add it, you should create an Attachment first)
        /// 
        /// JSON example:
        /// {
        ///	    "Name": "TestName",
        ///	    "LastName": "TestLastName",
        ///	    "Email": "test@mail.com",
        ///	    "Password": "testPassword"
        /// }
        /// </param>
        /// <returns> 
        /// Created user: success
        /// 422 Unprocessable Entity: invalid input 
        /// 500 Internal Server Error: problems with database 
        /// </returns>
        [Route("create")]
        [HttpPost]
        public User Create(User user)
        {
            User createdUser = null;

            try {createdUser = userRepository.Create(user); }
            catch (SqlException)
            {
                var content = $"Failed to create user.";
                var reasonPhrase = "Internal server error";
                Utility.GenerateResponseMessage(HttpStatusCode.InternalServerError, reasonPhrase, content);
            }

            if (createdUser == null)
            {
                var content = $"Failed to create user.";
                var reasonPhrase = "Invalid input";
                Utility.GenerateResponseMessage(MiscConstants.UnprocessableEntity, reasonPhrase, content);
            }

            return createdUser;
        }

        /// <summary>
        /// Gets user by id.
        /// </summary>
        /// <param name="userId"> User id </param>
        /// <returns> 
        /// Found user data: success
        /// 404 Not Found : user not found
        /// 500 Internal Server Error: problems with database 
        /// </returns>
        [Route("{userId:guid}")]
        [HttpGet]
        public User GetById(Guid userId)
        {
            User foundUser = null;

            try { foundUser = userRepository.GetById(userId); }
            catch (SqlException)
            {
                var content = $"Failed to find user.";
                var reasonPhrase = "Internal server error";
                Utility.GenerateResponseMessage(HttpStatusCode.InternalServerError, reasonPhrase, content);
            }

            if (foundUser == null)
            {
                var content = $"User does not exist.";
                var reasonPhrase = "User not found";
                Utility.GenerateResponseMessage(HttpStatusCode.NotFound, reasonPhrase, content);
            }

            return foundUser;
        }

        /// <summary>
        /// Gets user by email.
        /// </summary>
        /// <param name="email"> Email of user to find </param>
        /// <returns> 
        /// Found user data: success
        /// 404 Not Found : user not found
        /// 500 Internal Server Error: problems with database 
        /// </returns>
        [Route("{email}")]
        [HttpGet]
        public User GetByEmail(string email)
        {
            User foundUser = null;

            try { foundUser = userRepository.GetByEmail(email); }
            catch (SqlException)
            {
                var content = $"Failed to find user.";
                var reasonPhrase = "Internal server error";
                Utility.GenerateResponseMessage(HttpStatusCode.InternalServerError, reasonPhrase, content);
            }

            if (foundUser == null)
            {
                var content = $"User does not exist.";
                var reasonPhrase = "User not found";
                Utility.GenerateResponseMessage(HttpStatusCode.NotFound, reasonPhrase, content);
            }

            return foundUser;
        }

        /// <summary>
        /// Updates existing user data.
        /// </summary>
        /// <param name="user"> Should contain existing user id and new data. </param>
        /// <returns>
        /// True: user updated
        /// False: invalid input
        /// Null: user not found
        /// Throws SqlException: problems with database
        /// 200 User updated: success
        /// 404 Not Found: user not found
        /// 422 Unprocessable Entity: invalid input 
        /// 500 Internal Server Error: problems with database 
        /// </returns>
        [Route("update")]
        [HttpPut]
        public void Update(User user)
        {
            bool? updated = null;

            try { updated = userRepository.Update(user); }
            catch (SqlException)
            {
                var content = $"Failed to update user.";
                var reasonPhrase = "Internal server error";
                Utility.GenerateResponseMessage(HttpStatusCode.InternalServerError, reasonPhrase, content);
            }

            if (updated == null)
            {
                var content = $"User does not exist.";
                var reasonPhrase = "User not found";
                Utility.GenerateResponseMessage(HttpStatusCode.NotFound, reasonPhrase, content);
            }
            
            if (updated == true)
            {
                var content = $"User successfully updated.";
                var reasonPhrase = "User updated";
                Utility.GenerateResponseMessage(HttpStatusCode.OK, reasonPhrase, content);
            }
            
            if (updated == false)
            {
                var content = $"Failed to update user.";
                var reasonPhrase = "Invalid input";
                Utility.GenerateResponseMessage(MiscConstants.UnprocessableEntity, reasonPhrase, content);
            }
        }

        /// <summary>
        /// Deletes user with given id.
        /// </summary>
        /// <param name="userId"> User id </param>
        /// <returns> 
        /// 200 User deleted: success
        /// 404 Not Found : user not found
        /// 500 Internal Server Error: problems with database 
        /// </returns>
        [Route("{userId:guid}")]
        [HttpDelete]
        public void Delete(Guid userId)
        {
            bool deleted = false;

            try { deleted = userRepository.Delete(userId); }
            catch (SqlException)
            {
                var content = $"Failed to delete user.";
                var reasonPhrase = "Internal server error";
                Utility.GenerateResponseMessage(HttpStatusCode.InternalServerError, reasonPhrase, content);
            }

            if (deleted)
            {
                var content = $"User successfully deleted.";
                var reasonPhrase = "User deleted";
                Utility.GenerateResponseMessage(HttpStatusCode.OK, reasonPhrase, content);
            }
            
            if (!deleted)
            {
                var content = $"Unable to delete user.";
                var reasonPhrase = "User not found";
                Utility.GenerateResponseMessage(HttpStatusCode.NotFound, reasonPhrase, content);
            }
        }
    }
}
