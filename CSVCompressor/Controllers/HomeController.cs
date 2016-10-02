using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace CSVCompressor.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            DataTable parsedCSV = LoadCsv(HttpContext.Server.MapPath("~/Fichier/CSVBrut/Export.csv"));
            DataTable mergedCSV = mergeCSV(parsedCSV);
            dataTableToCSV(mergedCSV);

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public static DataTable mergeCSV(DataTable parsedCSV)
        {
            int nbLigneToMerge = 2;
            int start = 2;

            bool firstLineForColumnName = true;

            DataTable mergedCSV = new DataTable();

            foreach (DataColumn col in parsedCSV.Columns)
            {
                mergedCSV.Columns.Add(col.ColumnName);
            }

            List<DataRow> rowsToMerge = new List<DataRow>();

            while (start < (parsedCSV.Rows.Count + nbLigneToMerge))
            {
                mergeRows(parsedCSV.AsEnumerable().Skip(start-2).Take(nbLigneToMerge).ToList(), ref mergedCSV, ref start);
                start += nbLigneToMerge;
            }

            return mergedCSV;
        }

        private void dataTableToCSV(DataTable dt)
        {
            StringBuilder sb = new StringBuilder();

            IEnumerable<string> columnNames = dt.Columns.Cast<DataColumn>().
                Select(column => column.ColumnName);
            sb.AppendLine(string.Join(";", columnNames));

            foreach (DataRow row in dt.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                sb.AppendLine(string.Join(";", fields));
            }

            System.IO.File.WriteAllText(HttpContext.Server.MapPath("~/Fichier/CSVModif/MergedExport.csv"), sb.ToString(), Encoding.UTF8);
        }

        private static void mergeRows(List<DataRow> rowsToMerge, ref DataTable finalDataTable, ref int counter)
        {
            DataRow mergedRows = finalDataTable.NewRow();
            string indi_code = rowsToMerge.First()[0].ToString();
            int toRemove = 0;

            foreach (DataRow row in rowsToMerge)
            {
                if (indi_code == row[0].ToString())
                {
                    for (int i = 0; i < row.Table.Columns.Count; i++)
                    {
                        if ((mergedRows[i] == DBNull.Value || mergedRows[i].ToString() == String.Empty) && (row[i] != DBNull.Value || row[i].ToString() != String.Empty))
                        {
                            mergedRows[i] = row[i];
                        }
                    }
                }
                else
                {
                    toRemove++;
                }
            }
            counter -= toRemove;
            toRemove = 0;
            finalDataTable.Rows.Add(mergedRows);
        }

        private DataTable LoadCsv(string filename)
        {
            // Get the file's text.
            string whole_file = System.IO.File.ReadAllText(filename,Encoding.UTF8);

            DataTable parsedCSV = new DataTable();
            DataRow rowToAdd;

            // Split into lines.

            string[] lines = whole_file.Split(new char[] {'\r'},StringSplitOptions.RemoveEmptyEntries);

            // See how many rows and columns there are.
            int num_rows = lines.Length -1;
            int num_cols = lines[0].Split(';').Length;
            string[] first_line = lines[0].Split(';');

            for (int i = 0; i < num_cols; i++)
            {
                parsedCSV.Columns.Add(first_line[i]);
            }

            rowToAdd = parsedCSV.NewRow();


            // Load the array.
            for (int r = 1; r < num_rows; r++)
            {
                string[] line_r = lines[r].Split(';');
                for (int c = 0; c < num_cols; c++)
                {
                    rowToAdd[c] = line_r[c].Replace('\n','\0');
                }
                parsedCSV.Rows.Add(rowToAdd);
                rowToAdd = parsedCSV.NewRow();
            }

            // Return the values.
            return parsedCSV;
        }
    }
}