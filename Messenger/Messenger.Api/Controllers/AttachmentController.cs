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
    [RoutePrefix("api/attachments")]
    public class AttachmentController : ApiController
    {
        private readonly string connectionString = InputConstraintsAndDefaultValues.ConnectionString;
        private readonly IAttachmentRepository attachmentRepository;

        public AttachmentController()
        {
            attachmentRepository = new AttachmentRepository(connectionString);
        }



        /// <summary>
        /// Creates attachment. Can be embedded to message later.
        /// </summary>
        /// <param name="attachment"> 
        /// New attachment. 
        /// JSON example:
        /// {
        ///	    "Type": "png",
        ///	    "File": "0x010203"
        /// }
        /// </param>
        /// <returns>
        /// Success: created attachment
        /// Failure: null
        /// </returns>
        [Route("")]
        [HttpPost]
        public Attachment Create(Attachment attachment)
        {
            return attachmentRepository.Create(attachment);
        }

        /// <summary>
        /// Gets the attachment
        /// </summary>
        /// <param name="id"> Attachment id </param>
        /// <returns> 
        /// Found: Attachment 
        /// Not found: null
        /// </returns>
        [Route("{id:guid}")]
        [HttpGet]
        public Attachment Get(Guid id)
        {
            return attachmentRepository.Get(id);
        }

        /// <summary>
        /// Deletes attachment with specified id.
        /// </summary>
        /// <param name="id"></param>
        [Route("{id:guid}")]
        [HttpDelete]
        public void Delete(Guid id)
        {
            attachmentRepository.Delete(id);
        }
    }
}
