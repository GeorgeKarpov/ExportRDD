using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using AcadWindows = Autodesk.AutoCAD.Windows;
//using ExportPt1;

namespace ExpRddApp
{
    public partial class FrmStation : Form
    {
        private string stationId;
        private string stationName;
        private List<elements.RailwayLine> lines;
        private string zeroLevelLine;
        private string version;
        private string orderRddFileName;
        private string docId;
        Dictionary<string, Control> labelsControls = new Dictionary<string, Control>();
        Dictionary<string, CheckBox> checkBoxControls = new Dictionary<string, CheckBox>();
        private Dictionary<string, string> loadFiles;
        AcadWindows.OpenFileDialog OpenFile;
        public bool CopyLevel { set; get; }
        public bool AutoAC { set; get; }
        public bool OrderRdd { set; get; }
        public string DwgDir { get; set; }
        public string RddSaveTo { get; set; }

        private List<string> authors;
        private string author = "";

        public string GetAuthor()
        {
            return this.author;
        }

        public void SetAuthors(List<string> value)
        {
            this.authors = value;
            if (authors != null)
            {
                cmbAuthor.DataSource = authors;
            }
        }

        public string StationId
        {
            get
            {
                return stationId;
            }
            set
            {
                stationId = value;
                txtBoxStationId.Text = stationId;
            }
        }

        public string StationName
        {
            get
            {
                return stationName;
            }
            set
            {
                stationName = value;
                txtBoxStationName.Text = stationName;
            }
        }

        public Dictionary<string, string> LoadFiles
        {
            get
            {
                return loadFiles;
            }
            set
            {
                loadFiles = value;
                foreach (KeyValuePair<string, string> entry in loadFiles
                                                               .Where(x => x.Key.Contains("xls") &&
                                                                            !string.IsNullOrEmpty(x.Value)))
                {
                    labelsControls[entry.Key].Text = Path.GetFileName(entry.Value);
                }
                if (loadFiles.ContainsKey("VersId"))
                {
                    txtBoxVersion.Text = loadFiles["VersId"];
                }
                if (loadFiles.ContainsKey("DocId"))
                {
                    txtBoxDocId.Text = loadFiles["DocId"];
                }
                // checkBox
                foreach (KeyValuePair<string, string> entry in loadFiles
                                                               .Where(x => x.Key.Contains("checkBox")))
                {
                    if (bool.TryParse(entry.Value.ToLower(), out bool test))
                    {
                        checkBoxControls[entry.Key].Checked = test;
                    }
                    else
                    {
                        checkBoxControls[entry.Key].Checked = false;
                    }
                }
            }
        }

        public List<elements.RailwayLine> Lines
        {
            get
            {
                return lines;
            }
            set
            {
                lines = value;
                foreach (elements.RailwayLine line in lines)
                {
                    int row = dgwLines.Rows.Add();
                    dgwLines.Rows[row].Cells["Line"].Value = line.Designation;
                    dgwLines.Rows[row].Cells["From"].Value = line.start;
                    dgwLines.Rows[row].Cells["To"].Value = line.end;
                    dgwLines.Rows[row].Cells["Direction"].Value = line.direction;
                    dgwLines.Rows[row].Cells["Color"].Style.BackColor = line.color;
                }
                if (lines.Count > 0)
                {
                    cmbLines.Items.AddRange(lines.Select(x => x.Designation).ToArray());
                    cmbLines.SelectedIndex = 0;
                }
            }
        }


        public Dictionary<string, bool> CheckData { get; set; } = new Dictionary<string, bool>();
        public string ZeroLevelLine
        {
            get
            {
                return zeroLevelLine;
            }
            set
            {
                zeroLevelLine = value;
            }
        }

        public string GetVersion()
        {
            return version;
        }

        public void SetVersion(string value)
        {
            version = value;
            txtBoxVersion.Text = version;
        }

        public string GetOrderRddFileName()
        {
            return orderRddFileName;
        }

        public string GetDocId()
        {
            return docId;
        }

        public void SetDocId(string value)
        {
            docId = value;
            txtBoxDocId.Text = docId;
        }

