using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoveMVC
{
    [AttributeUsage(AttributeTargets.Property)]
    public class JsGetAttribute : Attribute
    {
        public string Javascript { get; set; }

        public JsGetAttribute(string javascript)
        {
            Javascript = javascript;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class JsSetAttribute : Attribute
    {
        public string Javascript { get; set; }

        public JsSetAttribute(string javascript)
        {
            Javascript = javascript;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ChangesTemplateAttribute : Attribute
    {
    }
}
