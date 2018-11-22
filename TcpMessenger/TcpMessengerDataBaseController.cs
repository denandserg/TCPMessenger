using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpMessenger
{
    public class TcpMessengerDataBaseController
    {
        private string _connStr;

        public TcpMessengerDataBaseController(string connStr = @"Data Source=dwayne-pc;Initial Catalog=TcpMessengerDB;Integrated Security=True")
        {
            _connStr = connStr;
        }

        #region User controllers
        public void AddUser(string name, string login, string pwd)
        {
            string sqlExpression = "spr_AddUser";
            using (SqlConnection connection = new SqlConnection(_connStr))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);

                command.CommandType = CommandType.StoredProcedure;
                SqlParameter nameParam = new SqlParameter
                {
                    ParameterName = "@name",
                    Value = name
                };
                SqlParameter loginParam = new SqlParameter
                {
                    ParameterName = "@login",
                    Value = login
                };
                SqlParameter pwdParam = new SqlParameter
                {
                    ParameterName = "@pwd",
                    Value = pwd
                };
                command.Parameters.Add(nameParam);
                command.Parameters.Add(loginParam);
                command.Parameters.Add(pwdParam);

                var res = command.ExecuteScalar();
            }
        }

        public List<UserObj> GetUsers()
        {
            string sqlExpression = "spr_GetAllUsers";
            List<UserObj> list = new List<UserObj>();

            using (SqlConnection connection = new SqlConnection(_connStr))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                command.CommandType = CommandType.StoredProcedure;
                var reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        string name = reader.GetString(1);
                        string login = reader.GetString(2);
                        string pwd = reader.GetString(3);
                        list.Add(new UserObj(id, name, login, pwd));
                    }
                }
                reader.Close();
                
            }

            return list;
        }
        #endregion

        #region Message controllers
        public void UpdateMessage(int id, string dateOut)
        {
            string sqlExpression = "spr_UpdateMessage";
            using (SqlConnection connection = new SqlConnection(_connStr))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                SqlCommand commandInsertIdentityOn = new SqlCommand("SET IDENTITY_INSERT MessageObjs ON", connection);
                commandInsertIdentityOn.ExecuteNonQuery();
                command.CommandType = CommandType.StoredProcedure;
                SqlParameter idParam = new SqlParameter
                {
                    ParameterName = "@id",
                    Value = id
                };
                SqlParameter dateOutParam = new SqlParameter
                {
                    ParameterName = "@dateOut",
                    Value = dateOut
                };
                
                command.Parameters.Add(idParam);
                command.Parameters.Add(dateOutParam);

                var res = command.ExecuteScalar();
            }
        }

        public List<MessageObj> GetMessages()
        {
            string sqlExpression = "spr_GetAllMessages";
            List<MessageObj> list = new List<MessageObj>();

            using (SqlConnection connection = new SqlConnection(_connStr))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                command.CommandType = CommandType.StoredProcedure;
                var reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        string message = reader.GetString(1);
                        string dateIn = reader.GetString(2);
                        string dateOut = reader.GetString(3);
                        int senderId = reader.GetInt32(4);
                        int rcptId = reader.GetInt32(5);
                        list.Add(new MessageObj(id, message, dateIn, dateOut, senderId, rcptId));
                    }
                }
                reader.Close();

            }

            return list;
        }

        public void AddMessage(string messg, string dateIn, int senderId, int rcptId)
        {
            string sqlExpression = "spr_AddMessage";
            using (SqlConnection connection = new SqlConnection(_connStr))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);

                command.CommandType = CommandType.StoredProcedure;
                SqlParameter msgParam = new SqlParameter
                {
                    ParameterName = "@message",
                    Value = messg
                };
                SqlParameter dateInParam = new SqlParameter
                {
                    ParameterName = "@dateIn",
                    Value = dateIn
                };
                SqlParameter dateOutParam = new SqlParameter
                {
                    ParameterName = "@dateOut",
                    Value = "--:--:--"
                };
                SqlParameter senderIdParam = new SqlParameter
                {
                    ParameterName = "@senderId",
                    Value = senderId
                };
                SqlParameter rcptIdParam = new SqlParameter
                {
                    ParameterName = "@rcptId",
                    Value = rcptId
                };
                command.Parameters.Add(msgParam);
                command.Parameters.Add(dateInParam);
                command.Parameters.Add(dateOutParam);
                command.Parameters.Add(senderIdParam);
                command.Parameters.Add(rcptIdParam);

                var res = command.ExecuteScalar();
            }
        }

        #endregion
    }

    public class UserObj
    {
        public string Name { get; set; }
        public string Login { get; set; }
        public string Pwd { get; set; }
        public int Id { get; set; }

        public UserObj(int id, string name, string login, string pwd)
        {
            Id = id;
            Name = name;
            Login = login;
            Pwd = pwd;
        }
    }

    public class MessageObj
    {
        public int  Id { get; set; }
        public string Messg { get; set; }
        public string DateIn { get; set; }
        public string DateOut { get; set; }
        public int SenderId { get; set; }
        public int RcptId { get; set; }

        public MessageObj(int id, string messg, string dateIn, string dateOut, int senderId, int rcptId)
        {
            Id = id;
            Messg = messg;
            DateIn = dateIn;
            DateOut = dateOut;
            SenderId = senderId;
            RcptId = rcptId;
        }
    }
}
