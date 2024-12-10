using System.Windows.Forms;
using static System.ComponentModel.Design.ObjectSelectorEditor;

namespace KurseWork_Network
{
    public partial class Form1 : Form
    {
        public Tree tree = new Tree(); // ������
        public List<Node> selectedNodes = new List<Node>(); // ������ ���������� �����
        
        private NumericUpDown numericRegions = new();
        private NumericUpDown numericCommunication = new();

        private Button genereateGraph = new();
        private Button nodeRoutes = new();
        private Button analyse = new();

        private Form2? form2 = null; // ������ �� ������ �����
        private Form3? form3 = null;
        private bool form2closed = true;

        private Point mouseStart; // ��������� ������� ����

        private bool isDragging = false; // ����, ����������� �� ����� �����������

        private PictureBox pbCanvas; // ���� ��� PictureBox
        private bool isCanvasDragging = false; // ���� ����������� �������
        private Point canvasOffset = new Point(0, 0); // �������� �������

        private float zoomScale = 1.0f; // ������� �������
        private const float zoomStep = 0.1f; // ��� ����������/���������� ��������
        private const float minZoom = 0.5f; // ����������� �������
        private const float maxZoom = 2.0f; // ������������ �������
        private float moveSpeedFactor = 1.0f; // ������ �������� �����������



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

            numericRegions.Location = new Point(605, 10);
            numericRegions.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            numericRegions.Value = 3;
            numericCommunication.Location = new Point(605, 40);
            numericCommunication.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            numericCommunication.Value = 9;

            genereateGraph.Location = new Point(605, 70);
            genereateGraph.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            genereateGraph.Click += btnGenerate_Click;
            genereateGraph.Text = "����������� ����";

            nodeRoutes.Location = new Point(605, 110);
            nodeRoutes.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            nodeRoutes.Click += btnOpenForm2_Click;
            nodeRoutes.Text = "�������� �����";

            analyse.Location = new Point(605, 150);
            analyse.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            analyse.Click += btnOpenForm3_Click;
            analyse.Text = "����� �����";

            Controls.Add(pbCanvas);
            Controls.Add(numericRegions);
            Controls.Add(numericCommunication);
            Controls.Add(genereateGraph);
            Controls.Add(nodeRoutes);
            Controls.Add(analyse);

            // ���������� ����������� ������� ����
            pbCanvas.Paint += PbCanvas_Paint;
            pbCanvas.MouseDown += PbCanvas_MouseDown;
            pbCanvas.MouseMove += PbCanvas_MouseMove;
            pbCanvas.MouseUp += PbCanvas_MouseUp;
            pbCanvas.MouseWheel += PbCanvas_MouseWheel;


            GenerateRegionsAndCommunications((int)numericRegions.Value, (int)numericCommunication.Value);

        }

        // ���������� ������� ����
        private void PbCanvas_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                // ������ ����������� �������
                mouseStart = e.Location;
                isCanvasDragging = true;
                return;
            }

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

                if(form3 != null)
                {
                    if (selectedNodes.Count > 1)
                        form3.UpdateNodeLabels(selectedNodes.First(), selectedNodes.Last());
                    else
                        form3.UpdateNodeLabels(null, null);
                }
                

                pbCanvas.Invalidate(); // ������������
            }
        }

        // ���������� ����������� ����
        private void PbCanvas_MouseMove(object? sender, MouseEventArgs e)
        {
            if (isCanvasDragging)
            {
                // ������������ ��������
                int deltaX = (int)((e.X - mouseStart.X) / zoomScale);
                int deltaY = (int)((e.Y - mouseStart.Y) / zoomScale);

                canvasOffset.X += deltaX;
                canvasOffset.Y += deltaY;

                mouseStart = e.Location; // ��������� ��������� �������
                pbCanvas.Invalidate(); // ��������������
                return;
            }

            if (isDragging && selectedNodes.Count > 0 && e.Button == MouseButtons.Left)
            {
                int deltaX = e.X - mouseStart.X; // �������� �� X
                int deltaY = e.Y - mouseStart.Y; // �������� �� Y


                foreach (var node in selectedNodes)
                {
                    node.X += (int)(deltaX / zoomScale);
                    node.Y += (int)(deltaY / zoomScale);
                }

                // ��������� ��������� ������� ����
                mouseStart = new Point(e.X, e.Y);

                pbCanvas.Invalidate(); // ������������
            }
        }

        // ���������� ���������� ����
        private void PbCanvas_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                isCanvasDragging = false; // ��������� ����������� �������
            }
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false; // ��������� �����������
            }
        }

        private void PbCanvas_MouseWheel(object? sender, MouseEventArgs e)
        {
            Point cursorPosition = e.Location;

            float previousScale = zoomScale;

            if (e.Delta > 0)
            {
                zoomScale = Math.Min(zoomScale + zoomStep, maxZoom);
            }
            else if (e.Delta < 0)
            {
                zoomScale = Math.Max(zoomScale - zoomStep, minZoom);
            }

            float scaleChange = zoomScale / previousScale;
            moveSpeedFactor = 1 / zoomScale;

            //canvasOffset.X = (int)((canvasOffset.X - cursorPosition.X) * scaleChange + cursorPosition.X);
            //canvasOffset.Y = (int)((canvasOffset.Y - cursorPosition.Y) * scaleChange + cursorPosition.Y);

            pbCanvas.Invalidate(); // �������������� ������
        }



        // ��������� ������
        private void PbCanvas_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // ������������� ���������������
            g.ScaleTransform(zoomScale, zoomScale);

            // ��������� �������� �������
            g.TranslateTransform(canvasOffset.X, canvasOffset.Y);

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
            int regionCount = (int)numericRegions.Value;
            int connectionsPerRegion = (int)numericCommunication.Value;

            GenerateRegionsAndCommunications(regionCount, connectionsPerRegion);
        }

        // ����� ���� ��� ���������� ����
        private Node? FindNodeAt(float screenX, float screenY)
        {
            // ����������� �������� ���������� � ��������
            PointF graphPosition = ScreenToGraph(screenX, screenY);

            // ������ ������� ������� ����
            float hitRadius = tree.Nodes.First().Radius * zoomScale; // ������ ����

            // ���� ��������� ����
            foreach (var node in tree.Nodes)
            {
                float dx = node.X - graphPosition.X;
                float dy = node.Y - graphPosition.Y;
                float distanceSquared = dx * dx + dy * dy;

                // ���� ���������� �� ���� ������ ������� �������, ���� ������
                if (distanceSquared <= hitRadius * hitRadius)
                {
                    return node;
                }
            }

            return null; // ���� �� ������
        }



        private PointF ScreenToGraph(float x, float y)
        {
            float graphX = (x / zoomScale - canvasOffset.X) ;
            float graphY = (y / zoomScale - canvasOffset.Y) ;
            return new PointF(graphX, graphY);
        }


        private PointF GraphToScreen(float x, float y)
        {
            float screenX = (x + canvasOffset.X) * zoomScale;
            float screenY = (y + canvasOffset.Y) * zoomScale;
            return new PointF(screenX, screenY);
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

        private void btnOpenForm3_Click(object? sender, EventArgs e)
        {
            form3 = new Form3(this);

            // ���������� ������ �����
            form3.Show();
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
                {
                    form2.SetForm1(this);
                    form2.LoadData(distances, routes);
                }
            }
        }
    }
}
