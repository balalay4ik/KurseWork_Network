using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KurseWork_Network
{
    public partial class Form3 : Form
    {
        private Form1 form1; // Ссылка на Form1
        private DataGridView dgvPackets;
        private NumericUpDown nudMessageSize, nudPacketSize, nudServiceInfoSize;
        private Button btnAnalyze;
        private ComboBox cbProtocol;
        private Label lblStartNode, lblEndNode;


        public Form3(Form1 form1)
        {
            this.form1 = form1;
            InitializeComponent();

            // Таблица для отображения переданных пакетов
            dgvPackets = new DataGridView
            {
                Location = new System.Drawing.Point(12, 12),
                Size = new System.Drawing.Size(700, 300),
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
            };

            // Добавление столбцов
            dgvPackets.Columns.Add("PacketNumber", "Номер пакета");
            dgvPackets.Columns.Add("PacketSize", "Размер пакета (байт)");
            dgvPackets.Columns.Add("Protocol", "Протокол");
            dgvPackets.Columns.Add("ServiceInfoSize", "Служебная информация (байт)");
            dgvPackets.Columns.Add("TimeSpent", "Затраченное время (мс)");
            dgvPackets.Columns.Add("TotalDelivered", "Всего доставлено (байт)");
            dgvPackets.Columns.Add("PacketStatus", "Статус пакета");


            // Поля для настройки
            nudMessageSize = new NumericUpDown
            {
                Location = new System.Drawing.Point(12, 330),
                Minimum = 1,
                Maximum = 100000,
                Value = 10000,
                Increment = 100,
                Size = new System.Drawing.Size(120, 20)
            };
            Label lblMessageSize = new Label
            {
                Text = "Размер сообщения (байт):",
                Location = new System.Drawing.Point(12, 310),
                Size = new System.Drawing.Size(200, 20)
            };

            nudPacketSize = new NumericUpDown
            {
                Location = new System.Drawing.Point(12, 380),
                Minimum = 1,
                Maximum = 10000,
                Value = 1000,
                Increment = 50,
                Size = new System.Drawing.Size(120, 20)
            };
            Label lblPacketSize = new Label
            {
                Text = "Размер пакета (байт):",
                Location = new System.Drawing.Point(12, 360),
                Size = new System.Drawing.Size(200, 20)
            };

            nudServiceInfoSize = new NumericUpDown
            {
                Location = new System.Drawing.Point(12, 430),
                Minimum = 1,
                Maximum = 1000,
                Value = 100,
                Increment = 10,
                Size = new System.Drawing.Size(120, 20)
            };
            Label lblServiceInfoSize = new Label
            {
                Text = "Служебная информация (байт):",
                Location = new System.Drawing.Point(12, 410),
                Size = new System.Drawing.Size(200, 20)
            };

            // Кнопка для запуска анализа
            btnAnalyze = new Button
            {
                Text = "Запустить анализ",
                Location = new System.Drawing.Point(12, 470),
                Size = new System.Drawing.Size(150, 30)
            };
            btnAnalyze.Click += BtnAnalyze_Click;

            // Выбор протокола
            cbProtocol = new ComboBox
            {
                Location = new System.Drawing.Point(430, 330),
                Size = new System.Drawing.Size(120, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cbProtocol.Items.AddRange(new string[] { "TCP", "UDP" });
            cbProtocol.SelectedIndex = 0; // По умолчанию TCP

            // Обозначение стартовой ноды
            lblStartNode = new Label
            {
                Text = "Стартовая нода: None",
                Location = new System.Drawing.Point(430, 355),
                Size = new System.Drawing.Size(200, 20),
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.Green
            };
            Controls.Add(lblStartNode);

            // Обозначение конечной ноды
            lblEndNode = new Label
            {
                Text = "Конечная нода: None",
                Location = new System.Drawing.Point(430, 380),
                Size = new System.Drawing.Size(200, 20),
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.Red
            };
            Controls.Add(lblEndNode);


            // Добавляем элементы на форму
            Controls.Add(dgvPackets);
            Controls.Add(lblMessageSize);
            Controls.Add(nudMessageSize);
            Controls.Add(lblPacketSize);
            Controls.Add(nudPacketSize);
            Controls.Add(lblServiceInfoSize);
            Controls.Add(nudServiceInfoSize);
            Controls.Add(btnAnalyze);
            Controls.Add(cbProtocol);

            // Настройки формы
            Text = "Анализ сети";
            Size = new System.Drawing.Size(760, 550);
        }

        private void Form3_Load(object sender, EventArgs e)
        {

        }

        // Обработчик кнопки анализа
        private void BtnAnalyze_Click(object? sender, EventArgs e)
        {
            if (form1 == null || form1.tree == null)
            {
                MessageBox.Show("Сеть не найдена!");
                return;
            }

            // Получаем параметры
            int messageSize = (int)nudMessageSize.Value;
            int packetSize = (int)nudPacketSize.Value;
            int serviceInfoSize = (int)nudServiceInfoSize.Value;
            string protocol = cbProtocol.SelectedItem.ToString();

            // Полезная нагрузка в одном пакете
            int payloadSize = packetSize - serviceInfoSize;

            if (payloadSize <= 0)
            {
                MessageBox.Show("Размер служебной информации превышает или равен размеру пакета!");
                return;
            }

            NetworkAnalyzer analyzer = new NetworkAnalyzer(form1.tree);

            dgvPackets.Rows.Clear();

            int totalDelivered = 0;
            double totalTimeSpent = 0;
            bool isFirstConnection = true;

            int packetCount = (int)Math.Ceiling((double)messageSize / payloadSize);
            for (int i = 1; i <= packetCount; i++)
            {
                // Размер реальной информации в текущем пакете
                int currentPacketPayload = Math.Min(payloadSize, messageSize - totalDelivered);

                // Затраченное время зависит от протокола
                var timeSpent = protocol == "TCP"
                    ? analyzer.AnalyzeTraffic(
                        form1.tree.Nodes.First(),
                        form1.tree.Nodes.Last(),
                        isFirstConnection)
                    : analyzer.AnalyzeTraffic(
                        form1.tree.Nodes.First(),
                        form1.tree.Nodes.Last());

                if (timeSpent.status == Status.Ok)
                {
                    totalDelivered += currentPacketPayload;
                    isFirstConnection = false;
                }
                else
                {
                    i--;
                }

                totalTimeSpent += timeSpent.totalTime;

                dgvPackets.Rows.Add(i, currentPacketPayload + serviceInfoSize, protocol, serviceInfoSize, totalTimeSpent, totalDelivered, EnumHelper.GetDescription(timeSpent.status));
            }
        }

        public void UpdateNodeLabels(Node startNode, Node endNode)
        {
            if (startNode != null)
            {
                lblStartNode.Text = $"Стартовая нода: {startNode?.Id ?? "None"}";
                lblEndNode.Text = $"Конечная нода: {endNode?.Id ?? "None"}";
            }
            else
            {
                lblStartNode.Text = $"Стартовая нода: None";
                lblEndNode.Text = $"Конечная нода: None";
            }
        }
    }
}
