using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClosedXML.Excel;

namespace KurseWork_Network
{
    public partial class Form3 : Form
    {
        private Form1 form1; // Посилання на Form1
        private DataGridView dgvPackets; // Таблиця для відображення пакетів
        private NumericUpDown nudMessageSize, nudPacketSize, nudServiceInfoSize; // Поля для введення розміру повідомлення, пакета та службової інформації
        private Button btnAnalyze; // Кнопка для запуску аналізу
        private ComboBox cbProtocol; // Вибір протоколу
        private Label lblStartNode, lblEndNode; // Мітки для відображення стартової та кінцевої ноди


        public Form3(Form1 form1)
        {
            this.form1 = form1;
            InitializeComponent();

            // Таблиця для відображення переданих пакетів
            dgvPackets = new DataGridView
            {
                Location = new System.Drawing.Point(12, 12),
                Size = new System.Drawing.Size(743, 300),
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
            };

            // Додавання стовпців
            dgvPackets.Columns.Add("PacketNumber", "Номер пакету");
            dgvPackets.Columns.Add("PacketSize", "Розмір пакету (байт)");
            dgvPackets.Columns.Add("Protocol", "Протокол");
            dgvPackets.Columns.Add("ServiceInfoSize", "Службова інформація (байт)");
            dgvPackets.Columns.Add("TimeSpent", "Затрачений час (мс)");
            dgvPackets.Columns.Add("TotalDelivered", "Всього доставлено (байт)");
            dgvPackets.Columns.Add("PacketStatus", "Статус пакету");


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
                Text = "Розмір повідомлення (байт):",
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
                Text = "Розмір пакету (байт):",
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
                Text = "Службова інформація (байт):",
                Location = new System.Drawing.Point(12, 410),
                Size = new System.Drawing.Size(200, 20)
            };

            // Кнопка для запуску аналізу
            btnAnalyze = new Button
            {
                Text = "Запустити аналіз",
                Location = new System.Drawing.Point(12, 470),
                Size = new System.Drawing.Size(150, 30)
            };
            btnAnalyze.Click += BtnAnalyze_Click;

            // Вибір протоколу
            cbProtocol = new ComboBox
            {
                Location = new System.Drawing.Point(430, 330),
                Size = new System.Drawing.Size(120, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cbProtocol.Items.AddRange(new string[] { "TCP", "UDP" });
            cbProtocol.SelectedIndex = 0; // По умолчанию TCP
            cbProtocol.SelectedIndexChanged += cbProtocol_SelectedIndexChanged;

            // Обозначення стартової ноди
            lblStartNode = new Label
            {
                Text = "Стартова нода: None",
                Location = new System.Drawing.Point(430, 355),
                Size = new System.Drawing.Size(200, 20),
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.Green
            };
            Controls.Add(lblStartNode);

            // Обозначення кінцевої ноди
            lblEndNode = new Label
            {
                Text = "Кінцева нода: None",
                Location = new System.Drawing.Point(430, 380),
                Size = new System.Drawing.Size(200, 20),
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.Red
            };
            Controls.Add(lblEndNode);

            Button btnSaveToExcel = new Button
            {
                Text = "Зберегти в Excel",
                Location = new System.Drawing.Point(180, 470),
                Size = new System.Drawing.Size(150, 30)
            };
            btnSaveToExcel.Click += BtnSaveToExcel_Click;
            Controls.Add(btnSaveToExcel);

            // Додавання елементів на форму
            Controls.Add(dgvPackets);
            Controls.Add(lblMessageSize);
            Controls.Add(nudMessageSize);
            Controls.Add(lblPacketSize);
            Controls.Add(nudPacketSize);
            Controls.Add(lblServiceInfoSize);
            Controls.Add(nudServiceInfoSize);
            Controls.Add(btnAnalyze);
            Controls.Add(cbProtocol);

            Button btnComplexAnalyze = new Button
            {
                Text = "Комплексний аналіз",
                Location = new System.Drawing.Point(350, 470),
                Size = new System.Drawing.Size(150, 30)
            };
            btnComplexAnalyze.Click += BtnComplexAnalyze_Click;
            Controls.Add(btnComplexAnalyze);

            // Налаштування форми
            Text = "Аналіз мережі";
            Size = new System.Drawing.Size(780, 550);

            string selectedItem = cbProtocol.SelectedItem.ToString();
            nudServiceInfoSize.Value = NetworkAnalyzer.ProtocolsInfoSize[selectedItem];
        }

        private void Form3_Load(object sender, EventArgs e)
        {

        }

        private void cbProtocol_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Получение выбранного элемента
            string selectedItem = cbProtocol.SelectedItem.ToString();

            nudServiceInfoSize.Value = NetworkAnalyzer.ProtocolsInfoSize[selectedItem];
        }


