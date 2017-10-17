using System;
using System.Data.SqlClient;
using Messenger.Model;

namespace Messenger.DataLayer.Sql
{
    public class UserRepository : IUserRepository
    {
        private readonly string connectionString;

        public UserRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public User Create(User user)
        {
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
                    if (user.Avatar != null) command.Parameters.AddWithValue("@avatar", user.Avatar);
                    else command.Parameters.AddWithValue("@avatar", new byte[0]);
                    command.Parameters.AddWithValue("@password", user.Password);

                    command.ExecuteNonQuery();
                    return user;
                }
            }
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
                    command.CommandText =
                        "SELECT * " +
                        "FROM [User] " +
                        "WHERE [Id] = @id";
                    command.Parameters.AddWithValue("@id", id);

                    command.ExecuteNonQuery();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            foundUser = new User
                            {
                                Id = reader.GetGuid(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                Email = reader.GetString(reader.GetOrdinal("Email")),
                                Password = reader.GetString(reader.GetOrdinal("Name")),
                                Avatar = (byte[])reader["Avatar"]
                            };

                        }
                    }
                }
            }

            return foundUser;
        }
    }
}
