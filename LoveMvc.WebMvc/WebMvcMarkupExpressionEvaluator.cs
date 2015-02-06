using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using LoveMvc;

namespace LoveMvc.WebMvc
{
    public class WebMvcMarkupExpressionEvaluator : IMarkupExpressionEvaluator
    {
        public static void Initialize()
        {
            LoveVirtualViewProvider.Initialize();
        }

        public ControllerContext _controllerContext { get; private set; }
        public WebMvcMarkupExpressionEvaluator(ControllerContext controllerContext)
        {
            _controllerContext = controllerContext;
        }

        public LoveBlock Evaluate<T>(LoveMarkupExpression expression, T model) where T : new()
        {
            return Evaluate(expression, model, false);
        }

        public LoveBlock Evaluate<T>(LoveMarkupExpression expression, T model, bool isSecondTry) where T : new()
        {
            if (expression.Content.Trim().StartsWith("Html."))
            {
                var simpleExpression = HtmlHelperBindingMapper.GetSimpleRazorExpression(expression);
                var scopes = expression.GetScopes();
                var results = LoveVirtualViewProvider.Instance.GetExpressionEvaluation(model, expression.Content, simpleExpression, scopes);

                var normal = results.NormalResults;
                var simple = results.SimpleResults;

                // WARNING: This is buggy!
                // Ensure model has data at relevant property
                // TODO: Create own object and decorate with DataAnotations using provider:
                // http://stackoverflow.com/questions/11964956/how-can-i-register-a-custom-modelmetadataprovider-with-simple-injector-in-mvc3
                // http://haacked.com/archive/2011/07/14/model-metadata-and-validation-localization-using-conventions.aspx/
                if (!isSecondTry && string.IsNullOrWhiteSpace(simple))
                {
                    try
                    {
                        var nModel = CreateModelWithSpecificValue<T>(model, simpleExpression.ReplaceStart("Model", "this"));
                        return Evaluate(expression, nModel, true);
                    }
                    catch
                    {
                        // Hey at least we tried
                        var breakdance = false;
                    }
                }

                // Parse the markup
                var mappedBlock = HtmlHelperBindingMapper.MapBinding(expression, normal, simple, simpleExpression);

                return mappedBlock;
            }

            throw new InvalidOperationException();
        }

        private static T CreateModelWithSpecificValue<T>(T model, string simpleExpression) where T : new()
        {
            var mCtor = model.GetType().GetConstructor(System.Type.EmptyTypes);

            if (mCtor != null)
            {
                model = (T)mCtor.Invoke(null);
            }

            ReflectionHelper.SetValue(model, simpleExpression, (Type propType) =>
            {
                switch (Type.GetTypeCode(propType))
                {
                    // Bool
                    case TypeCode.Boolean:
                        return true;

                    // Integer Types
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.SByte:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                    case TypeCode.Char:
                        return 42;

                    // Float Types
                    case TypeCode.Decimal:
                    case TypeCode.Double:
                    case TypeCode.Single:
                        return 42.42;

                    // Date
                    case TypeCode.DateTime:
                        return new DateTime(2011, 11, 11);

                    // String
                    case TypeCode.String:
                        return "SsPpEeCcIiFfIiCc";

                    // Null types
                    case TypeCode.DBNull:
                        return DBNull.Value;
                    case TypeCode.Empty:
                        return null;

                    // Other
                    case TypeCode.Object:
                    default:
                        break;
                }


                var ctor = propType.GetConstructor(System.Type.EmptyTypes);

                if (ctor != null)
                {
                    return ctor.Invoke(null);
                }

                return null;
            });

            return model;
        }


    }
}
