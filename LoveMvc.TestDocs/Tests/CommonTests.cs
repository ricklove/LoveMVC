using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoveMvc.TestDocs.Tests
{
    public class CommonTests
    {
        public static IEnumerable<Exception> RunTests(LoveTemplate template, object model)
        {
            var tests = new List<Func<LoveTemplate, object, IEnumerable<Exception>>>() { 
                Test_DoesNotContainMarkupExpressions,
                Test_AllBindingsMapToViewModel
            };

            foreach (var test in tests)
            {
                foreach (var ex in test(template, model))
                {
                    yield return ex;
                }
            }

        }

        public static IEnumerable<Exception> Test_DoesNotContainMarkupExpressions(LoveTemplate template, object model)
        {
            foreach (var n in template.Flatten())
            {
                if (n is LoveMarkupExpression)
                {
                    yield return new LoveTestFail(n, "is Markup Expression");
                }
            }
        }

        public static IEnumerable<Exception> Test_AllBindingsMapToViewModel(LoveTemplate template, object model)
        {
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

        //public static IEnumerable<Exception> Test_DoesNotContainViewModelReferencesInMarkup(LoveTemplate template)
        //{
        //    foreach (var n in template.Flatten())
        //    {
        //        try
        //        {
        //            if (n is LoveMarkupExpression)
        //            {
        //                throw new LoveTestFail(n, "Is Markup Expression");
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            yield return ex;
        //        }
        //    }
        //}
    }
}
