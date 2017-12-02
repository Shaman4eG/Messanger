using System;
using System.Data.SqlClient;
using Messenger.Model;
using NLog;
using System.Reflection;

namespace Messenger.DataLayer.Sql
{
    // TODO: Exact information saying why validation failed.
    // TODO: Bigger test coverage
    /// <summary>
    /// Provides methods for manipulations on User entity.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly string connectionString;
        private readonly UserRepositoryInputValidator validator;
        private static readonly Logger logger = LogManager.GetLogger(typeof(UserRepository).FullName); 

        public UserRepository(string connectionString)
        {
            this.connectionString = connectionString;
            validator = new UserRepositoryInputValidator(connectionString, new AttachmentRepository(connectionString), this);
        }



        /// <summary>
        /// Creates user with provided data and saves it to database.
        /// </summary>
        /// <param name="user">
        /// New user data.
        /// - Id is given by system as Guid.New(). 
        /// - AvatarId is optional. 
        /// - Other fields required to be filled by caller.
        /// </param>
        /// <returns> 
        /// Created user: success
        /// Null: invalid input
        /// Throws SqlException: problems with database
        /// </returns>
        public User Create(User user)
        {
            logger.Info($"Attempting to create user. " +
                        $"Name = [{user?.Name}], " +
                        $"LastName = [{user?.LastName}], " +
                        $"Email = [{user?.Email}], " +
                        $"AvatarId = [{user?.AvatarId}]");

            bool valid = false;
            try { valid = validator.ValidateUserInput(user); }
            catch (SqlException ex)
            {
                var userData = $"Name = [{user?.Name}], " +
                               $"LastName = [{user?.LastName}], " +
                               $"Email = [{user?.Email}], " +
                               $"AvatarId = [{user?.AvatarId}]";

                Utility.SqlExceptionHandler(ex, MethodBase.GetCurrentMethod(), userData, logger);
            }
            if (!valid)
            {
                logger.Info($"Failed to create user. Invalid data. " +
                            $"Name = [{user.Name}], " +
                            $"LastName = [{user.LastName}], " +
                            $"Email = [{user.Email}], " +
                            $"AvatarId = [{user.AvatarId}]");

                return null;
            }

            AddUserData(user);

            try { CreateUserQuery(user); }
            catch (SqlException ex) 
            {
                var userData = $"Name = [{user.Name}], " +
                               $"LastName = [{user.LastName}], " +
                               $"Email = [{user.Email}], " +
                               $"AvatarId = [{user.AvatarId}]";

                Utility.SqlExceptionHandler(ex, MethodBase.GetCurrentMethod(), userData, logger);
            }

            logger.Info($"User successfully created. " +
                        $"Id = [{user.Id}], " +
                        $"Name = [{user.Name}], " +
                        $"LastName = [{user.LastName}], " +
                        $"Email = [{user.Email}], " +
                        $"AvatarId = [{user.AvatarId}]");

            return user;
        }

