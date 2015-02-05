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

            docs.Add(new TodosViewModel());

            return docs;
        }
    }
}
