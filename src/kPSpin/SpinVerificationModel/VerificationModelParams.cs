using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpSpin.SpinVerificationModel {
    public class VerificationModelParams {
        public bool LinksEnabled { get; set; }
        public int Steps { get; set; }
        public int MaxCompartments { get; set; }
        public int MaxLinks { get; set; }
        public bool O1 { get; set; }

        public static VerificationModelParams Default() {
            return new VerificationModelParams() {
                LinksEnabled = true,
                Steps = 20,
                MaxCompartments = -1,
                MaxLinks = -1,
                O1 = true
            };
        }
    }

}