        /// <summary>
        /// Gets user by id.
        /// </summary>
        /// <param name="userId"> User id </param>
        /// <returns> 
        /// Found user data: success
        /// Null: user not found 
        /// Throws SqlException: problems with database
        /// </returns>
        public User Get(Guid userId)
        {
            logger.Info($"Attempting to find user. Id = [{userId}]");

            User foundUser = null;

            try { foundUser = GetUser(userId); }
            catch (SqlException ex)
            {
                Utility.SqlExceptionHandler(ex, MethodBase.GetCurrentMethod(), $"Id = [{userId}]", logger);
            }
            if (foundUser == null)
            {
                logger.Info($"User not found. Id = [{userId}]");
                return null;
            }

            logger.Info($"User found. " +
                        $"Id = [{foundUser.Id}], " +
                        $"Name = [{foundUser.Name}], " +
                        $"LastName = [{foundUser.LastName}], " +
                        $"Email = [{foundUser.Email}], " +
                        $"AvatarId = [{foundUser.AvatarId}]");

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
        /// </returns>
        public bool? Update(User user)
        {
            logger.Info($"Attempting to update user. " +
                        $"Id = [{user?.Id}], " +
                        $"Name = [{user?.Name}], " +
                        $"LastName = [{user?.LastName}], " +
                        $"Email = [{user?.Email}], " +
                        $"AvatarId = [{user?.AvatarId}]");

            User foundUser = null;

            try { foundUser = Get(user.Id); }
            catch (SqlException ex)
            {
                var userData = $"Id = [{user?.Id}], " +
                               $"Name = [{user?.Name}], " +
                               $"LastName = [{user?.LastName}], " +
                               $"Email = [{user?.Email}], " +
                               $"AvatarId = [{user?.AvatarId}]";

                Utility.SqlExceptionHandler(ex, MethodBase.GetCurrentMethod(), userData, logger);
            }
            if (foundUser == null)
            {
                logger.Info($"Unable to update user. Id = [{user?.Id}]");
                return null;
            }

            bool valid = false;
            try { valid = validator.ValidateUserInput(user); }
            catch (SqlException ex)
            {
                var userData = $"Id = [{user?.Id}], " +
                               $"Name = [{user?.Name}], " +
                               $"LastName = [{user?.LastName}], " +
                               $"Email = [{user?.Email}], " +
                               $"AvatarId = [{user?.AvatarId}]";

                Utility.SqlExceptionHandler(ex, MethodBase.GetCurrentMethod(), userData, logger);
            }
            if (!valid)
            {
                logger.Info($"Failed to update user. Invalid data. " +
                            $"Name = [{user?.Name}], " +
                            $"LastName = [{user?.LastName}], " +
                            $"Email = [{user?.Email}], " +
                            $"AvatarId = [{user?.AvatarId}]");

                return false;
            }

            try { UpdateUserQuery(user); }
            catch (SqlException ex)
            {
                var userData = $"Id = [{user.Id}], " +
                               $"Name = [{user.Name}], " +
                               $"LastName = [{user.LastName}], " +
                               $"Email = [{user.Email}], " +
                               $"AvatarId = [{user.AvatarId}]";

                Utility.SqlExceptionHandler(ex, MethodBase.GetCurrentMethod(), userData, logger);
            }

            logger.Info($"User successfully updated. " +
                        $"User data before update: " +
                        $"Id = [{foundUser.Id}], " +
                        $"Name = [{foundUser.Name}], " +
                        $"LastName = [{foundUser.LastName}], " +
                        $"Email = [{foundUser.Email}], " +
                        $"AvatarId = [{foundUser.AvatarId}]. " +
                        $"User data after update: " +
                        $"Id = [{user.Id}], " +
                        $"Name = [{user.Name}], " +
                        $"LastName = [{user.LastName}], " +
                        $"Email = [{user.Email}], " +
                        $"AvatarId = [{user.AvatarId}]");

            return true;
        }

        /// <summary>
        /// Deletes user with given id. 
        /// </summary>
        /// <param name="userId"> User id </param>
        /// <returns>
        /// True: user deleted
        /// False: user not found
        /// Throws SqlException: problems with database
        /// </returns>
        public bool Delete(Guid userId)
        {
            logger.Info($"Attempting to delete user. Id = [{userId}]");

            User foundUser = null;

            try { foundUser = Get(userId); }
            catch (SqlException ex)
            {
                Utility.SqlExceptionHandler(ex, MethodBase.GetCurrentMethod(), $"Id = [{userId}]", logger);
            }
            if (foundUser == null)
            { 
                logger.Info($"Unable to delete user. Id = [{userId}]");
                return false;
            }

            try { DeleteUserQuery(userId); }
            catch (SqlException ex)
            {
                Utility.SqlExceptionHandler(ex, MethodBase.GetCurrentMethod(), $"Id = [{userId}]", logger);
            }

            logger.Info($"User successfully deleted. Id = [{userId}]");
            return true;
        }



        /// <summary>
        /// Gives user unique Id.
        /// </summary>
        /// <param name="user"> User data </param>
        private void AddUserData(User user)
        {
            user.Id = Guid.NewGuid();
        }

        private void CreateUserQuery(User user)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "INSERT INTO [User] ([Id], [Name], [LastName], [Email], [Password], [AvatarId]) " +
                        "VALUES (@id, @name, @lastName, @email, @password, @avatarId)";

                    command.Parameters.AddWithValue("@id", user.Id);
                    command.Parameters.AddWithValue("@name", user.Name);
                    command.Parameters.AddWithValue("@lastName", user.LastName);
                    command.Parameters.AddWithValue("@email", user.Email);
                    command.Parameters.AddWithValue("@password", user.Password);
                    command.Parameters.AddWithValue("@avatarId", user.AvatarId == Guid.Empty ? (object)DBNull.Value : user.AvatarId);

                    command.ExecuteNonQuery();
                }
            }
        }

        private User GetUser(Guid userId)
        {
            User foundUser = null;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    GetUserQuery(command, userId);
                    foundUser = RetrieveUserData(command);
                }
            }

            return foundUser;
        }

        private void GetUserQuery(SqlCommand command, Guid userId)
        {
            command.CommandText =
                "SELECT * " +
                "FROM [User] " +
                "WHERE [Id] = @id";
            command.Parameters.AddWithValue("@id", userId);

            command.ExecuteNonQuery();
        }

        private User RetrieveUserData(SqlCommand command)
        {
            User user = null;

            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    user = new User
                    {
                        Id = reader.GetGuid(reader.GetOrdinal("Id")),
                        Name = reader.GetString(reader.GetOrdinal("Name")),
                        LastName = reader.GetString(reader.GetOrdinal("LastName")),
                        Email = reader.GetString(reader.GetOrdinal("Email")),
                        Password = reader.GetString(reader.GetOrdinal("Password")),
                        AvatarId = reader["AvatarId"] == DBNull.Value ? Guid.Empty : reader.GetGuid(reader.GetOrdinal("AvatarId"))
                    };
                }
            }

            return user;
        }

        private void UpdateUserQuery(User user)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "UPDATE [User] " +
                        "SET [Name] = @name, [LastName] = @lastName, [Email] = @email, [Password] = @password, [AvatarId] = @avatarId " +
                        "WHERE Id = @id ";

                    command.Parameters.AddWithValue("@id", user.Id);
                    command.Parameters.AddWithValue("@name", user.Name);
                    command.Parameters.AddWithValue("@lastName", user.LastName);
                    command.Parameters.AddWithValue("@email", user.Email);
                    command.Parameters.AddWithValue("@password", user.Password);
                    command.Parameters.AddWithValue("@avatarId", user.AvatarId == Guid.Empty ? (object)DBNull.Value : user.AvatarId);

                    command.ExecuteNonQuery();
                }
            }
        }

        private void DeleteUserQuery(Guid userId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "DELETE FROM [User]" +
                        "WHERE [Id] = @id";
                    command.Parameters.AddWithValue("@id", userId);

                    command.ExecuteNonQuery();
                }
            }

        }
    }
}
