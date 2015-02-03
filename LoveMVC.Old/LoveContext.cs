using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace LoveMVC
{
    public class LoveContext
    {
        public static LoveContext<T> CreateLoveContext<T>(HtmlHelper<T> html)
        {
            return LoveContext<T>.CreateLoveContext(html);
        }
    }

    public class LoveContext<T>
    {
        public static LoveContext<T> CreateLoveContext<T>(HtmlHelper<T> html)
        {
            // Determine client type
            return new LoveContext<T>(ClientType.NoScript, html);
        }

        private IClientView _clientHandler;
        private HtmlHelper<T> _htmlInner;

        private LoveContext(ClientType clientType, HtmlHelper<T> html)
        {
            _htmlInner = html;

            switch (clientType)
            {
                case ClientType.Smart:
                    throw new NotImplementedException();
                    break;
                case ClientType.NoScript:
                    _clientHandler = new NoScriptClientView();
                    break;
                default:
                    break;
            }
        }

        public HtmlHelper<T> Html { get { throw new NotImplementedException(); } }

        public IDisposable lif(Expression<Func<T, bool>> expression)
        {
            return new AutoEndBlock(() => { }, () => { });
        }

        public DisposableItemHolder<TItem> lforeach<TItem>(Expression<Func<T, IEnumerable<TItem>>> expression) where TItem : class
        {
            var autoBlock = new AutoEndBlock(() => { }, () => { });

            return new DisposableItemHolder<TItem>() { Disposable = autoBlock, Item = null };
        }

        public class DisposableItemHolder<TItem> : IDisposable
        {
            public IDisposable Disposable { get; set; }
            public TItem Item { get; set; }

            void IDisposable.Dispose()
            {
                if (Disposable != null)
                {
                    Disposable.Dispose();
                }
            }
        }

        #region Begin End View

        private AutoEndBlock _viewBlock;

        public IDisposable BeginView()
        {
            if (_viewBlock != null)
            {
                throw new InvalidOperationException();
            }

            _viewBlock = new AutoEndBlock(() => _clientHandler.BeginView(), () => _clientHandler.EndView());
            return _viewBlock;
        }

        public void EndView()
        {
            _viewBlock.End();
        }

        #endregion
    }



    public enum ClientType
    {
        Smart,
        NoScript
    }

    public class AutoEndBlock : IDisposable
    {
        private bool _hasEnded = false;
        private Action _doEnd;

        public AutoEndBlock(Action doBegin, Action doEnd)
        {
            doBegin();
            _doEnd = doEnd;
        }

        void IDisposable.Dispose()
        {
            if (!_hasEnded)
            {
                End();
            }
        }

        public void End()
        {
            if (_hasEnded) { throw new InvalidOperationException(); }
            _hasEnded = true;

            _doEnd();
        }
    }
}