        /// <summary>
        /// Метод-обробник натискання кнопки "Аналіз"
        /// </summary>
        /// <param name="sender">Джерело події</param>
        /// <param name="e">Параметри події</param>
        /// <remarks>
        /// Метод аналізує трафік в мережі, використовуючи вибраний протокол,
        /// і виводить результати в таблицю.
        private void BtnAnalyze_Click(object? sender, EventArgs e)
        {
            if (form1 == null || form1.tree == null)
            {
                MessageBox.Show("Мережа не знайдена!");
                return;
            }

            // Отримуємо параметри
            int messageSize = (int)nudMessageSize.Value;
            int packetSize = (int)nudPacketSize.Value;
            int serviceInfoSize = (int)nudServiceInfoSize.Value;
            string protocol = cbProtocol.SelectedItem.ToString();

            // Корисний вантаж в одному пакеті
            int payloadSize = packetSize - serviceInfoSize;

            if (payloadSize <= 0)
            {
                MessageBox.Show("Розмiр службової інформації перевищує або дорiвнює розмiру пакета!");
                return;
            }

            if(form1.selectedNodes.Count < 2)
            {
                MessageBox.Show("Виберiть хоча б два вузли!");
                return;
            }

            NetworkAnalyzer analyzer = new NetworkAnalyzer(form1.tree);

            dgvPackets.Rows.Clear();

            int totalDelivered = 0;
            double totalTimeSpent = 0;
            double totalserviceInfoSize = 0;
            bool isFirstConnection = true;

            int packetCount = (int)Math.Ceiling((double)messageSize / payloadSize);
            for (int i = 1; i <= packetCount; i++)
            {
                // Размер реальной информации в текущем пакете
                int currentPacketPayload = Math.Min(payloadSize, messageSize - totalDelivered);

                // Затраченное время зависит от протокола
                var timeSpent = protocol == "TCP"
                    ? analyzer.AnalyzeTraffic(
                        form1.selectedNodes.First(),
                        form1.selectedNodes.Last(),
                        isFirstConnection)
                    : analyzer.AnalyzeTraffic(
                        form1.selectedNodes.First(),
                        form1.selectedNodes.Last());

                if (timeSpent.status == Status.Ok)
                {
                    totalDelivered += currentPacketPayload;
                    totalserviceInfoSize += serviceInfoSize;

                    if(protocol == "TCP" && isFirstConnection)
                    {
                        totalserviceInfoSize += NetworkAnalyzer.ProtocolsInfoSize[protocol] * 2;
                    }
                    isFirstConnection = false;
                }
                else if (protocol == "TCP")
                {
                    i--;
                }
                else if (protocol == "UDP")
                {
                    totalDelivered += currentPacketPayload;
                    totalserviceInfoSize += serviceInfoSize;
                }

                totalTimeSpent += timeSpent.totalTime;

                dgvPackets.Rows.Add(i, currentPacketPayload + serviceInfoSize, protocol, totalserviceInfoSize, totalTimeSpent, totalDelivered, EnumHelper.GetDescription(timeSpent.status));
            }
        }

