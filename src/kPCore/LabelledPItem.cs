using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpCore {
    public class LabelledPItem : PItem {

        protected List<string> labels;

        public virtual List<string> Labels {
            get {
                if (labels == null) {
                    labels = new List<string>();
                }

                return labels;
            }
        }

        public LabelledPItem()
            : base() {
        }

        public bool HasLabels() {
            return !(labels == null || labels.Count == 0);
        }

        public static void CopyLabels(LabelledPItem src, LabelledPItem dest) {
            if (src.labels != null) {
                dest.labels = new List<string>();
                foreach (string label in src.labels) {
                    dest.labels.Add(label);
                }
            }
        }
    }
}
