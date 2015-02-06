using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LoveMvc.TestDocs
{
    public class TestViewModel : IViewViewModelPair<TestViewModel>
    {
        public TextReader GetViewDocument()
        {
            var name = GetName();

            var targetName = name + ".cshtml";

            return GetViewDocument(targetName);
        }

        private string GetName()
        {
            var name = GetType().FullName;
            name = name.EndsWith("ViewModel") ? name.Substring(0, name.Length - "ViewModel".Length) : name;
            name = name.EndsWith("Model") ? name.Substring(0, name.Length - "Model".Length) : name;
            return name;
        }

        public static TextReader GetViewDocument(string name)
        {
            var assembly = typeof(Tests.LoveTestFail).GetTypeInfo().Assembly;
            var stream = assembly.GetManifestResourceStream(name);

            return new StreamReader(stream);
        }

        public string Name
        {
            get { return GetName(); }
        }

        public TestViewModel Model
        {
            get { return this; }
        }

        public object ModelUntyped
        {
            get { return this; }
        }

        public System.IO.TextReader ViewSource
        {
            get { return GetViewDocument(); }
        }



    }
}