        private void BtnComplexAnalyze_Click(object sender, EventArgs e)
        {
            if (form1 == null || form1.tree == null)
            {
                MessageBox.Show("Мережа не знайдена!");
                return;
            }

            dgvPackets.Rows.Clear(); // Очищаем таблицу перед анализом

            string protocol = cbProtocol.SelectedItem.ToString();
            int packetSize = (int)nudPacketSize.Value;
            int serviceInfoSize = NetworkAnalyzer.ProtocolsInfoSize[protocol];

            // Выполняем анализ для размеров сообщения от 1000 до 15000 с шагом 1000
            for (int messageSize = 1000; messageSize <= 15000; messageSize += 1000)
            {
                int payloadSize = packetSize - serviceInfoSize;

                if (payloadSize <= 0)
                {
                    MessageBox.Show("Розмiр службової інформації перевищує або дорiвнює розмiру пакета!");
                    return;
                }

                int totalDelivered = 0;
                double totalTimeSpent = 0;
                int totalPackets = (int)Math.Ceiling((double)messageSize / payloadSize);
                double totalserviceInfoSize = 0;

                bool isFirstConnection = true;

                NetworkAnalyzer analyzer = new NetworkAnalyzer(form1.tree);

                for (int i = 1; i <= totalPackets; i++)
                {
                    int currentPacketPayload = Math.Min(payloadSize, messageSize - totalDelivered);

                    // Выполняем анализ
                    var timeSpent = protocol == "TCP"
                        ? analyzer.AnalyzeTraffic(form1.selectedNodes.First(), form1.selectedNodes.Last(), isFirstConnection)
                        : analyzer.AnalyzeTraffic(form1.selectedNodes.First(), form1.selectedNodes.Last());

                    if (timeSpent.status == Status.Ok)
                    {
                        totalDelivered += currentPacketPayload;
                        totalserviceInfoSize += serviceInfoSize;

                        if (protocol == "TCP" && isFirstConnection)
                        {
                            totalserviceInfoSize += NetworkAnalyzer.ProtocolsInfoSize[protocol] * 2;
                        }
                        isFirstConnection = false;
                    }
                    else if (protocol == "TCP")
                    {
                        i--;
                    }
                    else if (protocol == "UDP")
                    {
                        totalDelivered += currentPacketPayload;
                        totalserviceInfoSize += serviceInfoSize;
                    }

                    totalTimeSpent += timeSpent.totalTime;
                }

                // Добавляем последнюю запись анализа в таблицу
                dgvPackets.Rows.Add(
                    totalPackets,  // Номер пакету
                    packetSize,                // Размер пакета
                    protocol,                  // Протокол
                    totalserviceInfoSize,           // Служебная информация
                    totalTimeSpent, // Затраченное время (мс)
                    totalDelivered,            // Всего доставлено (байт)
                    "Ok"                       // Статус пакета
                );
            }
        }

            /// <summary>
            /// Оновлює текстові мітки початкового та кінцевого вузлів на основі
            /// заданих параметрів.
            /// </summary>
            /// <param name="startNode">Початковий вузол.</param>
            /// <param name="endNode">Кінцевий вузол.</param>
            public void UpdateNodeLabels(Node startNode, Node endNode)
        {
            if (startNode != null)
            {
                lblStartNode.Text = $"Початковий вузол: {startNode?.Id ?? "None"}";
                lblEndNode.Text = $"Кінцевий вузол: {endNode?.Id ?? "None"}";
            }
            else
            {
                lblStartNode.Text = $"Початковий вузол: None";
                lblEndNode.Text = $"Кінцевий вузол: None";
            }
        }

        /// <summary>
        /// Експортує дані з DataGridView до Excel файлу.
        /// </summary>
        /// <param name="sender">Джерело події.</param>
        /// <param name="e">Параметри події.</param>
        ///
        private void BtnSaveToExcel_Click(object sender, EventArgs e)
        {
            if (dgvPackets.Rows.Count == 0)
            {
                MessageBox.Show("Таблиця порожня. Немає чого експортувати.");
                return;
            }

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Таблиця");

                // Заголовки стовпців
                for (int i = 0; i < dgvPackets.Columns.Count; i++)
                {
                    worksheet.Cell(1, i + 1).Value = dgvPackets.Columns[i].HeaderText;
                }

                // Дані таблиці
                for (int i = 0; i < dgvPackets.Rows.Count; i++)
                {
                    for (int j = 0; j < dgvPackets.Columns.Count; j++)
                    {
                        worksheet.Cell(i + 2, j + 1).Value = dgvPackets.Rows[i].Cells[j].Value?.ToString();
                    }
                }

                // Збереження файлу
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel файли (*.xlsx)|*.xlsx",
                    Title = "Зберегти як Excel файл"
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    workbook.SaveAs(saveFileDialog.FileName);
                    MessageBox.Show("Таблиця успішно збережена в Excel файл.");
                }
            }
        }
    }
}
