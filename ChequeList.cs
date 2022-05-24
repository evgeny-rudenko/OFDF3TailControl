using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace OFDF3TailControl
{
    static class ChequeList
    {
        class cheque
        {
            DateTime DateCheque;
            // фискальный признак
            String FP;
            Decimal SumCheque;


        }

        class OFDCeque : cheque
        {

        }

        class f3tailcheque : cheque
        {
               
        
        }
        
        public static void PlatformaOFDSource (string file)
        {

        }
    }
}
