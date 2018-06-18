using System.Windows.Forms;

namespace Digger.Win32
{
    public partial class DlgLevel : Form
    {
        public DlgLevel()
        {
            InitializeComponent();

            cbLevel.SelectedIndex = 0;
        }

        public string LevelFilePath { get; set; }

        private void btnBrowse_Click(object sender, System.EventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "Digger level files|*.DLF" };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                LevelFilePath = dlg.FileName;
                txtPath.Text = System.IO.Path.GetFileName(dlg.FileName);
            }
        }

        private void rdoCustom_CheckedChanged(object sender, System.EventArgs e)
        {
            btnBrowse.Enabled = rdoCustom.Checked;
            txtPath.Enabled = rdoCustom.Checked;
        }
    }
}
