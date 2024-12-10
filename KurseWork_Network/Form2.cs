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
        private DataGridView dgvDistances = new();
        private ListBox lbRoutes = new();
        private Form1 form1;


        public Form2()
        {
            InitializeComponent();

            // Настройка DataGridView
            dgvDistances.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvDistances.Location = new System.Drawing.Point(12, 12);
            dgvDistances.Name = "dgvDistances";
            dgvDistances.Size = new System.Drawing.Size(500, 200);
            dgvDistances.TabIndex = 0;
            dgvDistances.Columns.Add("Node", "Узел");
            dgvDistances.Columns.Add("Distance", "Расстояние");
            dgvDistances.Columns.Add("Transit", "Транзиты");
            dgvDistances.ReadOnly = true;
            dgvDistances.AllowUserToAddRows = false;
            dgvDistances.AllowUserToDeleteRows = false;
            dgvDistances.AllowUserToOrderColumns = false;


            // Настройка ListBox
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

        public void LoadData(List<(string NodeId, int Distance, int TransitCount)> distances,
                         List<(string NodeId, string Path)> routes)
        {

            // Очистка таблицы и списка
            dgvDistances.Rows.Clear();
            lbRoutes.Items.Clear();

            // Заполнение таблицы расстояний
            foreach (var (NodeId, Distance, TransitCount) in distances)
            {
                dgvDistances.Rows.Add(NodeId,
                                      Distance == int.MaxValue ? "∞" : Distance.ToString(),
                                      TransitCount == int.MaxValue ? "-" : TransitCount.ToString());
            }

            // Заполнение списка маршрутов
            foreach (var (NodeId, Path) in routes)
            {
                lbRoutes.Items.Add($"{NodeId}: {Path}");
            }
        }


    }
}
