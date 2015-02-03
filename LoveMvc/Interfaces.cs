﻿using System;
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
}
