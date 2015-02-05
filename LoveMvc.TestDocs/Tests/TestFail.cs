using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace LoveMvc.TestDocs.Tests
{
    public class LoveTestFail : Exception
    {
        public string TestName { get; set; }
        public LoveNode Node { get; private set; }
        public string Message { get; private set; }

        public LoveTestFail(LoveNode node, string message)
        {
            Node = node;
            Message = message;

            // FROM: http://stackoverflow.com/questions/9551780/how-to-get-property-name-when-it-uses-yield-return
            // Hack to get stack trace inside yield return
            var method = new StackTrace(true).GetFrame(1).GetMethod();

            if (method.Name == "MoveNext")
            {
                var methodType = method.DeclaringType.Name;
                TestName = System.Text.RegularExpressions.Regex.Replace(methodType, @".*<([^)]+)>.*", "$1");
            }
            else
            {
                TestName = method.Name;
            }

        }

        public override string ToString()
        {
            var testName = TestName;

            return string.Format("FAIL: {0}: '{2}' {1}", testName, Message, (Node is LoveSpan) ? (Node as LoveSpan).Content : "");
        }
    }
}
