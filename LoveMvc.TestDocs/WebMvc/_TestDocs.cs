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

            docs.Add(new ViewViewModelPair(todosModel, GetViewDocument("PureMarkup")));
            docs.Add(new ViewViewModelPair(todosModel, GetViewDocument("NotBinding")));
            docs.Add(todosModel);

            return docs;
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
