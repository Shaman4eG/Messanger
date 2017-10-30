using System;
using System.Data.SqlClient;
using Messenger.Model;
using System.Data;

namespace Messenger.DataLayer.Sql
{
    public class UserRepository : IUserRepository
    {
        private readonly string connectionString;
        private readonly UserRepositoryInputValidator validator;

        public UserRepository(string connectionString)
        {
            this.connectionString = connectionString;
            validator = new UserRepositoryInputValidator();
        }



        public User Create(User user)
        {
            bool valid = validator.ValidateCreate(user);
            if (!valid) return null;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "INSERT INTO [User] ([Id], [Name], [LastName], [Email], [Password], [Avatar]) " +
                        "VALUES (@id, @name, @lastName, @email, @password, @avatar)";

                    user.Id = Guid.NewGuid();
                    command.Parameters.AddWithValue("@id", user.Id);
                    command.Parameters.AddWithValue("@name", user.Name);
                    command.Parameters.AddWithValue("@lastName", user.LastName);
                    command.Parameters.AddWithValue("@email", user.Email);
                    command.Parameters.AddWithValue("@password", user.Password);
                    command.Parameters.AddWithValue("@avatar", user.Avatar);

                    command.ExecuteNonQuery();
                }
            }

            return user;
        }

        public void Delete(Guid id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = 
                        "DELETE FROM [User]" +
                        "WHERE [Id] = @id";
                    command.Parameters.AddWithValue("@id", id);

                    command.ExecuteNonQuery();
                }
            }
        }

        public User Get(Guid id)
        {
            User foundUser = null;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    GetUserQuery(command, id);
                    foundUser = PutUserDataInUser(command);
                }
            }

            return foundUser;
        }



        private void GetUserQuery(SqlCommand command, Guid id)
        {
            command.CommandText =
                "SELECT * " +
                "FROM [User] " +
                "WHERE [Id] = @id";
            command.Parameters.AddWithValue("@id", id);

            command.ExecuteNonQuery();
        }

        private User PutUserDataInUser(SqlCommand command)
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
                        Avatar = reader["Avatar"] == DBNull.Value ? null : (byte[])reader["Avatar"]
                    };
                }
            }

            return user;
        }

    }
}
