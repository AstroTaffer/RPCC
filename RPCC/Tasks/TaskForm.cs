using System;
using System.Globalization;
using System.Windows.Forms;

namespace RPCC.Tasks
{
    public partial class TaskForm : Form
    {
        public TaskForm()
        {
            InitializeComponent();
            textBoxDateTime.Text = DateTime.UtcNow.ToString(CultureInfo.CurrentCulture);
        }
    }
}