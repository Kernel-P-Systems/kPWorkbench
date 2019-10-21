using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Indentation;
using ICSharpCode.AvalonEdit.Search;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using System.Xml;

namespace KpUi.Editor
{
    public abstract class BaseKpEditor : System.Windows.Forms.Integration.ElementHost
    {
        protected TextEditor editor;

        public TextEditor Editor
        {
            get { return editor; }
        }

        public string FileName { get; set; }

        public bool HasAssociatedFile { get { return !string.IsNullOrEmpty(FileName); } }

        protected virtual void Init()
        {
            editor = new TextEditor();
            SearchPanel.Install(editor);
            FoldingManager.Install(editor.TextArea);
            editor.TextArea.IndentationStrategy = new DefaultIndentationStrategy();
            editor.ShowLineNumbers = true;
            editor.FontFamily = new FontFamily("Consolas");
            editor.FontSize = 12;

            KeyDown += (sender, e) =>
            {
                switch (e.KeyData)
                {
                    case Keys.Control | Keys.S:
                        Save();
                        e.SuppressKeyPress = true;
                        break;
                }
            };

            Dock = DockStyle.Fill;
            Child = editor;
        }

        public abstract void Save();

        public BaseKpEditor()
        {
            Init();
        }
    }
}
