using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LoveMvc.TestDocs.Tests
{
    public class CommonTests
    {
        public static IEnumerable<Exception> RunTests(LoveTemplate template, IViewViewModelPair viewViewModelPair)
        {
            //var tests = new List<Func<LoveTemplate, object, IEnumerable<Exception>>>() { 
            //    Test_DoesNotContainMarkupExpressions,
            //    Test_AllBindingsMapToViewModel,
            //    Test_DoesNotContainViewModelReferencesInMarkup
            //};

            var delegateType = typeof(Func<LoveTemplate, IViewViewModelPair, IEnumerable<Exception>>);
            var tests = typeof(CommonTests).GetMethods()
                .Where(m => m.Name != "RunTests")
                .Select(m => Delegate.CreateDelegate(delegateType, m, false) as Func<LoveTemplate, IViewViewModelPair, IEnumerable<Exception>>)
                .Where(d => d != null)
                .ToList();

            foreach (var test in tests)
            {
                foreach (var ex in test(template, viewViewModelPair))
                {
                    yield return ex;
                }
            }

        }

        public static IEnumerable<Exception> Test_DoesNotContainMarkupExpressions(LoveTemplate template, IViewViewModelPair viewViewModelPair)
        {
            var model = viewViewModelPair.Model;

            foreach (var n in template.Flatten())
            {
                if (n is LoveMarkupExpression)
                {
                    yield return new LoveTestFail(n, "is Markup Expression");
                }
            }
        }

        public static IEnumerable<Exception> Test_AllBindingsMapToViewModel(LoveTemplate template, IViewViewModelPair viewViewModelPair)
        {
            var model = viewViewModelPair.Model;

            var propertyMappings = GetPropertyMappings(model.GetType()).ToList();

            var scopes = new string[] { "Model" }.ToDictionary(s => s, s => new HashSet<string>(propertyMappings.Select(p => s + "." + p)));

            foreach (var n in template.Flatten())
            {
                if (n is LoveBinding)
                {
                    var exp = (n as LoveBinding).Content;
                    var validScopes = new List<string>() { "Model" };

                    // Create Scopes
                    foreach (var lScope in n.GetScopes())
                    {
                        var lName = lScope.Name.Content;
                        if (scopes.ContainsKey(lName))
                        {
                            validScopes.Add(lName);
                            continue;
                        }

                        var lExp = lScope.Expression.Content;

                        if (lScope.ScopeType == LoveScopeType.Foreach)
                        {
                            lExp = lExp + ".Item";
                        }
                        else
                        {
                            throw new NotImplementedException();
                            //mappingsInScope.Add(lName + m.Substring(lExp.Length));
                        }

                        var mappingsInScope = new HashSet<string>();

                        foreach (var s in validScopes)
                        {
                            foreach (var m in scopes[s])
                            {
                                if (m.StartsWith(lExp))
                                {
                                    mappingsInScope.Add(lName + m.Substring(lExp.Length));
                                }
                            }
                        }

                        if (mappingsInScope.Count > 0)
                        {
                            validScopes.Add(lName);
                            scopes.Add(lName, mappingsInScope);
                        }
                    }

                    // Find mapping in scope
                    var foundMapping = false;

                    foreach (var s in validScopes)
                    {
                        if (scopes[s].Contains(exp))
                        {
                            foundMapping = true;
                            break;
                        }
                    }

                    if (!foundMapping)
                    {
                        yield return new LoveTestFail(n, "does Not Map to View Model");
                    }
                }
            }
        }

        private static IEnumerable<string> GetPropertyMappings(Type t)
        {
            foreach (var prop in t.GetProperties())
            {
                yield return prop.Name;

                foreach (var propInner in GetPropertyMappings(prop.PropertyType))
                {
                    yield return prop.Name + "." + propInner;
                }
            }
        }

        private static IEnumerable<string> GetPropertyValues(object model, HashSet<object> visitHistory = null, int depth = 0, int maxDepth = 16, Assembly definedAssembly = null)
        {
            if (depth > maxDepth)
            {
                yield break;
            }

            if (visitHistory == null)
            {
                visitHistory = new HashSet<object>();
            }

            if (visitHistory.Contains(model))
            {
                yield break;
            }

            visitHistory.Add(model);

            var t = model.GetType();

            if (definedAssembly == null)
            {
                definedAssembly = t.Assembly;
            }

            // Don't get property values from types defined outside the assembly
            // Their own value should be returned
            if (t.Assembly != definedAssembly)
            {
                // Skip this
                //yield break;
            }

            foreach (var prop in t.GetProperties())
            {
                object val = null;
                string valStr = null;

                try
                {
                    val = prop.GetValue(model);

                    if (val.GetType().GetMethod("ToString", System.Type.EmptyTypes).DeclaringType != typeof(object))
                    {
                        valStr = val.ToString();
                    }
                }
                catch
                {
                    val = null;
                    valStr = null;
                }

                if (!string.IsNullOrWhiteSpace(valStr))
                {
                    yield return valStr;
                }

                if (val != null)
                {
                    foreach (var valueInner in GetPropertyValues(val, visitHistory, depth + 1, maxDepth, definedAssembly))
                    {
                        yield return valueInner;
                    }

                    if (val is System.Collections.IEnumerable)
                    {
                        foreach (var item in (System.Collections.IEnumerable)val)
                        {
                            foreach (var valueInner in GetPropertyValues(item, visitHistory, depth + 2, maxDepth, definedAssembly))
                            {
                                yield return valueInner;
                            }
                        }
                    }
                }

            }
        }

        //public static IEnumerable<Exception> Test_DoesNotContainViewModelReferencesInMarkup(LoveTemplate template, IViewViewModelPair viewViewModelPair)
        //{
        //    var model = viewViewModelPair.Model;
        //    var propertyMappings = GetPropertyMappings(model.GetType());
        //    var propertyNames = propertyMappings.SelectMany(p => p.Split('.'));
        //    var propertyLookups = propertyNames.Select(p=> "." + p);

        //    foreach (var n in template.Flatten())
        //    {
        //        if (n is LoveMarkup)
        //        {
        //            var nMarkup = n as LoveMarkup;
        //            if (propertyLookups.Any(p => nMarkup.Content.Contains(p)))
        //            {
        //                yield return new LoveTestFail(n, "has View Model Reference in Markup");
        //            }
        //        }
        //    }
        //}

        public static IEnumerable<Exception> Test_DoesNotContainViewModelValueInMarkup(LoveTemplate template, IViewViewModelPair viewViewModelPair)
        {
            var model = viewViewModelPair.Model;
            var values = GetPropertyValues(model)
                .Where(v => v.Length != 1 || v.ToInt() > 1)
                .Where(v => v != "True" && v != "False")
                .ToList();

            foreach (var n in template.Flatten())
            {
                if (n is LoveMarkup)
                {
                    var nMarkup = n as LoveMarkup;
                    if (values.Any(v => nMarkup.Content.Contains(v)))
                    {
                        yield return new LoveTestFail(n, "has View Model Value in Markup");
                    }
                }
            }
        }

        //public static IEnumerable<Exception> Test_DoesRenderedViewEqualConstructedView(LoveTemplate template, IViewViewModelPair viewViewModelPair)
        //{
        //    //Providers.ViewRenderer.RenderView()
        //}

    }

    public static class TestHelpers
    {
        public static int? ToInt(this string text)
        {
            var val = 0;
            if (int.TryParse(text, out val))
            {
                return val;
            }
            else
            {
                return null;
            }
        }
    }
}
