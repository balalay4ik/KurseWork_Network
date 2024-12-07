using System.Windows.Forms;
using static System.ComponentModel.Design.ObjectSelectorEditor;

namespace KurseWork_Network
{
    public partial class Form1 : Form
    {
        private PictureBox pbCanvas; // ���� ��� PictureBox
        private Tree tree = new Tree(); // ������
        private List<Node> selectedNodes = new List<Node>(); // ������ ���������� �����
        private Point mouseStart; // ��������� ������� ����
        private bool isDragging = false; // ����, ����������� �� ����� �����������
        private NumericUpDown numericUpDown1 = new();
        private NumericUpDown numericUpDown2 = new();
        private Button btn = new();
        private Button btn2 = new();
        private Form2? form2 = null; // ������ �� ������ �����
        private bool form2closed = true;

        public Form1()
        {
            InitializeComponent();

            // ������ PictureBox
            pbCanvas = new PictureBox
            {
                Name = "pbCanvas",
                Size = new Size(600, 560),
                Location = new Point(1, 0),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            numericUpDown1.Location = new Point(605, 10);
            numericUpDown1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            numericUpDown1.Value = 0;
            numericUpDown2.Location = new Point(605, 40);
            numericUpDown2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            numericUpDown2.Value = 0;

            btn.Location = new Point(605, 70);
            btn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btn.Click += btnGenerate_Click;

            btn2.Location = new Point(605, 110);
            btn2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btn2.Click += btnOpenForm2_Click;

            Controls.Add(pbCanvas);
            Controls.Add(numericUpDown1);
            Controls.Add(numericUpDown2);
            Controls.Add(btn);
            Controls.Add(btn2);

            // ���������� ����������� ������� ����
            pbCanvas.Paint += PbCanvas_Paint;
            pbCanvas.MouseDown += PbCanvas_MouseDown;
            pbCanvas.MouseMove += PbCanvas_MouseMove;
            pbCanvas.MouseUp += PbCanvas_MouseUp;

        }

        // ���������� ������� ����
        private void PbCanvas_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Node? clickedNode = FindNodeAt(e.X, e.Y);
                

                if (ModifierKeys.HasFlag(Keys.Control)) // ��������� � Ctrl
                {
                    if (clickedNode != null)
                    {
                        if (selectedNodes.Contains(clickedNode))
                        {
                            selectedNodes.Remove(clickedNode); // ����� ���������
                        }
                        else
                        {
                            selectedNodes.Add(clickedNode); // �������� � ����������
                        }
                    }
                }
                else
                {
                    if (clickedNode != null)
                    {
                        if (!selectedNodes.Contains(clickedNode))
                        {
                            selectedNodes.Clear(); // ����� ��������� �� ���� �����
                            selectedNodes.Add(clickedNode); // �������� ������ �������
                        }
                    }
                    else
                    {
                        selectedNodes.Clear(); // �������� ���������� ����
                    }
                }

                // ������������� ��������� ����� ��� �����������
                mouseStart = new Point(e.X, e.Y);
                isDragging = true;

                if(!form2closed)
                    LoadDataToForm2();

                pbCanvas.Invalidate(); // ������������
            }
        }

        // ���������� ����������� ����
        private void PbCanvas_MouseMove(object? sender, MouseEventArgs e)
        {
            if (isDragging && selectedNodes.Count > 0 && e.Button == MouseButtons.Left)
            {
                int deltaX = e.X - mouseStart.X; // �������� �� X
                int deltaY = e.Y - mouseStart.Y; // �������� �� Y

                foreach (var node in selectedNodes)
                {
                    node.X += deltaX;
                    node.Y += deltaY;
                }

                // ��������� ��������� ������� ����
                mouseStart = new Point(e.X, e.Y);

                pbCanvas.Invalidate(); // ������������
            }
        }

        // ���������� ���������� ����
        private void PbCanvas_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false; // ��������� �����������
            }
        }

        // ��������� ������
        private void PbCanvas_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // ������ ����� ������
            tree.Draw(g);

            // ������������ ���������� ����
            foreach (var node in selectedNodes)
            {
                DrawNodeHighlight(g, node);
            }
        }

        // ��������� ��������� ����
        private void DrawNodeHighlight(Graphics g, Node node)
        {
            int highlightRadius = 20; // ������ ���������
            g.DrawEllipse(Pens.Red, node.X - highlightRadius, node.Y - highlightRadius, highlightRadius * 2, highlightRadius * 2);
        }


        private void GenerateRegionsAndCommunications(int regionCount, int connectionsPerRegion)
        {
            RegionGenerator generator = new RegionGenerator();
            Tree generatedTree = generator.Generate(regionCount, connectionsPerRegion, 50);

            // ������������� ��������������� ������ ��� �������
            tree = generatedTree;

            // ��������� �����������
            pbCanvas.Invalidate();
        }

        private void btnGenerate_Click(object? sender, EventArgs e)
        {
            int regionCount = (int)numericUpDown1.Value;
            int connectionsPerRegion = (int)numericUpDown2.Value;

            GenerateRegionsAndCommunications(regionCount, connectionsPerRegion);
        }

        // ����� ���� ��� ���������� ����
        private Node? FindNodeAt(int x, int y)
        {
            foreach (var node in tree.Nodes)
            {
                if (node.IsPointInside(x, y))
                {
                    return node;
                }
            }
            return null;
        }

        private void Form2Closed(object? sender, EventArgs e) => form2closed = true;

        private void btnOpenForm2_Click(object? sender, EventArgs e)
        {
            form2 = new Form2();
            form2closed = false;
            form2.Closed += Form2Closed;
            LoadDataToForm2();

            // ���������� ������ �����
            form2.Show();
        }

        private void LoadDataToForm2()
        {
            // �������� ��������� ���� (����� �������� ����� ����� ���������)
            Node? startNode = selectedNodes.FirstOrDefault();

            if (startNode != null)
            {
                // ��������� ����� ���������� �����
                ShortestPathSolver solver = new ShortestPathSolver(tree);
                solver.FindShortestPaths(startNode);

                // �������� ������� ���������� � ��������
                var distances = solver.GetDistanceTable();
                var routes = solver.GetRoutes();

                // ������ ������ ����� � ������� ������
                if (form2 != null)
                    form2.LoadData(distances, routes);
            }
        }
    }
}
