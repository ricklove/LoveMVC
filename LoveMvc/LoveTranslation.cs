using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LoveMvc
{
    public class LoveTranslation
    {
        public LoveTranslationPart MainPart { get; private set; }
        public Dictionary<string, LoveTranslationPart> Parts { get; private set; }
    }

    public class LoveTranslationPart
    {
        public string Name { get; set; }
        public string Content { get; set; }
    }
}
