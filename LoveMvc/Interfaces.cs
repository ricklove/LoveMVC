using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoveMvc
{
    public static class Providers
    {
        public static IViewRenderer ViewRenderer { get; set; }
    }

    public interface ITemplateParser
    {
        LoveSyntaxTree Parse(TextReader source);
    }

    public interface IMarkupExpressionEvaluator
    {
        LoveBlock Evaluate<T>(LoveMarkupExpression expression, T model);
    }

    public interface IViewRenderer
    {
        string RenderView<T>(IViewViewModelPair<T> viewViewModelPair);
    }

    public interface IViewViewModelPair
    {
        string Name { get; }
        TextReader ViewSource { get; }
        object ModelUntyped { get; }
    }

    public interface IViewViewModelPair<T> : IViewViewModelPair
    {
        string Name { get; }
        T Model { get; }
        TextReader ViewSource { get; }
    }

    public class ViewViewModelPair<T> : IViewViewModelPair<T>
    {
        public T Model { get; private set; }
        public string Name { get; private set; }
        private Func<TextReader> _doGetViewSource;
        public TextReader ViewSource { get { return _doGetViewSource(); } }

        public ViewViewModelPair(T model, Func<TextReader> doGetViewSource, string name = "")
        {
            Model = model;
            _doGetViewSource = doGetViewSource;
            Name = name;
        }

        public object ModelUntyped
        {
            get { return Model; }
        }
    }
}