        public FrmStation()
        {
            InitializeComponent();
            loadFiles = new Dictionary<string, string>();
            lines = new List<elements.RailwayLine>();
            foreach (Control x in this.Controls)
            {
                if (x is GroupBox)
                {
                    foreach (Control l in x.Controls)
                    {
                        if (l is GroupBox)
                        {
                            foreach (Control ll in l.Controls)
                            {
                                if (ll is Label)
                                {
                                    labelsControls.Add(ll.Name, ll);
                                    if (ll.Name.Contains("xls"))
                                    {
                                        loadFiles.Add(ll.Name, null);
                                    }
                                }
                                if (ll is CheckBox)
                                {
                                    checkBoxControls.Add(ll.Name, (CheckBox)ll);
                                }
                            }
                        }
                        if (l is Label)
                        {
                            labelsControls.Add(l.Name, l);
                            if (l.Name.Contains("xls"))
                            {
                                loadFiles.Add(l.Name, null);
                            }
                        }
                        if (l is CheckBox)
                        {
                            checkBoxControls.Add(l.Name, (CheckBox)l);
                        }
                    }
                }
            }
            loadFiles.Add("VersId", null);
            loadFiles.Add("DocId", null);
            TxtBoxVersion_TextChanged(txtBoxVersion, new EventArgs());
        }

        private void FrmStation_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult != DialogResult.OK)
            {
                ErrLogger.Information("Program canceled by user", "Station Dialog");
                return;
            }

            AcadWindows.SaveFileDialog saveFile =
                new AcadWindows.SaveFileDialog("Save RDD", DwgDir + "\\" + Constants.defaultFileName, "xml", "SaveRdd",
                                                AcadWindows.SaveFileDialog.SaveFileDialogFlags.NoUrls);

            if (saveFile.ShowDialog() != DialogResult.OK)
            {
                ErrLogger.Information("Program canceled by user", "Station Dialog");
                e.Cancel = true;
                return;
            }
            RddSaveTo = saveFile.Filename;

            foreach (KeyValuePair<string, CheckBox> box in checkBoxControls)
            {
                loadFiles[box.Key] = box.Value.Checked.ToString();
            }


            if (InputCheckFailed())
            {
                e.Cancel = true;
                return;
            }

