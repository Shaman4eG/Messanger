using Messenger.Constants;
using Messenger.DataLayer;
using Messenger.DataLayer.Sql;
using Messenger.Model;
using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Net;
using System.Web.Http;

namespace Messenger.Api.Controllers
{
    [RoutePrefix("api/messages")]
    public class MessageController : ApiController
    {
        private readonly string connectionString = MiscConstants.ConnectionString;
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
        /// New message data. 
        /// Should Contain: 
        /// - Required: ChatId, AuthorId, SelfDelition. At least one of these must be provided: Text or AttachmentId.
        /// 
        /// JSON example: 
        /// {
        ///	    "ChatId": "id",
        ///	    "Author": "id",
        ///	    "Text": "test message text",
        ///	    "SelfDelition": "false"
        /// }
        /// </param>
        /// <returns> 
        /// Message sent: success
        /// 422 Unprocessable Entity: invalid input 
        /// 500 Internal Server Error: problems with database 
        /// </returns>
        [Route("send")]
        [HttpPost]
        public Message Send(Message message)
        {
            Message sentMessage = null;

            try { sentMessage = messageRepository.Send(message); }
            catch (SqlException)
            {
                var content = $"Failed to send message. " +
                              $"ChatId = [{message?.ChatId}], " +
                              $"AuthorId = [{message?.AuthorId}], " +
                              $"Text = [{message?.Text}], " +
                              $"AttachmentId = [{message?.AttachmentId}], " +
                              $"SelfDeletion = [{message?.SelfDeletion.ToString()}]";
                var reasonPhrase = "Internal server error";
                Utility.GenerateResponseMessage(HttpStatusCode.InternalServerError, reasonPhrase, content);
            }

            if (sentMessage == null)
            {
                var content = $"Failed to send message. " +
                              $"ChatId = [{message?.ChatId}], " +
                              $"AuthorId = [{message?.AuthorId}], " +
                              $"Text = [{message?.Text}], " +
                              $"AttachmentId = [{message?.AttachmentId}], " +
                              $"SelfDeletion = [{message?.SelfDeletion.ToString()}]";
                var reasonPhrase = "Invalid input";
                Utility.GenerateResponseMessage(MiscConstants.UnprocessableEntity, reasonPhrase, content);
            }

            return sentMessage;
        }

        /// <summary>
        /// Get message by id.
        /// </summary>
        /// <param name="id"> Message id </param>
        /// Found message data: success
        /// 404 Not Found : message not found
        /// 500 Internal Server Error: problems with database 
        [Route("{messageId:guid}")]
        [HttpGet]
        public Message Get(Guid messageId)
        {
            Message foundMessage = null;

            try { foundMessage = messageRepository.Get(messageId); }
            catch (SqlException)
            {
                var content = $"Failed to find message. Id = [{messageId}]";
                var reasonPhrase = "Internal server error";
                Utility.GenerateResponseMessage(HttpStatusCode.InternalServerError, reasonPhrase, content);
            }

            if (foundMessage == null)
            {
                var content = $"Message does not exist. Id = [{messageId}]";
                var reasonPhrase = "Message not found";
                Utility.GenerateResponseMessage(HttpStatusCode.NotFound, reasonPhrase, content);
            }

            return foundMessage;
        }

        /// <summary>
        /// Gets chat messages.
        /// </summary>
        /// <param name="chatId"> Chat id </param>
        /// Found chat messages: success
        /// 500 Internal Server Error: problems with database 
        [Route("getChatMessages/{chatId:guid}")]
        [HttpGet]
        public ReadOnlyCollection<Message> GetChatMessages(Guid chatId)
        {
            ReadOnlyCollection<Message> foundChatMessages = null;

            try { foundChatMessages = messageRepository.GetChatMessages(chatId); }
            catch (SqlException)
            {
                var content = $"Failed to find chat messages. chatId = [{chatId}]";
                var reasonPhrase = "Internal server error";
                Utility.GenerateResponseMessage(HttpStatusCode.InternalServerError, reasonPhrase, content);
            }

            return foundChatMessages;
        }

        /// <summary>
        /// Delete message with given id.
        /// </summary>
        /// <param name="id"> Message id </param>
        /// <returns> 
        /// 200 Message deleted: success
        /// 404 Not Found : message not found
        /// 500 Internal Server Error: problems with database 
        /// </returns>
        [Route("{messageId:guid}")]
        [HttpDelete]
        public void Delete(Guid messageId)
        {
            bool deleted = false;

            try { deleted = messageRepository.Delete(messageId); }
            catch (SqlException)
            {
                var content = $"Failed to delete message. Id = [{messageId}]";
                var reasonPhrase = "Internal server error";
                Utility.GenerateResponseMessage(HttpStatusCode.InternalServerError, reasonPhrase, content);
            }

            if (deleted)
            {
                var content = $"Message successfully deleted. Id = [{messageId}]";
                var reasonPhrase = "Message deleted";
                Utility.GenerateResponseMessage(HttpStatusCode.OK, reasonPhrase, content);
            }

            if (!deleted)
            {
                var content = $"Unable to delete message. Id = [{messageId}]";
                var reasonPhrase = "Message not found";
                Utility.GenerateResponseMessage(HttpStatusCode.NotFound, reasonPhrase, content);
            }
        }
    }
}
