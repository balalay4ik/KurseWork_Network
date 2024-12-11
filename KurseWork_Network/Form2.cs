using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.ComponentModel.Design.ObjectSelectorEditor;

namespace KurseWork_Network
{
    public partial class Form2 : Form
    {
        
        private DataGridView dgvDistances = new(); // DataGridView для відображення відстаней між вузлами.
        private ListBox lbRoutes = new(); // ListBox для відображення маршрутів.     
        private Form1 form1; // Посилання на Form1, necessary for data exchange.


        public Form2()
        {
            InitializeComponent();

            // Налаштування DataGridView
            dgvDistances.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvDistances.Location = new System.Drawing.Point(12, 12);
            dgvDistances.Name = "dgvDistances";
            dgvDistances.Size = new System.Drawing.Size(500, 200);
            dgvDistances.TabIndex = 0;
            dgvDistances.Columns.Add("Node", "Вузол");
            dgvDistances.Columns.Add("Distance", "Відстань");
            dgvDistances.Columns.Add("Transit", "Транзити");
            dgvDistances.ReadOnly = true;
            dgvDistances.AllowUserToAddRows = false;
            dgvDistances.AllowUserToDeleteRows = false;
            dgvDistances.AllowUserToOrderColumns = false;

            // Налаштування ListBox
            lbRoutes.FormattingEnabled = true;
            lbRoutes.Location = new System.Drawing.Point(12, 220);
            lbRoutes.Name = "lbRoutes";
            lbRoutes.Size = new System.Drawing.Size(500, 200);
            lbRoutes.TabIndex = 1;

            Controls.Add(dgvDistances);
            Controls.Add(lbRoutes);
        }

        public void SetForm1(Form1 form1)
        {
            this.form1 = form1;
        }

        private void Form2_Load(object sender, EventArgs e)
        {


        }

        /// <summary>
        /// Завантажує дані до форми. Очищає таблицю відстаней та список маршрутів,
        /// а потім заповнює їх новими даними.
        /// </summary>
        /// <param name="distances">Таблиця відстаней. Елемент - трійка (Id вузла, відстань, к-ть транзитів)</param>
        /// <param name="routes">Список маршрутів. Елемент - пара (Id вузла, маршрут)</param>
        public void LoadData(List<(string NodeId, int Distance, int TransitCount)> distances,
                         List<(string NodeId, string Path)> routes)
        {

            // Очищення таблиці та списку
            dgvDistances.Rows.Clear();
            lbRoutes.Items.Clear();

            // Заповнення таблиці відстаней
            foreach (var (NodeId, Distance, TransitCount) in distances)
            {
                dgvDistances.Rows.Add(NodeId,
                                      Distance == int.MaxValue ? " " : Distance.ToString(),
                                      TransitCount == int.MaxValue ? "-" : TransitCount.ToString());
            }

            // Заповнення списку маршрути
            foreach (var (NodeId, Path) in routes)
            {
                lbRoutes.Items.Add($"{NodeId}: {Path}");
            }
        }


    }
}
