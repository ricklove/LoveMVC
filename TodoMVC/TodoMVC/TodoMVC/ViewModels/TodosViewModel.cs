using DelegateDecompiler;
using System.Collections.Generic;
using TodoMVC.Models.App;
using System.Linq;
using LoveMVC;

namespace TodoMVC.ViewModels
{
    public class TodosViewModel
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
    }

    public class TodoItemViewModel
    {
        public TodoItem TodoItem { get; set; }
    }
}