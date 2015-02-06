using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoveMvc
{
    public interface ITemplateParser
    {
        LoveSyntaxTree Parse(TextReader source);
    }

    public interface IMarkupExpressionEvaluator
    {
        LoveBlock Evaluate<T>(LoveMarkupExpression expression, T model) where T : new();
    }

    public interface IViewViewModelPair
    {
        string Name { get; }
        object Model { get; }
        TextReader ViewSource { get; }
    }

    public class ViewViewModelPair : IViewViewModelPair
    {
        public object Model { get; private set; }
        public string Name { get; private set; }
        private Func<TextReader> _doGetViewSource;
        public TextReader ViewSource { get { return _doGetViewSource(); } }

        public ViewViewModelPair(object model, Func<TextReader> doGetViewSource, string name = "")
        {
            Model = model;
            _doGetViewSource = doGetViewSource;
            Name = name;
        }


    }
}
