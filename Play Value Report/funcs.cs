﻿using System;
using System.IO;
using System.Linq;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Data;
using System.Text.RegularExpressions;
using System.Text;
using System.Runtime.InteropServices;

namespace Play_Value_Report
{
    class funcs
    {
        // Write an entry to the log file
        public static void LogWrite(string logMessage, bool with_date = true)
        {
            string m_exePath = string.Empty;
            m_exePath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
            try
            {
                using (StreamWriter w = File.AppendText(m_exePath + "\\" + "report_error.log"))
                {
                    try
                    {
                        if (with_date) { w.Write("\n{0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString() + ":"); }
                        w.WriteLine("  {0}", logMessage);
                    }
                    catch (Exception ex)
                    {
                        string error = ex.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                string major_error = ex.ToString();
            }
        }

        // Find database.loc file and load the games list
        public static bool TestDatabaseConnection(string connnectionString)
        {
            try
            {
                if (connnectionString.Length > 0)
                {
                    using (SqlConnection ECS = new SqlConnection(connnectionString + " connection timeout=5"))
                    {
                        ECS.Open();
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex) { LogWrite(ex.Message); return false; }
        }

        // Create list of game categories
        public static void GetGameCategoryList(List<classy.GameCategories> catList)
        {

            SqlConnection ECS = new SqlConnection(globals.ConnectionString);
            try
            {
                ECS.Open();
                using (var SQL = new SqlCommand("select Game_Category, Category_Description from Game_Categories WITH (NOLOCK) where deleted = 0 order by Category_Description asc", ECS))
                {
                    using (var oReader = SQL.ExecuteReader())
                    {
                        if (oReader.HasRows)
                        {
                            

                            while (oReader.Read())
                            {
                                classy.GameCategories item = new classy.GameCategories();
                                item.categoryID = oReader.GetString(0);
                                item.description = oReader.GetString(1);
                                catList.Add(item);
                            }
                        }
                    }
                }

                ECS.Close();
            }
            catch (SqlException)
            {
                LogWrite("Error getting game categories list");
            }
        }

        // Execute stored procedure for report and load into data grid
        public static void RunReport(DataGridView dataGrid, DateTime dateFrom, DateTime dateTo, string gCats)
        {

            SqlConnection ECS = new SqlConnection(globals.ConnectionString);
            try
            {
                using (SqlConnection con = ECS)
                {
                    SqlCommand cmd = new SqlCommand("Custom_Promotional_Value", con);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@StartDate", dateFrom);
                    cmd.Parameters.AddWithValue("@EndDate", dateTo);
                    cmd.Parameters.AddWithValue("@CategoriesSelected", gCats);

                    con.Open();
                    cmd.ExecuteNonQuery();

                    using (SqlDataAdapter adap = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adap.Fill(dt);
                        dataGrid.DataSource = dt;
                    }
                }
            }
            catch (Exception)
            {
                LogWrite("Error running stored procedure");
            }

        }


        // Get list of selected categories and place into simple string
        public static void SimplifyGameCats(CheckedListBox listBox, List<classy.GameCategories> catList)
        {
            globals.GameCatString = String.Empty;

            foreach (var item in listBox.CheckedItems)
            {
                foreach (var cat in catList)
                {
                    if (item.ToString() == cat.description)
                    {
                        globals.GameCatString += cat.categoryID + ",";
                    }
                }
            }
            const string reduceMultiSpace = @"[ ]{2,}";
            globals.GameCatString = Regex.Replace(globals.GameCatString.Replace("\t", ""), reduceMultiSpace, "");
            globals.GameCatString = Regex.Replace(globals.GameCatString, @"\t|\n|\r", "");
            globals.GameCatString.Replace(" ", "").TrimEnd(',');
            globals.GameCatString = globals.GameCatString.Substring(0, globals.GameCatString.Length - 1);
        }

        // Save data grid to CSV
        public static void SaveDataGridViewToCSV(DataGridView dataGrid, string filename, string fromDate = "", string ToDate = "")
        {
            try
            {
                var sb = new StringBuilder();
                var headers = dataGrid.Columns.Cast<DataGridViewColumn>();
                if (fromDate.Length > 1 && ToDate.Length > 1)
                {
                    sb.AppendLine("From: " + "," + fromDate + "," + "," + "To Date: " + "," + ToDate);
                    sb.AppendLine("");
                }
                sb.AppendLine(string.Join(",", headers.Select(column => "\"" + column.HeaderText + "\"").ToArray()));

                foreach (DataGridViewRow row in dataGrid.Rows)
                {
                    var cells = row.Cells.Cast<DataGridViewCell>();
                    sb.AppendLine(string.Join(",", cells.Select(cell => "\"" + cell.Value + "\"").ToArray()));
                }
                File.WriteAllText(filename, sb.ToString(), System.Text.Encoding.UTF8);
                // Choose whether to write header. Use EnableWithoutHeaderText instead to omit header.
                //dataGrid.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
                // Select all the cells
                //dataGrid.SelectAll();
                // Copy selected cells to DataObject
                //DataObject dataObject = dataGrid.GetClipboardContent();
                // Get the text of the DataObject, and serialize it to a file
                //File.WriteAllText(filename, dataObject.GetText(TextDataFormat.CommaSeparatedValue));
            }
            catch (Exception) { LogWrite("Error exporting to CSV"); }
        }

        // Simply add a trailing backslash to a string - for use with folders and the auto update
        public static string AddTrailingBackslash(string path)
        {
            // Set the seperate char with \
            string separator1 = Path.DirectorySeparatorChar.ToString();
            string separator2 = Path.AltDirectorySeparatorChar.ToString();
            // Tidy up string
            path = path.Trim();
            // Return default string if it already ends with \
            if (path.EndsWith(separator1) || path.EndsWith(separator2))
                return path;
            // IF there is alternative seperator then add that (think linux)
            if (path.Contains(separator2))
                return path + separator2;
            // Add Windows seperater if nothing else to do
            return path + separator1;
        }
    }

    /// INI file read and write class
    public class iniFile
    {
        public string path;

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        // INIFile Constructor.
        public iniFile(string INIPath)
        {
            path = INIPath;
        }

        // Write Data to the INI File
        public void IniWriteValue(string Section, string Key, string Value)
        {
            WritePrivateProfileString(Section, Key, Value, this.path);
        }

        // Read Data Value From the Ini File
        public string IniReadValue(string Section, string Key)
        {
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(Section, Key, "", temp, 255, this.path);
            return temp.ToString();

        }
    }
}
