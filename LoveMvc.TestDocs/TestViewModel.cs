using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LoveMvc.TestDocs
{
    public class TestViewModel : IViewViewModelPair
    {
        public TextReader GetViewDocument()
        {
            var name = GetType().FullName;
            name = name.EndsWith("ViewModel") ? name.Substring(0, name.Length - "ViewModel".Length) : name;
            name = name.EndsWith("Model") ? name.Substring(0, name.Length - "Model".Length) : name;

            var targetName = name + ".love.cshtml";

            var assembly = GetType().GetTypeInfo().Assembly;
            var stream = assembly.GetManifestResourceStream(targetName);

            return new StreamReader(stream);
        }

        public object Model
        {
            get { return this; }
        }

        public System.IO.TextReader ViewSource
        {
            get { return GetViewDocument(); }
        }
    }
}
