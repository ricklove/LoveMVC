using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoveMvc.TestDocs.WebMvc
{
    public class TodosViewModel : TestViewModel
    {
        public List<TodoItemViewModel> Todos { get; set; }

        [Computed]
        [ChangesTemplate]
        public bool HasTodos { get { return Todos.Count > 0; } }

        [Computed]
        [JsGet("this.Tasks.every(function(t){ return t.IsDone; })")]
        [JsSet("this.Tasks.forEach(function(t){ t.IsDone = value;})")]
        public bool AreAllDone
        {
            get
            {
                return Todos.All(t => t.TodoItem.IsDone);
            }
            set
            {
                foreach (var t in Todos)
                {
                    t.TodoItem.IsDone = value;
                }
            }
        }

        public string NewTodoText { get; set; }


        [Required]
        [EmailAddress]
        public string EmailBlank { get; set; }

        [Required]
        [EmailAddress]
        public string EmailOK { get; set; }

        [Required]
        [EmailAddress]
        public string EmailError { get; set; }


        public TodosViewModel()
        {
            NewTodoText = "New Do This!";
            EmailOK = "rick@toldpro.com";
            EmailError = "rick";
            EmailBlank = "";

            Todos = new List<TodoItemViewModel>();
            Todos.Add(new TodoItemViewModel() { TodoItem = new TodoItem() { ID = 1, IsDone = false, Text = "Do this 1!" } });
            //Todos.Add(new TodoItemViewModel() { TodoItem = new TodoItem() { ID = 2, IsDone = false, Text = "Do this 2!" } });
            //Todos.Add(new TodoItemViewModel() { TodoItem = new TodoItem() { ID = 3, IsDone = false, Text = "Do this 3!" } });
        }
    }

    public class TodoItemViewModel
    {
        public TodoItem TodoItem { get; set; }
    }

    public class TodoItem
    {
        public int ID { get; set; }
        public string Text { get; set; }
        public bool IsDone { get; set; }
    }
}
