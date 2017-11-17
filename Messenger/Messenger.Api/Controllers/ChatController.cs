using Messenger.DataLayer;
using Messenger.DataLayer.Sql;
using Messenger.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;


namespace Messenger.Api.Controllers
{
    [RoutePrefix("api/chats")]
    public class ChatController : ApiController
    {
        private readonly string connectionString = InputConstraintsAndDefaultValues.ConnectionString;
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
        /// New chat data. For members add only users ids. 
        /// JSON example: 
        /// {
	    ///     "Name": "testChat",
	    ///     "Members":
        ///	    [
        ///		    {"Id": "id"},
	    ///	        {"Id": "id"}
	    ///     ]
        /// }
        /// </param>
        /// <returns> 
        /// Success: Created chat  
        /// Fail: null
        /// </returns>
        [Route("")]
        [HttpPost]
        public Chat Create(Chat chat)
        {
            bool allChatMemebersFound = PutChatMembersInChat(chat);
            if (!allChatMemebersFound) return null;

            return chatRepository.Create(chat);
        }

        /// <summary>
        /// Get chat by id.
        /// </summary>
        /// <param name="id"> Chat id </param>
        /// <returns> Found chat data or null, if chat not found </returns>
        [Route("{id:guid}")]
        [HttpGet]
        public Chat Get(Guid id)
        {
            return chatRepository.Get(id);
        }

        /// <summary>
        /// Gets user chats.
        /// </summary>
        /// <param name="userId"> User's id </param>
        /// <returns> List of user chats or null, if no chats found </returns>
        [Route("user/{userId:guid}")]
        [HttpGet]
        public ReadOnlyCollection<Chat> GetUserChats(Guid userId)
        {
            return chatRepository.GetUserChats(userId);
        }

        /// <summary>
        /// Delete chat with given id.
        /// </summary>
        /// <param name="id"> Chat id </param>
        [Route("{id:guid}")]
        [HttpDelete]
        public void Delete(Guid id)
        {
            chatRepository.Delete(id);
        }



        private bool PutChatMembersInChat(Chat chat)
        {
            if (chat == null || chat.Members == null) return false;

            List<User> members = new List<User>();
            foreach (User member in chat.Members)
            {
                var foundUser = userRepository.Get(member.Id);
                if (foundUser != null) members.Add(foundUser);
                else return false;
            }

            chat.Members = members.AsReadOnly();
            return true;
        }

    }
}
