using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace OFDF3TailControl
{
  static class CompareCheque
    {
    
        /// <summary>
        /// Сравниваем соедржимое таблиц с чеками
        /// </summary>
        /// <param name="dtofd">Таблица ОФД  </param>
        /// <param name="dtef">Таблица ефарма</param>
    public static void CompareMe (DataTable dtofd, DataTable dtef)
    {
            List<string> ofdhashes = new List<string>();
            List<string> efhashes = new List<string>();
            List<string> notinofd = new List<string>();
            foreach (DataRow r in dtofd.AsEnumerable())
            {
                ofdhashes.Add( r["hash"].ToString() );
            }

            foreach (DataRow r in dtef.AsEnumerable())
            {
                efhashes.Add(r["hash"].ToString());
            }

            StringBuilder sb = new StringBuilder();
            
            foreach (string s in efhashes )
            {
                if (!ofdhashes.Contains(s))
                {
                    Console.WriteLine(s);
                    sb.AppendLine(s);
                }
            }

            Console.WriteLine();
            Console.WriteLine("Записей чеков ЕФ");
            Console.WriteLine(efhashes.Count);
            Console.WriteLine();
            Console.WriteLine("Записей чеков ОФД");
            Console.WriteLine(ofdhashes.Count);
            Console.WriteLine();

            File.WriteAllText("result.txt",sb.ToString());

        }
    
        
        
    /// <summary>
    /// 
    /// </summary>
    public static void WriteResultXLS ()
    {
        throw new NotImplementedException();
    }
    
    }
}
