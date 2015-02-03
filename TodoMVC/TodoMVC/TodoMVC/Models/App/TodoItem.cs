using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TodoMVC.Models.App
{
    public class TodoItem
    {
        public int ID { get; set; }
        public string Text { get; set; }
        public bool IsDone { get; set; }
    }
}