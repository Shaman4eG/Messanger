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
    [RoutePrefix("api/attachments")]
    public class AttachmentController : ApiController
    {
        private readonly string connectionString = MiscConstants.ConnectionString;
        private readonly IAttachmentRepository attachmentRepository;

        public AttachmentController()
        {
            attachmentRepository = new AttachmentRepository(connectionString);
        }



        /// <summary>
        /// Creates attachment.
        /// </summary>
        /// <param name="attachment"> 
        /// New attachment data.
        /// Should contain:
        /// - Required: Type, File
        /// 
        /// JSON example:
        /// {
        ///	    "Type": "png",
        ///	    "File": "0x010203"
        /// }
        /// </param>
        /// <returns>
        /// Created attachment: success 
        /// 422 Unprocessable Entity: invalid input
        /// 500 Internal Server Error: problems with database 
        /// </returns>
        [Route("create")]
        [HttpPost]
        public Attachment Create(Attachment attachment)
        {
            Attachment createdAttachment = null;

            try { createdAttachment = attachmentRepository.Create(attachment); }
            catch (SqlException)
            {
                var content = $"Failed to create attachment. " +
                              $"Type = [{attachment.Type}]";
                var reasonPhrase = "Internal server error";
                Utility.GenerateResponseMessage(HttpStatusCode.InternalServerError, reasonPhrase, content);
            }

            if (createdAttachment == null)
            {
                var content = $"Failed to create attachment. " +
                              $"Type = [{attachment.Type}]";
                var reasonPhrase = "Invalid input";
                Utility.GenerateResponseMessage(MiscConstants.UnprocessableEntity, reasonPhrase, content);
            }

            return createdAttachment;
        }

        /// <summary>
        /// Gets attachment by id.
        /// </summary>
        /// <param name="attachmentId"> Attachment id </param>
        /// <returns> 
        /// Found attachment data: success 
        /// 404 Not Found: attachment not found 
        /// 500 Internal Server Error: problems with database 
        /// </returns>
        [Route("{attachmentId:guid}")]
        [HttpGet]
        public Attachment Get(Guid attachmentId)
        {
            Attachment foundAttachment = null;

            try { foundAttachment = attachmentRepository.Get(attachmentId); }
            catch (SqlException)
            {
                var content = $"Failed to find attachment. Id = [{attachmentId}]";
                var reasonPhrase = "Internal server error";
                Utility.GenerateResponseMessage(HttpStatusCode.InternalServerError, reasonPhrase, content);
            }

            if (foundAttachment == null)
            {
                var content = $"Attachment does not exist. Id = [{attachmentId}]";
                var reasonPhrase = "Attachment not found";
                Utility.GenerateResponseMessage(HttpStatusCode.NotFound, reasonPhrase, content);
            }

            return foundAttachment;
        }

        /// <summary>
        /// Deletes attachment with given id.
        /// </summary>
        /// <param name="attachmentId"> Attachment id </param>
        /// <returns> 
        /// 200 Attachment deleted: success
        /// 404 Not Found : attachment not found
        /// 500 Internal Server Error: problems with database 
        /// </returns>
        [Route("{attachmentId:guid}")]
        [HttpDelete]
        public void Delete(Guid attachmentId)
        {
            bool deleted = false;
            
            try { deleted = attachmentRepository.Delete(attachmentId); }
            catch (SqlException)
            {
                var content = $"Failed to delete attachment. Id = [{attachmentId}]";
                var reasonPhrase = "Internal server error";
                Utility.GenerateResponseMessage(HttpStatusCode.InternalServerError, reasonPhrase, content);
            }

            if (deleted)
            {
                var content = $"Attachment successfully deleted. Id = [{attachmentId}]";
                var reasonPhrase = "Attachment deleted";
                Utility.GenerateResponseMessage(HttpStatusCode.OK, reasonPhrase, content);
            }

            if (!deleted)
            {
                var content = $"Unable to delete attachment. Id = [{attachmentId}]";
                var reasonPhrase = "Attachment not found";
                Utility.GenerateResponseMessage(HttpStatusCode.NotFound, reasonPhrase, content);
            }
        }
    }
}
