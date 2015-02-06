using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LoveMvc
{
    //public static class CloneHelper
    //{
    //    public static T DeepClone<T>(this T a)
    //    {
    //        using (MemoryStream stream = new MemoryStream())
    //        {
    //            DataContractSerializer dcs = new DataContractSerializer(typeof(T));
    //            dcs.WriteObject(stream, a);
    //            stream.Position = 0;
    //            return (T)dcs.ReadObject(stream);
    //        }
    //    }
    //}

    public static class ReflectionHelper
    {
        public static object GetValue(object model, string propertyPath)
        {
            object parent;
            PropertyInfo propInfo;

            return GetValue(model, propertyPath, out parent, out propInfo);
        }

        public static object GetValue(object model, string propertyPath, out object parent, out PropertyInfo propertyInfo)
        {
            // TODO: Fix this
            if (propertyPath == "this")
            {
                parent = null;
                propertyInfo = null;

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

            var deepValue = GetValue(value, nextPath.ReplaceStart(nextPropName, "this"), out parent, out propertyInfo);

            if (deepValue == value)
            {
                parent = model;
                propertyInfo = prop;
            }

            return deepValue;
        }

        public static void SetValue(object model, string propertyPath, Func<Type, object> doGetValueForType)
        {
            object parent;
            PropertyInfo propInfo;
            GetValue(model, propertyPath, out parent, out propInfo);

            var value = doGetValueForType(propInfo.PropertyType);
            propInfo.SetValue(parent, value);
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
            if (string.IsNullOrEmpty(match)) { yield break; }

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
