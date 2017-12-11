using Messenger.Constants;
using Messenger.DataLayer;
using Messenger.DataLayer.Sql;
using Messenger.Model;
using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Net;
using System.Text;
using System.Web.Http;


namespace Messenger.Api.Controllers
{
    [System.Diagnostics.DebuggerStepThrough]
    [RoutePrefix("api/chats")]
    public class ChatController : ApiController
    {
        private readonly string connectionString = MiscConstants.ConnectionString;
        private readonly UserRepository userRepository;
        private readonly IChatRepository chatRepository;

        public ChatController()
        {
            userRepository = new UserRepository(connectionString);
            chatRepository = new ChatRepository(connectionString, userRepository);
        }



        /// <summary>
        /// Create new chat.
        /// </summary>
        /// <param name="chat"> 
        /// New chat data. 
        /// Should contain:
        /// - For personal chat (1 or 2 members)
        ///     - Required: Members (provide only id for each member) 
        ///     
        ///     JSON example: 
        ///     {
        ///        "Members":
        ///	       [
        ///	   	       {"Id": "memberId"},
        ///	           {"Id": "memberId"}
        ///        ]
        ///     }
        ///     
        /// - For group chat (more than 2 members) 
        ///     - Required: AdminId, Name, Members (provide only id for each member) 
        ///     - Optional: AvatarId (To add it, you should create an Attachment first)
        ///     
        ///     JSON example: 
        ///     {
        ///        "AdminId": "adminId"
        ///        "Name": "testGroupChat",
        ///        "Members":
        ///	       [
        ///	   	       {"Id": "memberId"},
        ///	           {"Id": "memberId"}
        ///        ]
        ///     }
        /// </param>
        /// <returns> 
        /// Created chat: success
        /// 422 Unprocessable Entity: invalid input 
        /// 500 Internal Server Error: problems with database 
        /// </returns>
        [Route("create")]
        [HttpPost]
        public Chat Create(Chat chat)
        {
            Chat createdChat = null;

            try { createdChat = chatRepository.Create(chat); }
            catch (SqlException)
            {
                var content = $"Failed to create chat.";
                var reasonPhrase = "Internal server error";
                Utility.GenerateResponseMessage(HttpStatusCode.InternalServerError, reasonPhrase, content);
            }

            if (createdChat == null)
            {
                var content = $"Failed to create chat.";
                var reasonPhrase = "Invalid input";
                Utility.GenerateResponseMessage(MiscConstants.UnprocessableEntity, reasonPhrase, content);
            }

            return createdChat;
        }

        /// <summary>
        /// Get chat by id.
        /// </summary>
        /// <param name="chatId"> Chat id </param>
        /// Found chat data: success
        /// 404 Not Found : chat not found
        /// 500 Internal Server Error: problems with database 
        [Route("{chatId:guid}")]
        [HttpGet]
        public Chat Get(Guid chatId)
        {
            Chat foundChat = null;

            try { foundChat = chatRepository.Get(chatId); }
            catch (SqlException)
            {
                var content = $"Failed to find chat.";
                var reasonPhrase = "Internal server error";
                Utility.GenerateResponseMessage(HttpStatusCode.InternalServerError, reasonPhrase, content);
            }

            if (foundChat == null)
            {
                var content = $"Chat does not exist.";
                var reasonPhrase = "Chat not found";
                Utility.GenerateResponseMessage(HttpStatusCode.NotFound, reasonPhrase, content);
            }

            return foundChat;
        }

        /// <summary>
        /// Gets user chats.
        /// </summary>
        /// <param name="userId"> User's id </param>
        /// Found user chats: success
        /// 500 Internal Server Error: problems with database 
        [Route("getUserChats/{userId:guid}")]
        [HttpGet]
        public ReadOnlyCollection<Chat> GetUserChats(Guid userId)
        {
            ReadOnlyCollection<Chat> foundUserChats = null;

            try { foundUserChats = chatRepository.GetUserChats(userId); }
            catch (SqlException)
            {
                var content = $"Failed to find user chats.";
                var reasonPhrase = "Internal server error";
                Utility.GenerateResponseMessage(HttpStatusCode.InternalServerError, reasonPhrase, content);
            }

            return foundUserChats;
        }

        /// <summary>
        /// Delete chat with given id.
        /// </summary>
        /// <param name="chatId"> Chat id </param>
        /// <returns> 
        /// 200 Chat deleted: success
        /// 404 Not Found : chat not found
        /// 500 Internal Server Error: problems with database 
        /// </returns>
        [Route("{chatId:guid}")]
        [HttpDelete]
        public void Delete(Guid chatId)
        {
            bool deleted = false;

            try { deleted = chatRepository.Delete(chatId); }
            catch (SqlException)
            {
                var content = $"Failed to delete chat.";
                var reasonPhrase = "Internal server error";
                Utility.GenerateResponseMessage(HttpStatusCode.InternalServerError, reasonPhrase, content);
            }

            if (deleted)
            {
                var content = $"Chat successfully deleted.";
                var reasonPhrase = "Chat deleted";
                Utility.GenerateResponseMessage(HttpStatusCode.OK, reasonPhrase, content);
            }

            if (!deleted)
            {
                var content = $"Unable to delete chat.";
                var reasonPhrase = "Chat not found";
                Utility.GenerateResponseMessage(HttpStatusCode.NotFound, reasonPhrase, content);
            }
        }
    }
}
