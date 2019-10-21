using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Indentation;
using ICSharpCode.AvalonEdit.Search;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace KpUi.Editor
{
    [Designer("System.Windows.Forms.Design.ControlDesigner, System.Design")]
    [DesignerSerializer("System.ComponentModel.Design.Serialization.TypeCodeDomSerializer , System.Design", "System.ComponentModel.Design.Serialization.CodeDomSerializer, System.Design")]
    public class KpQueriesEditor : BaseKpEditor
    {
        protected override void Init()
        {
            base.Init();
            
            using (var stream = new MemoryStream(kPUI.Properties.Resources.KpQueriesSyntaxHighlighting))
            {
                using (XmlTextReader syntaxHiReader = new XmlTextReader(stream))
                {
                    editor.SyntaxHighlighting = HighlightingLoader.Load(syntaxHiReader, HighlightingManager.Instance);
                }
            }
        }

        public override void Save()
        {
            if (!HasAssociatedFile)
            {
                var opd = new SaveFileDialog();
                opd.Filter = "kP Queries file (*.kpq)|*.kpq";

                if (opd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    FileName = opd.FileName;
                }
            }

            if (HasAssociatedFile)
            {
                Editor.Save(FileName);
            }
        }
    }
}
