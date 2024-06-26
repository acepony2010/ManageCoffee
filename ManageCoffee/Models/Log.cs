﻿using System;
using System.Collections.Generic;
using ManageCoffee.DAO;

#nullable disable

namespace ManageCoffee.Models
{
    public partial class Log
    {
        public int LogId { get; set; }
        public int UserId { get; set; }
        public string Action { get; set; }
        public string Object { get; set; }
        public int ObjectId { get; set; }
        public DateTime? CreatedAt { get; set; }

        public virtual User User { get; set; }

        public string getUserName()
        {
            UserDAO table = new UserDAO();
            var name = table.GetUserByID(this.UserId).Name;
            return name;
        }
    }
}
