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

// TODO: ChatId and UserId restrictions, check other apis for same issue
namespace Messenger.Api.Controllers
{
    [RoutePrefix("api/messages")]
    public class MessageController : ApiController
    {
        private readonly string connectionString = InputConstraintsAndDefaultValues.ConnectionString;
        private readonly UserRepository userRepository;
        private readonly IMessageRepository messageRepository;
        private readonly IAttachmentRepository attachmentRepository;

        public MessageController()
        {
            userRepository = new UserRepository(connectionString);
            attachmentRepository = new AttachmentRepository(connectionString);
            messageRepository = new MessageRepository(connectionString, userRepository);
        }



        /// <summary>
        /// Send message.
        /// </summary>
        /// <param name="message"> 
        /// New message data. For Author add only id. 
        /// JSON example: 
        /// {
        ///	    "ChatId": "id",
        ///	    "Author": "id",
        ///	    "Text": "test message text",
        ///	    "SelfDelition": "false"
        /// }
        /// </param>
        /// <returns> 
        /// Success: Message sent
        /// Fail: null
        /// </returns>
        /// 
        [Route("")]
        [HttpPost]
        public Message Send(Message message)
        {
            return messageRepository.Send(message);
        }

        /// <summary>
        /// Retrieves individual message.
        /// </summary>
        /// <param name="id"> Message id </param>
        /// <returns>
        /// Found: requested message
        /// Not found: null
        /// </returns>
        [Route("{id:guid}")]
        [HttpGet]
        public Message Get(Guid id)
        {
            return messageRepository.Get(id);
        }

        /// <summary>
        /// Retrieves messages of chat with given id.
        /// </summary>
        /// <param name="chatId"> Chat id </param>
        /// <returns>
        /// Found messages
        /// </returns>
        [Route("chat/{chatId:guid}")]
        [HttpGet]
        public ReadOnlyCollection<Message> GetUserChats(Guid chatId)
        {
            return messageRepository.GetChatMessages(chatId);
        }

        /// <summary>
        /// Delete message with given id.
        /// </summary>
        /// <param name="id"> Message id </param>
        [Route("{id:guid}")]
        [HttpDelete]
        public void Delete(Guid id)
        {
            messageRepository.Delete(id);
        }
    }
}
