using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoveMvc
{
    public static class ReflectionHelper
    {
        private static object GetValue(object model, string propertyPath)
        {
            if (propertyPath == "this")
            {
                return model;
            }

            if (!propertyPath.StartsWith("this."))
            {
                throw new ArgumentException("binding must start with 'this.'");
            }

            // Get property
            var nextPath = propertyPath.Substring("this.".Length);
            var dotIndex = nextPath.IndexOf(".");
            var nextPropName = dotIndex >= 0 ? nextPath.Substring(dotIndex) : nextPath;

            var t = model.GetType();
            var prop = t.GetProperty(nextPropName);

            if (prop == null)
            {
                throw new ArgumentException("Binding not found in model: property:" + nextPropName + " model:" + model.ToString());
            }

            var value = prop.GetValue(model);

            return GetValue(value, nextPath.ReplaceStart(nextPropName, "this"));
        }
    }

    public static class StringHelper
    {
        public static string ReplaceStart(this string text, string match, string replacement)
        {
            if (!text.StartsWith(match))
            {
                throw new ArgumentException("text must start with target: text:" + text + " target:" + match);
            }

            return replacement + text.Substring(match.Length);
        }

        public static IEnumerable<int> Find(this string text, string match)
        {
            var index = -1;
            index = text.IndexOf(match);

            while (index >= 0)
            {
                yield return index;
                index = text.IndexOf(match, index + match.Length);
            }
        }
    }
}