            StationId = txtBoxStationId.Text;
            orderRddFileName = loadFiles[nameof(lblxlsOrderRdd)];
            foreach (Control x in this.Controls)
            {
                if (x is GroupBox)
                {
                    foreach (Control l in x.Controls)
                    {
                        if (l is GroupBox)
                        {
                            foreach (Control ll in l.Controls)
                            {
                                if (ll is CheckBox)
                                {
                                    CheckData.Add(ll.Name, ((CheckBox)ll).Checked);
                                }
                            }
                        }
                        else if (l is CheckBox)
                        {
                            CheckData.Add(l.Name, ((CheckBox)l).Checked);
                        }
                    }
                }
            }
        }

        private bool InputCheckFailed()
        {
            if (txtBoxStationId.Text.Length == 0 ||
                txtBoxStationName.Text.Length == 0)
            {
                Autodesk.AutoCAD.ApplicationServices.Application
                    .ShowAlertDialog("Input data missing");
                return true;
            }
            foreach (KeyValuePair<string, string> entry in this.loadFiles)
            {
                if (!labelsControls.ContainsKey(entry.Key))
                {
                    continue;
                }

                if (string.IsNullOrEmpty(entry.Value) || !File.Exists(entry.Value))
                {
                    Label checkLabel = (Label)labelsControls[entry.Key];
                    if (checkLabel.Visible && checkLabel.Enabled)
                    {
                        Autodesk.AutoCAD.ApplicationServices.Application
                            .ShowAlertDialog("File name for '" + checkLabel.Tag.ToString() + "' is empty or not found.");
                        return true;
                    }
                }
            }
            return false;
        }

        private void BtnDetLock_Click(object sender, EventArgs e)
        {
            string sTypes = "xlsx; xlsm";
            OpenFile = new AcadWindows
                        .OpenFileDialog("Detection Locking", "", sTypes, "OpenFile",
                         AcadWindows.OpenFileDialog.OpenFileDialogFlags.SearchPath);
            if (OpenFile.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                //loadFiles[nameof(lblxlsDetLock)] = null;
                //lblxlsDetLock.Text = "Detection Locking";
                return;
            }
            lblxlsDetLock.Text = Path.GetFileName(OpenFile.Filename);
            loadFiles[nameof(lblxlsDetLock)] = OpenFile.Filename;
        }

        private void BtnFP_Click(object sender, EventArgs e)
        {
            string sTypes = "xlsm; xlsx";
            OpenFile = new AcadWindows
                        .OpenFileDialog("Flank Protection", "", sTypes, "OpenFile",
                         AcadWindows.OpenFileDialog.OpenFileDialogFlags.SearchPath);
            if (OpenFile.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                //loadFiles[nameof(lblxlsFP)] = null;
                //lblxlsFP.Text = "Flank Protection";
                return;
            }
            lblxlsFP.Text = Path.GetFileName(OpenFile.Filename);
            loadFiles[nameof(lblxlsFP)] = OpenFile.Filename;
        }

        private void BtnEmSg_Click(object sender, EventArgs e)
        {
            string sTypes = "xlsm; xlsx";
            OpenFile = new AcadWindows
                        .OpenFileDialog("Emergency Stops", "", sTypes, "OpenFile",
                         AcadWindows.OpenFileDialog.OpenFileDialogFlags.SearchPath);
            if (OpenFile.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                //loadFiles[nameof(lblxlsEmSg)] = null;
                //lblxlsEmSg.Text = "Emergency Stops";
                return;
            }
            lblxlsEmSg.Text = Path.GetFileName(OpenFile.Filename);
            loadFiles[nameof(lblxlsEmSg)] = OpenFile.Filename;
        }

        private void BtnSpProf_Click(object sender, EventArgs e)
        {
            string sTypes = "xlsm; xlsx";
            OpenFile = new AcadWindows
                        .OpenFileDialog("Speed Profiles", "", sTypes, "OpenFile",
                         AcadWindows.OpenFileDialog.OpenFileDialogFlags.SearchPath);
            if (OpenFile.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                //loadFiles[nameof(lblxlsSpProf)] = null;
                //lblxlsSpProf.Text = "Speed Profiles";
                return;
            }
            lblxlsSpProf.Text = Path.GetFileName(OpenFile.Filename);
            loadFiles[nameof(lblxlsSpProf)] = OpenFile.Filename;
        }

        private void BtnRoutes_Click(object sender, EventArgs e)
        {
            string sTypes = "xlsx; xlsm";
            OpenFile = new AcadWindows
                        .OpenFileDialog("Routes", "", sTypes, "OpenFile",
                         AcadWindows.OpenFileDialog.OpenFileDialogFlags.SearchPath);
            if (OpenFile.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                //loadFiles[nameof(lblxlsRoutes)] = null;
                //lblxlsRoutes.Text = "Routes";
                return;
            }
            lblxlsRoutes.Text = Path.GetFileName(OpenFile.Filename);
            loadFiles[nameof(lblxlsRoutes)] = OpenFile.Filename;
        }

        private void BtnCmpRoutes_Click(object sender, EventArgs e)
        {
            string sTypes = "xlsm; xlsx";
            OpenFile = new AcadWindows
                        .OpenFileDialog("Compound Routes", "", sTypes, "OpenFile",
                         AcadWindows.OpenFileDialog.OpenFileDialogFlags.SearchPath);
            if (OpenFile.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                //loadFiles[nameof(lblxlsCmpRoutes)] = null;
                //lblxlsCmpRoutes.Text = "Compound Routes";
                return;
            }
            lblxlsCmpRoutes.Text = Path.GetFileName(OpenFile.Filename);
            loadFiles[nameof(lblxlsCmpRoutes)] = OpenFile.Filename;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            string sTypes = "xlsx; xlsm";
            OpenFile = new AcadWindows
                        .OpenFileDialog("Level Crossings", "", sTypes, "OpenFile",
                         AcadWindows.OpenFileDialog.OpenFileDialogFlags.SearchPath);
            if (OpenFile.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                //loadFiles[nameof(lblxlsLxs)] = null;
                //lblxlsLxs.Text = "Level Crossings";
                return;
            }
            lblxlsLxs.Text = Path.GetFileName(OpenFile.Filename);
            loadFiles[nameof(lblxlsLxs)] = OpenFile.Filename;
        }

        private void BtnBgs_Click(object sender, EventArgs e)
        {
            string sTypes = "xlsx; xlsm";
            OpenFile = new AcadWindows
                        .OpenFileDialog("Balise Groups", "", sTypes, "OpenFile",
                         AcadWindows.OpenFileDialog.OpenFileDialogFlags.SearchPath);
            if (OpenFile.ShowDialog() != DialogResult.OK)
            {
                //loadFiles[nameof(lblxlsBgs)] = null;
                //lblxlsBgs.Text = "Balise Groups";
                return;
            }
            lblxlsBgs.Text = Path.GetFileName(OpenFile.Filename);
            loadFiles[nameof(lblxlsBgs)] = OpenFile.Filename;
        }


        private void CheckBoxDL_CheckedChanged(object sender, EventArgs e)
        {
            btnDetLock.Enabled = lblxlsDetLock.Enabled = checkBoxDL.Checked;
        }

        private void CheckBoxFP_CheckedChanged(object sender, EventArgs e)
        {
            btnFP.Enabled = lblxlsFP.Enabled = checkBoxFP.Checked;
        }

        private void CheckBoxEmSt_CheckedChanged(object sender, EventArgs e)
        {
            btnEmSg.Enabled = lblxlsEmSg.Enabled = checkBoxEmSt.Checked;
        }

        private void CheckBoxSpProf_CheckedChanged(object sender, EventArgs e)
        {
            btnSpProf.Enabled = lblxlsSpProf.Enabled = checkBoxSpProf.Checked;
        }

        private void CheckBoxRts_CheckedChanged(object sender, EventArgs e)
        {
            btnRoutes.Enabled = lblxlsRoutes.Enabled = checkBoxRts.Checked;
        }

        private void CheckBoxCmRts_CheckedChanged(object sender, EventArgs e)
        {
            btnCmpRoutes.Enabled = lblxlsCmpRoutes.Enabled = checkBoxCmRts.Checked;
        }

        private void CheckBoxLX_CheckedChanged(object sender, EventArgs e)
        {
            btnLXs.Enabled = lblxlsLxs.Enabled = checkBoxLX.Checked;
        }

        private void CheckBoxBG_CheckedChanged(object sender, EventArgs e)
        {
            BtnBgs.Enabled = lblxlsBgs.Enabled = checkBoxBG.Checked;
        }


        private void FrmStation_Load(object sender, EventArgs e)
        {
            dgwLines.ClearSelection();

        }

        private void BtnRdd_Click(object sender, EventArgs e)
        {
            OpenFile = new AcadWindows
                        .OpenFileDialog("Previous Rdd", "", "xml", "OpenFile",
                         AcadWindows.OpenFileDialog.OpenFileDialogFlags.SearchPath);
            if (OpenFile.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                //loadFiles[nameof(lblxlsRoutes)] = null;
                //lblxlsRoutes.Text = "Routes";
                return;
            }
            lblxlsRdd.Text = Path.GetFileName(OpenFile.Filename);
            loadFiles[nameof(lblxlsRdd)] = OpenFile.Filename;
        }

        private void CheckBoxRdd_CheckedChanged(object sender, EventArgs e)
        {
            btnRdd.Enabled = lblxlsRdd.Enabled = checkBoxRdd.Checked;
        }

        private void TxtBoxVersion_TextChanged(object sender, EventArgs e)
        {
            SetVersion(txtBoxVersion.Text);
            loadFiles["VersId"] = txtBoxVersion.Text;
            Regex version = new Regex("^[0-9]{2}[P]{0,1}[0-9]{0,2}");
            if (txtBoxVersion.Text == "01P01" || txtBoxVersion.Text == "" || !version.IsMatch(txtBoxVersion.Text))
            {
                lblxlsOrderRdd.Enabled = false;
                btnOrderRdd.Enabled = false;
                checkBoxLevel.Enabled = false;
                groupBox5.Enabled = false;
                groupBox3.Enabled = false;
                checkBoxRdd.Checked = false;
                OrderRdd = false;
            }
            else
            {
                lblxlsOrderRdd.Enabled = true;
                btnOrderRdd.Enabled = true;
                checkBoxLevel.Enabled = true;
                groupBox5.Enabled = true;
                groupBox3.Enabled = true;
                checkBoxRdd.Checked = true;
                OrderRdd = true;
            }
        }

        private void CheckBoxBGN_CheckedChanged(object sender, EventArgs e)
        {
            BtnBgsN.Enabled = lblxlsBgsN.Enabled = checkBoxBGN.Checked;
        }

        private void BtnBgsN_Click(object sender, EventArgs e)
        {
            OpenFile = new AcadWindows
                        .OpenFileDialog("Balise Groups Neighbor Station", "", "xlsx", "OpenFile",
                         AcadWindows.OpenFileDialog.OpenFileDialogFlags.SearchPath);
            if (OpenFile.ShowDialog() != DialogResult.OK)
            {
                //loadFiles[nameof(lblxlsBgs)] = null;
                //lblxlsBgs.Text = "Balise Groups";
                return;
            }
            lblxlsBgsN.Text = Path.GetFileName(OpenFile.Filename);
            loadFiles[nameof(lblxlsBgsN)] = OpenFile.Filename;
        }

        private void BtnSigClos_Click(object sender, EventArgs e)
        {
            OpenFile = new AcadWindows
                        .OpenFileDialog("Signals Closure", "", "xlsx", "OpenFile",
                         AcadWindows.OpenFileDialog.OpenFileDialogFlags.SearchPath);
            if (OpenFile.ShowDialog() != DialogResult.OK)
            {
                //loadFiles[nameof(lblxlsBgs)] = null;
                //lblxlsBgs.Text = "Balise Groups";
                return;
            }
            lblxlsSigClos.Text = Path.GetFileName(OpenFile.Filename);
            loadFiles[nameof(lblxlsSigClos)] = OpenFile.Filename;
        }

        private void BtnSigClosN_Click(object sender, EventArgs e)
        {
            OpenFile = new AcadWindows
                        .OpenFileDialog("Signals Closure Neighbor Station", "", "xlsx", "OpenFile",
                         AcadWindows.OpenFileDialog.OpenFileDialogFlags.SearchPath);
            if (OpenFile.ShowDialog() != DialogResult.OK)
            {
                //loadFiles[nameof(lblxlsBgs)] = null;
                //lblxlsBgs.Text = "Balise Groups";
                return;
            }
            lblxlsSigClosN.Text = Path.GetFileName(OpenFile.Filename);
            loadFiles[nameof(lblxlsSigClosN)] = OpenFile.Filename;
        }

        private void CheckBoxSC_CheckedChanged(object sender, EventArgs e)
        {
            btnSigClos.Enabled = lblxlsSigClos.Enabled = checkBoxSC.Checked;
        }

        private void CheckBoxSCN_CheckedChanged(object sender, EventArgs e)
        {
            btnSigClosN.Enabled = lblxlsSigClosN.Enabled = checkBoxSCN.Checked;
        }

        private void CmbLines_SelectedIndexChanged(object sender, EventArgs e)
        {
            zeroLevelLine = ((ComboBox)sender).SelectedItem.ToString();
        }

        private void TxtBoxDocId_TextChanged(object sender, EventArgs e)
        {
            SetDocId(txtBoxDocId.Text);
            loadFiles["DocId"] = txtBoxDocId.Text;
        }

        private void BtnOrderRdd_Click(object sender, EventArgs e)
        {
            OpenFile = new AcadWindows
                        .OpenFileDialog("Order By Rdd", "", "xml", "OpenFile",
                         AcadWindows.OpenFileDialog.OpenFileDialogFlags.SearchPath);
            if (OpenFile.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                //loadFiles[nameof(lblxlsRoutes)] = null;
                //lblxlsRoutes.Text = "Routes";
                return;
            }
            lblxlsOrderRdd.Text = Path.GetFileName(OpenFile.Filename);
            loadFiles[nameof(lblxlsOrderRdd)] = OpenFile.Filename;
        }

        private void CheckBoxRdd_CheckedChanged_1(object sender, EventArgs e)
        {
            btnRdd.Enabled = lblxlsRdd.Enabled = checkBoxRdd.Checked;
        }

        private void CheckBoxLevel_CheckedChanged(object sender, EventArgs e)
        {
            CopyLevel = checkBoxLevel.Checked;
        }

        private void CheckBoxAc_CheckedChanged(object sender, EventArgs e)
        {
            AutoAC = checkBoxAc.Checked;
        }

        private void CmbAuthor_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            if (comboBox.SelectedIndex > -1)
            {
                author = comboBox.SelectedValue.ToString();
            }
        }
    }
}
