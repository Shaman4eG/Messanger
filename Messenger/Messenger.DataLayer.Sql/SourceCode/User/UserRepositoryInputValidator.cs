using Messenger.Constants;
using Messenger.Model;
using NLog;
using System;
using System.Data.SqlClient;
using System.Reflection;
using System.Web.Http;

namespace Messenger.DataLayer.Sql
{
    class UserRepositoryInputValidator
    {
        private readonly string connectionString;
        private readonly AttachmentRepository attachmentRepository;
        private readonly UserRepository userRepository;
        private static readonly Logger logger = LogManager.GetLogger(typeof(UserRepository).FullName);

        public UserRepositoryInputValidator(
            string connectionString,
            AttachmentRepository attachmentRepository, 
            UserRepository userRepository)
        {
            this.connectionString = connectionString;
            this.attachmentRepository = attachmentRepository;
            this.userRepository = userRepository;
        }



        public bool ValidateUserInput(User user)
        {
            return CheckNull(user) &&
                   CheckRange(user) &&
                   CheckEmailUniqueness(user) &&
                   CheckAvatarAttachmentExistens(user.AvatarId);
        }



        private bool CheckNull(User user)
        {
            return user          != null &&
                   user.Name     != null &&
                   user.LastName != null &&
                   user.Email    != null &&
                   user.Password != null;
        }

        private bool CheckRange(User user)
        {
            return user.Name.Length     >= UserInputConstraints.MinUserNameLength &&
                   user.Name.Length     <= UserInputConstraints.MaxUserNameLength &&
                   user.LastName.Length >= UserInputConstraints.MinUserLastNameLength &&
                   user.LastName.Length <= UserInputConstraints.MaxUserLastNameLength &&
                   user.Email.Length    >= UserInputConstraints.MinUserEmailLength &&
                   user.Email.Length    <= UserInputConstraints.MaxUserEmailLength &&
                   user.Password.Length >= UserInputConstraints.MinUserPasswordLength &&
                   user.Password.Length <= UserInputConstraints.MaxUserPasswordLength;
        }

        private bool CheckEmailUniqueness(User user)
        {
            bool emailUnchanged = false;
            if (user.Id != Guid.Empty) emailUnchanged = CheckIfEmailUnchanged(user);
            if (emailUnchanged) return true;

            int numberOfSameEmailsInDb = 0;

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        GetNumberOfSameEmailsQuery(command, user.Email);
                        numberOfSameEmailsInDb = RetrieveEmailNumber(command);
                    }
                }
            }
            catch (SqlException ex)
            {
                var userInfo = $"Name = [{user.Name}], " +
                               $"LastName = [{user.LastName}], " +
                               $"Email = [{user.Email}], " +
                               $"AvatarId = [{user.AvatarId}]";

                Utility.SqlExceptionHandler(ex, MethodBase.GetCurrentMethod(), userInfo, logger);
            }

            if (numberOfSameEmailsInDb > 0) return false;
            else return true;
        }

        /// <summary>
        /// Checks that email was unchanged and only other user data is changed, 
        /// preventing errors due to ununiqueness. Only for Update().
        /// </summary>
        private bool CheckIfEmailUnchanged(User user)
        {
            var userToUpdate = userRepository.Get(user.Id);

            if (userToUpdate.Email == user.Email) return true;
            else return false;
        }

        private void GetNumberOfSameEmailsQuery(SqlCommand command, string email)
        {
            command.CommandText =
                "SELECT COUNT([Email]) AS Email " +
                "FROM [User] " +
                "WHERE [Email] = @email";
            command.Parameters.AddWithValue("@email", email);

            command.ExecuteNonQuery();
        }

        private int RetrieveEmailNumber(SqlCommand command)
        {
            int numberOfSameEmailsFoundInDb = 0;

            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    numberOfSameEmailsFoundInDb = reader.GetInt32(reader.GetOrdinal("Email"));
                }
            }

            return numberOfSameEmailsFoundInDb;
        }

        private bool CheckAvatarAttachmentExistens(Guid avatarId)
        {
            if (avatarId == Guid.Empty) return true;

            Attachment foundAttachment = null;

            try { foundAttachment = attachmentRepository.Get(avatarId); }
            catch (SqlException ex)
            {
                Utility.SqlExceptionHandler(ex, MethodBase.GetCurrentMethod(), $"Id = [{avatarId}]", logger);
            }

            if (foundAttachment == null) return false;
            else return true;
        }
    }
}
