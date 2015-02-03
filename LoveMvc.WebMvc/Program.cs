using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LoveMvc.WebMvc
{
    class Program
    {
        static void Main(string[] args)
        {
            var sourcePath = Path.Combine(Directory.GetCurrentDirectory() + @"\..\..\TestDocs\Todos.love.cshtml");
            sourcePath = Path.GetFullPath(sourcePath);

            var parser = new RazorParser();
            var results = parser.Parse(new StreamReader(sourcePath));
        }
    }
}
