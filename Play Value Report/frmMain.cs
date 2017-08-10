using System;
using System.Windows.Forms;
using System.Drawing;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.IO;

namespace Play_Value_Report
{
    public partial class frmMain : Form
    {
        static List<classy.GameCategories> gameCats = new List<classy.GameCategories>();

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            // Set default values
            clbGroups.Items.Clear();

            // Attempt to connect to database
            try
            {
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "database.loc"))
                {
                    string firstLine = File.ReadLines(AppDomain.CurrentDomain.BaseDirectory + "database.loc").First();
                    int first_semi = firstLine.IndexOf(";");
                    int second_semi = firstLine.IndexOf("=", first_semi);
                    string dataSource = firstLine.Substring(firstLine.IndexOf("=") + 1, first_semi - (firstLine.IndexOf("=") + 1));
                    string initialCatalog = firstLine.Substring(second_semi + 1, firstLine.Length - second_semi - 1);
                    initialCatalog.Replace(";", "");
                    globals.ConnectionString = "server=" + dataSource + ";Trusted_Connection=yes;database=" + initialCatalog + ";";
                }
                else
                {
                    funcs.LogWrite("Database.loc not found!");
                    MessageBox.Show("Database.loc not found! Application will now exit.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    Application.Exit();
                }
            }
            catch (Exception ex) { funcs.LogWrite(ex.Message); }

            if (!funcs.TestDatabaseConnection(globals.ConnectionString))
            {
                funcs.LogWrite("DATABASE CONNECTION ERROR");
                MessageBox.Show("Unable to connect to the database specified. Application will now close.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Application.Exit();
            }

            // Set new timeout for database connections
            globals.ConnectionString += "  connection timeout=15";

        }

        private void btnClose_Paint(object sender, PaintEventArgs e)
        {
            Bitmap bmp = (Bitmap)this.btnClose.Image;
            bmp.MakeTransparent(Color.White);
            btnClose.Image = bmp;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnRun_Paint(object sender, PaintEventArgs e)
        {
            Bitmap bmp = (Bitmap)this.btnRun.Image;
            bmp.MakeTransparent(Color.White);
            btnRun.Image = bmp;
        }

        private void frmMain_Shown(object sender, EventArgs e)
        {
            // Sort out the look of application
            btnClose.Update();
            btnClose.Refresh();
            btnRun.Update();
            btnRun.Refresh();
            btnExport.Update();
            btnExport.Refresh();
            btnExport.Enabled = false;
            btnRun.Focus();
            dtpFrom.Value = DateTime.Now.AddDays(-1).Date;
            dtpTo.Value = DateTime.Now.Date.AddDays(1).AddTicks(-1);
            funcs.GetGameCategoryList(gameCats);
            foreach (var item in gameCats)
            {
                clbGroups.Items.Add(item.description, true);
            }

            }

        private void btnRun_Click(object sender, EventArgs e)
        {
            btnExport.Enabled = true;
            funcs.SimplifyGameCats(clbGroups, gameCats);
            funcs.RunReport(grdResults, dtpFrom.Value, dtpTo.Value, globals.GameCatString);
        }

        private void btnExport_Paint(object sender, PaintEventArgs e)
        {
            Bitmap bmp = (Bitmap)this.btnExport.Image;
            bmp.MakeTransparent(Color.White);
            btnExport.Image = bmp;
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            globals.ExportPath = String.Empty;
            globals.ExportDir = String.Empty;

            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.InitialDirectory = @"C:\";
            saveDialog.DefaultExt = "csv";
            saveDialog.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
            saveDialog.FilterIndex = 1;
            saveDialog.Title = "Export Report to CSV";
            saveDialog.CheckFileExists = false;
            saveDialog.CheckPathExists = true;
            saveDialog.RestoreDirectory = true;
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                globals.ExportPath = saveDialog.FileName;
                globals.ExportDir = Path.GetDirectoryName(globals.ExportPath);
            }

            if (Directory.Exists(globals.ExportDir) && globals.ExportPath != String.Empty)
            {
                funcs.SaveDataGridViewToCSV(grdResults, globals.ExportPath);
                MessageBox.Show("Export completed");
            }
            else
            {
                MessageBox.Show("Unable to save to CSV", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }


        }
    }
}
