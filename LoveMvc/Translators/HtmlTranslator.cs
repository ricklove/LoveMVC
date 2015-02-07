using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoveMvc.Translators
{
    public class HtmlTranslator : ITemplateTranslator
    {
        public LoveTranslation TranslateTemplate(LoveTemplate template, object model)
        {
            var translation = new LoveTranslation();

            VisitNode(translation, template.Document, model);

            return translation;
        }

        private void VisitNode(LoveTranslation translation, LoveNode node, object model)
        {
            switch (node.Kind)
            {
                case LoveNodeKind.LoveBlock:
                    VisitBlock(translation, node as LoveBlock, model);
                    return;
                case LoveNodeKind.LoveModelStatement:
                    break;
                case LoveNodeKind.LoveName:
                    break;
                case LoveNodeKind.LoveMarkup:
                    break;
                case LoveNodeKind.LoveMarkupExpression:
                    break;
                case LoveNodeKind.LoveBinding:
                    break;
                case LoveNodeKind.LoveNotBinding:
                    break;
                case LoveNodeKind.LoveControlBlock:
                    break;
                case LoveNodeKind.LoveIfBlock:
                    break;
                case LoveNodeKind.LoveForeachBlock:
                    break;
                default:
                    break;
            }

            throw new NotImplementedException();
        }

        private void VisitBlock(LoveTranslation translation, LoveBlock loveBlock, object model)
        {
            throw new NotImplementedException();
        }
    }
}
