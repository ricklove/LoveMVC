using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoveMvc.TestDocs.WebMvc
{
    public static class _TestDocs
    {
        public static List<IViewViewModelPair> GetDocs()
        {
            var docs = new List<IViewViewModelPair>();

            var todosModel = new TodosViewModel();

            docs.Add(GetPair(todosModel, "ForeachWithHelper"));
            docs.Add(GetPair(todosModel, "Helper"));
            docs.Add(GetPair(todosModel, "Foreach"));
            docs.Add(GetPair(todosModel, "PureMarkup"));
            docs.Add(GetPair(todosModel, "NotBinding"));
            docs.Add(GetPair(todosModel, "IfBlock"));
            docs.Add(GetPair(todosModel, "IfInTag"));
            docs.Add(todosModel);

            return docs;
        }

        private static IViewViewModelPair GetPair(object model, string name)
        {
            return new ViewViewModelPair(model, GetViewDocument(name), name);
        }

        private static Func<System.IO.TextReader> GetViewDocument(string name)
        {
            return () =>
            {
                if (!name.StartsWith("LoveMvc.TestDocs.WebMvc"))
                {
                    name = "LoveMvc.TestDocs.WebMvc." + name;
                }

                if (!name.EndsWith(".cshtml"))
                {
                    name = name + ".cshtml";
                }

                return TestViewModel.GetViewDocument(name);
            };
        }
    }
}
