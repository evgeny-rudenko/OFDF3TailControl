using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.IO;
using ClosedXML;
using ClosedXML.Excel;
using System.Data;
using System.ComponentModel;
using System.Data.SqlClient;

namespace OFDF3TailControl
{
    /// <summary>
    /// Класс расчитывает диапазон дат , когда были пробиты чеки
    /// </summary>
   partial class Program
    {
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return  Base64Encode( sb.ToString());
            }
        }

        /// <summary>
        /// Получаем таблицу по запросу или имени 
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static DataTable fillDataTable(string table, string connectionString, string Databse = "eplus_work", SqlCommand command= null)
        {
            string query = table;
            if (command != null)
            {

            }

            //костыль - так лучше не делать
            if (table.ToUpper().Contains("SELECT") == true)
            {
                query = table;
            }
            else
            {
                query = "SELECT * FROM " + Databse + ".dbo." + table;
            }



            String conSTR = connectionString;
            SqlConnection sqlConn = new SqlConnection(conSTR);

            sqlConn.Open();
            SqlCommand cmd = new SqlCommand(query, sqlConn);
            cmd.CommandTimeout = 0;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            sqlConn.Close();
            return dt;
        }

        /// <summary>
        ///Трансформируем список в datatable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>( IList<T> data)
        {
            PropertyDescriptorCollection props =
                TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                table.Columns.Add(prop.Name, prop.PropertyType);
            }
            object[] values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }
            return table;
        }

        class Cheque
        {
            public DateTime cheque_date { get; set; }
            public decimal summ_cheque { get; set; }
            public string kassa { get; set; }
            public string fp { get; set; }
            public int cheque_number { get; set; }
            public string hash { get; set; }
        }
        
       class DTMAXMIN 
            {
            
            private DateTime maxdate;
            private DateTime mindate;
            public DTMAXMIN ()
            {
                maxdate = DateTime.Now.AddDays(-1000);
                mindate = DateTime.Now.AddDays(1000);
            }

            public void CompareDate (DateTime DateForCompare)
            {
                if (DateForCompare > maxdate)
                    maxdate = DateForCompare;

                if (DateForCompare < mindate)
                    mindate = DateForCompare;
            }

            public DateTime GetMaxDate ()
            {
                return maxdate;
            }

            public DateTime GetMinDate ()
            {

                return mindate;
            }
        }



        static void Main(string[] args)
        {
                
            string filename = "ofd_test.xlsx";
            if (!File.Exists(filename))
            {
                Console.WriteLine("Нет файла");
                Console.WriteLine(filename);
                Console.ReadKey();


            }

            #region Поля  xlsx  файла выгрузки ОФД
            /*
            Дата/время	A
            Магазин	B
            Имя кассы	C
            Дополнительный идентификатор	D
            РНМ	E
            Номер ФН	F
            ФП	G
            Номер документа	H
            Номер смены	I
            Номер чека за смену	J
            Без НДС	K
            НДС 0%	L
            НДС 10%	M
            НДС 20%	N
            НДС 20/120%	O
            НДС 10/110%	P
            Признак расчета	Q
            Наличными	R
            Электронными	S
            Предоплата (аванс)	T
            Зачет предоплаты (аванса)	U
            Постоплата (кредитами)	V
            Встречными предоставлениями	W
            Итого	X
            Тип налогообложения	Y
            Посмотреть чек	Z


            */
            #endregion

            DTMAXMIN vtime = new DTMAXMIN();

            List<Cheque> CL = new List<Cheque>();
            Console.WriteLine("Пробую прочитать файл выгрузки ОФД");
            string fileName = filename;
            using (var excelWorkbook = new XLWorkbook(fileName))
            {
                var nonEmptyDataRows = excelWorkbook.Worksheet(1).RowsUsed();
               
                foreach (var dataRow in nonEmptyDataRows)
                {
                    Cheque c = new Cheque();
                    if (dataRow.RowNumber() >= 2 )

                    {
                        try
                        {
                            c.cheque_date = DateTime.Parse(dataRow.Cell(1).Value.ToString());
                            c.cheque_number = int.Parse(dataRow.Cell(8).Value.ToString());
                            c.summ_cheque = decimal.Parse(dataRow.Cell(24).Value.ToString());
                            c.fp = dataRow.Cell(7).Value.ToString();
                            c.kassa = dataRow.Cell(3).Value.ToString();
                            c.hash = c.cheque_number.ToString().Trim()+"|"+c.fp.ToString().Trim();

                            CL.Add(c);

                            vtime.CompareDate(c.cheque_date);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Произошла ошибка");
                            Console.WriteLine(e);
                        }
                    }
                }
            }
            Console.WriteLine("Всего строк обработано ");
            Console.WriteLine(CL.Count);

            DataTable ofddt = ToDataTable(CL);
            Console.WriteLine(vtime.GetMaxDate());
            Console.WriteLine(vtime.GetMinDate());
            ofddt.TableName = "ofd";
            ofddt.WriteXml("ofdcheque.xml");
            
            

            DataTable efdt = new DataTable();
            if (Properties.Settings.Default.LoadEfChequeFromXML == true)
            {
                efdt.ReadXml("efcheque.xml");
            }
            else
            {
                efdt = getEfCheque(vtime.GetMinDate(), vtime.GetMaxDate());
                efdt.TableName = "efcheque";
                efdt.WriteXml("efcheque.XML");

            }
            CompareCheque.CompareMe(ofddt,efdt);
            Console.ReadKey();

        }

        /// <summary>
        /// Получаем список чеков из F3Tail
        /// </summary>
        /// <param name="mindate">Начальный период выборки чеков</param>
        /// <param name="maxdate">Конечный период</param>
        /// <returns>Возвращаем Datatable с чеками</returns>
        public static DataTable getEfCheque(DateTime mindate, DateTime maxdate)
        {
                        
            String conSTR = Properties.Settings.Default.ConnectionString;
            SqlConnection sqlConn = new SqlConnection(conSTR);
            string query = File.ReadAllText("CHEQUE.SQL");
            sqlConn.Open();
            SqlCommand cmd = new SqlCommand(query, sqlConn);
            cmd.CommandTimeout = 0;
            cmd.Parameters.Add("@MINDATE", SqlDbType.DateTime);
            cmd.Parameters["@MINDATE"].Value = mindate;
            cmd.Parameters.Add("@MAXDATE", SqlDbType.DateTime);
            cmd.Parameters["@MAXDATE"].Value = maxdate;

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            sqlConn.Close();
            return dt;
            //throw new NotImplementedException();
        }
    }
}
