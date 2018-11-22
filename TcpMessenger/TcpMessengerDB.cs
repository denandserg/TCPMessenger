namespace TcpMessenger
{
    using System;
    using System.Data.Entity;
    using System.Linq;

    public class TcpMessengerDB : DbContext
    {
        public TcpMessengerDB()
            : base("name=TcpMessengerDB")
        {
        }

        public virtual DbSet<UserObj> UserObjs { get; set; }
        public virtual DbSet<MessageObj> MessageObjs { get; set; }
    }

    
}