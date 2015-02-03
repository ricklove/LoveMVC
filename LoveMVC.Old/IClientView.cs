using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace LoveMVC
{
    interface IClientView
    {
        void EndView();
        void BeginView();
    }

    class NoScriptClientView : IClientView
    {
        public NoScriptClientView()
        {
        }

        public void BeginView()
        {
        }

        public void EndView()
        {
        }
    }
}
