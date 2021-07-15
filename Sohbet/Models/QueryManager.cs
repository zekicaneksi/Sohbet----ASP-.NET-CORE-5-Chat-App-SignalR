using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Sohbet.Models
{
    public class QueryManager
    {
        public IConfiguration Configuration { get; }
        string connectionString;
        public QueryManager(IConfiguration configuration)
        {
            Configuration = configuration;
            connectionString = configuration.GetConnectionString("sohbetdb");
        }

        public enum AddUserReturnMessage
        {
            Success,
            NickInUse,
            InvalidInput,
            UnknownError
        }
        public AddUserReturnMessage AddUser(UserModel user)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    //check the Nick and pass for invalid characters
                    string allowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                    int maxLength = 15, minLength = 6;

                    if (user.Nick == null || user.Password == null)
                        return AddUserReturnMessage.InvalidInput;

                    if ((user.Nick.Length < minLength) || (user.Password.Length < minLength) || (user.Nick.Length > maxLength) || (user.Password.Length > maxLength))
                        return AddUserReturnMessage.InvalidInput;

                    foreach (char ch in user.Nick)
                    {
                        if (!allowedChars.Contains(ch))
                            return AddUserReturnMessage.InvalidInput;
                    }

                    foreach (char ch in user.Password)
                    {
                        if (!allowedChars.Contains(ch))
                            return AddUserReturnMessage.InvalidInput;
                    }

                    connection.Open();

                    //checking if nick already exists
                    SqlCommand cmd = new SqlCommand(@"SELECT Count(Nick) from Users WHERE Nick=@Nick", connection);
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@Nick", SqlDbType.NVarChar);
                    cmd.Parameters["@Nick"].Value = user.Nick;

                    Int32 result = (Int32)cmd.ExecuteScalar();

                    
                    if(result != 0)
                    {
                        //Nick is in use
                        return AddUserReturnMessage.NickInUse;
                    }
                    else
                    {
                        //Adding the user
                        cmd.CommandText = @"INSERT INTO Users(Nick, Password) VALUES(@Nick, @Password)";
                        cmd.Parameters.Add("@Password", SqlDbType.NVarChar);
                        cmd.Parameters["@Password"].Value = user.Password;

                        if (cmd.ExecuteNonQuery()>=1)
                        {
                            return AddUserReturnMessage.Success;
                        }
                        else
                        {
                            return AddUserReturnMessage.UnknownError;
                        }
                        
                    }
                }

            }
            catch (Exception)
            {
                return AddUserReturnMessage.UnknownError;
            }
        }
        public enum ValidateUserReturnMessage
        {
            Success,
            NotFound,
            UnknownError
        }
        public ValidateUserReturnMessage ValidateUser(UserModel user)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {

                    //check the Nick and pass for invalid characters
                    string allowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                    int maxLength = 15, minLength = 6;

                    if(user.Nick == null || user.Password == null)
                        return ValidateUserReturnMessage.NotFound;

                    if ((user.Nick.Length < minLength) || (user.Password.Length < minLength) || (user.Nick.Length > maxLength) || (user.Password.Length > maxLength))
                        return ValidateUserReturnMessage.NotFound;

                    foreach (char ch in user.Nick)
                    {
                        if (!allowedChars.Contains(ch))
                            return ValidateUserReturnMessage.NotFound;
                    }

                    foreach (char ch in user.Password)
                    {
                        if (!allowedChars.Contains(ch))
                            return ValidateUserReturnMessage.NotFound;
                    }

                    connection.Open();

                    //checking if nick already exists
                    SqlCommand cmd = new SqlCommand(@"SELECT Count(Nick) from Users WHERE Nick=@Nick AND Password=@Password", connection);
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@Nick", SqlDbType.NVarChar);
                    cmd.Parameters.Add("@Password", SqlDbType.NVarChar);

                    cmd.Parameters["@Nick"].Value = user.Nick;
                    cmd.Parameters["@Password"].Value = user.Password;

                    Int32 result = (Int32)cmd.ExecuteScalar();

                    if (result == 0)
                    {
                        return ValidateUserReturnMessage.NotFound;
                    }
                    else
                    {
                        return ValidateUserReturnMessage.Success;   
                    }
                }
            }
            catch (Exception)
            {
                return ValidateUserReturnMessage.UnknownError;
            }
        }
    }
}
