using System.Windows.Forms;
using static System.ComponentModel.Design.ObjectSelectorEditor;

namespace KurseWork_Network
{
    public partial class Form1 : Form
    {
        public Tree tree = new Tree(); // Дерево графа
        public List<Node> selectedNodes = new List<Node>(); // Список обраних вузлів
        
        private NumericUpDown numericRegions = new(); // Поле для введення кількості регіонів
        private NumericUpDown numericCommunication = new(); // Поле для введення кількості зв'язків

        private Button genereateGraph = new(); // Кнопка для генерації графа
        private Button nodeRoutes = new(); // Кнопка для обчислення маршрутів
        private Button analyse = new(); // Кнопка для аналізу мережі

        private Form2? form2 = null; // Посилання на другу форму
        private Form3? form3 = null; // Посилання на третю форму
        private bool form2closed = true; // Прапор, що вказує, чи закрита друга форма

        private Point mouseStart; // Початкова позиція миші

        private bool isDragging = false; // Прапор, що вказує на режим перетягування

        private PictureBox pbCanvas; // Поле для PictureBox
        private bool isCanvasDragging = false; // Прапор переміщення канвасу
        private Point canvasOffset = new Point(0, 0); // Зміщення канвасу

        private float zoomScale = 1.0f; // Поточний масштаб
        private const float zoomStep = 0.1f; // Крок збільшення/зменшення масштабу
        private const float minZoom = 0.5f; // Мінімальний масштаб
        private const float maxZoom = 2.0f; // Максимальний масштаб
        private float moveSpeedFactor = 1.0f; // Фактор швидкості переміщення

        private ContextMenuStrip contextMenuNode; // Контекстне меню для вузлів
        private ContextMenuStrip contextMenuEdge; // Контекстне меню для ребер
        private ContextMenuStrip contextMenuCanvas; // Контекстне меню для канвасу

        Edge? clickedEdge = null; // Обране ребро



        public Form1()
        {
            InitializeComponent();

            // Створюємо PictureBox
            pbCanvas = new PictureBox
            {
                Name = "pbCanvas",
                Size = new Size(600, 560),
                Location = new Point(1, 0),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            numericRegions.Location = new Point(605, 30);
            numericRegions.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            numericRegions.Value = 3;
            numericRegions.Maximum = 100000;
            numericCommunication.Location = new Point(605, 80);
            numericCommunication.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            numericCommunication.Value = 9;

            genereateGraph.Location = new Point(605, 120);
            genereateGraph.Size = new Size(120, 25);
            genereateGraph.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            genereateGraph.Click += btnGenerate_Click;
            genereateGraph.Text = "Згенерувати граф";

            nodeRoutes.Location = new Point(605, 150);
            nodeRoutes.Size = new Size(120, 25);
            nodeRoutes.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            nodeRoutes.Click += btnOpenForm2_Click;
            nodeRoutes.Text = "Маршрути вузлів";

            analyse.Location = new Point(605, 180);
            analyse.Size = new Size(120, 25);
            analyse.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            analyse.Click += btnOpenForm3_Click;
            analyse.Text = "Аналіз мережі";

            Controls.Add(pbCanvas);
            Controls.Add(numericRegions);
            Controls.Add(numericCommunication);
            Controls.Add(genereateGraph);
            Controls.Add(nodeRoutes);
            Controls.Add(analyse);

            // Підключаємо обробники подій миші
            pbCanvas.Paint += PbCanvas_Paint;
            pbCanvas.MouseDown += PbCanvas_MouseDown;
            pbCanvas.MouseMove += PbCanvas_MouseMove;
            pbCanvas.MouseUp += PbCanvas_MouseUp;
            pbCanvas.MouseWheel += PbCanvas_MouseWheel;

            // Меню для вузла
            contextMenuNode = new ContextMenuStrip();
            contextMenuNode.Items.Add("Видалити вузол", null, (s, e) => DeleteSelectedNodes());
            contextMenuNode.Items.Add("Додати зв'язок", null, (s, e) => AddConnectionsToSelectedNodes());

            // Меню для ребер
            contextMenuEdge = new ContextMenuStrip();
            contextMenuEdge.Items.Add("Видалити ребро", null, (s, e) => DeleteSelectedEdge(clickedEdge));
            contextMenuEdge.Items.Add("Змінити вагу", null, (s, e) => OpenEdgeWeightEditor());

            // Меню для пустого простору
            contextMenuCanvas = new ContextMenuStrip();
            contextMenuCanvas.Items.Add("Додати вузол", null, (s, e) => AddNodeAtMousePosition());

            // Створення підписів для полів введення
            Label labelRegions = new Label
            {
                Text = "Кількість регіонів:",
                Location = new Point(605, 12),
                AutoSize = true
            };

            Label labelCommunication = new Label
            {
                Text = "Число зв'язків на регіон:",
                Location = new Point(605, 60), 
                AutoSize = true
            };

            Controls.Add(labelRegions);
            Controls.Add(labelCommunication);



            GenerateRegionsAndCommunications((int)numericRegions.Value, (int)numericCommunication.Value);

        }


        /// <summary>
        /// Обробник натискання кнопки миші на канвасі
        /// </summary>
        /// <param name="sender">Джерело події</param>
        /// <param name="e">Параметри події</param>
        ///
        private void PbCanvas_MouseDown(object? sender, MouseEventArgs e)
        {
            // Встановлюємо початкову точку для переміщення
            mouseStart = new Point(e.X, e.Y);

            if (e.Button == MouseButtons.Middle)
            {
                // Початок переміщення канвасу
                mouseStart = e.Location;
                isCanvasDragging = true;
                return;
            }

            if (e.Button == MouseButtons.Left)
            {
                Node? clickedNode = FindNodeAt(e.X, e.Y);



                if (ModifierKeys.HasFlag(Keys.Control)) // Видалення з Ctrl
                {
                    if (clickedNode != null)
                    {
                        if (selectedNodes.Contains(clickedNode))
                        {
                            selectedNodes.Remove(clickedNode); // Зняти виділення
                        }
                        else
                        {
                            selectedNodes.Add(clickedNode); // Додати до виділених
                        }
                    }
                }
                else
                {
                    if (clickedNode != null)
                    {
                        if (!selectedNodes.Contains(clickedNode))
                        {
                            selectedNodes.Clear(); // Зняти виділення з усіх вузлів
                            selectedNodes.Add(clickedNode); // Виділити тільки поточний
                        }
                    }
                    else
                    {
                        selectedNodes.Clear(); // Очистити виділених вузлів
                    }
                }

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
                

                pbCanvas.Invalidate(); // Перерисувати
            }

            if (e.Button == MouseButtons.Right)
            {
                Node? clickedNode = FindNodeAt(e.X, e.Y);
                clickedEdge = FindEdgeAt(e.X, e.Y);

                if (clickedNode != null)
                {
                    // Показати меню для вузлів
                    contextMenuNode.Show(pbCanvas, e.Location);
                }
                else if (clickedEdge != null)
                {
                    // Показати меню для ребер
                    contextMenuEdge.Show(pbCanvas, e.Location);
                }
                else
                {
                    // Показати меню для порожнього простору
                    contextMenuCanvas.Show(pbCanvas, e.Location);
                }
            }
        }


        /// <summary>
        /// Обробник руху миші по канвасу
        /// </summary>
        /// <param name="sender">Джерело події</param>
        /// <param name="e">Параметри події</param>
        /// <remarks>
        /// Якщо миша натиснута, то переміщаємо виділений вузол.
        /// Якщо натиснута середня кнопка, то переміщаємо сам канвас.
        private void PbCanvas_MouseMove(object? sender, MouseEventArgs e)
        {

            if (isCanvasDragging)
            {
                // Рахуємо зсув
                int deltaX = (int)((e.X - mouseStart.X) / zoomScale);
                int deltaY = (int)((e.Y - mouseStart.Y) / zoomScale);

                canvasOffset.X += deltaX;
                canvasOffset.Y += deltaY;

                mouseStart = e.Location; // Оновлюємо початкову позицію
                pbCanvas.Invalidate(); // Перерисовуємо
                return;
            }

            if (isDragging && selectedNodes.Count > 0 && e.Button == MouseButtons.Left)
            {
                int deltaX = e.X - mouseStart.X; // Зсув по X
                int deltaY = e.Y - mouseStart.Y; // Зсув по Y


                foreach (var node in selectedNodes)
                {
                    node.X += (int)(deltaX / zoomScale);
                    node.Y += (int)(deltaY / zoomScale);
                }

                // Оновлюємо початкову позицію миші
                mouseStart = new Point(e.X, e.Y);

                pbCanvas.Invalidate(); // Перерисовуємо
            }

        }


        /// <summary>
        /// Обробник відпускання миші над канвасом
        /// </summary>
        /// <param name="sender">Джерело події</param>
        /// <param name="e">Параметри події</param>
        /// <remarks>
        /// Якщо відпущена середня кнопка, то завершуємо переміщення канваса.
        /// Якщо відпущена ліве кнопка, то завершуємо переміщення виділених вузлів.
        /// </remarks>
        private void PbCanvas_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                isCanvasDragging = false; // Завершуємо переміщення канваса
            }
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false; // Завершуємо переміщення
            }
        }


        /// <summary>
        /// Обробник колеса миші на канвасі
        /// </summary>
        /// <param name="sender">Джерело події</param>
        /// <param name="e">Параметри події</param>
        /// <remarks>
        /// Масштабує канвас на місці, де знаходиться курсор.
        /// </remarks>
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

            canvasOffset.X = (int)((canvasOffset.X - cursorPosition.X) * scaleChange + cursorPosition.X);
            canvasOffset.Y = (int)((canvasOffset.Y - cursorPosition.Y) * scaleChange + cursorPosition.Y);

            pbCanvas.Invalidate();
        }



        /// <summary>
        /// Обробник події малювання канвасу
        /// </summary>
        /// <param name="sender">Джерело події</param>
        /// <param name="e">Параметри події</param>
        /// <remarks>
        /// Малює зв'язки дерева, та підкреслює виділені вузли.
        /// </remarks>
        private void PbCanvas_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // Установлюємо масштабування
            g.ScaleTransform(zoomScale, zoomScale);

            // Враховujemy зміщення канвасу
            g.TranslateTransform(canvasOffset.X, canvasOffset.Y);

            // Малюємо зв'язки дерева
            tree.Draw(g);

            // Підсвічуємо виділені вузли
            foreach (var node in selectedNodes)
            {
                DrawNodeHighlight(g, node);
            }
        }

        /// <summary>
        /// Малює підсвітку на виділених вузлах.
        /// </summary>
        /// <param name="g">Графічний контекст для малювання.</param>
        /// <param name="node">Вузол, на якому малюємо підсвітку.</param>
        /// <remarks>
        /// Малює еліпс червоного кольору навколо вузла.
        /// </remarks>
        private void DrawNodeHighlight(Graphics g, Node node)
        {
            int highlightRadius = 20; // Радиус подсветки
            g.DrawEllipse(Pens.Red, node.X - highlightRadius, node.Y - highlightRadius, highlightRadius * 2, highlightRadius * 2);
        }


        /// <summary>
        /// Генерація дерева з заданою кількістю регіонів та зв'язків між ними.
        /// </summary>
        /// <param name="regionCount">Кількість регіонів</param>
        /// <param name="connectionsPerRegion">Кількість зв'язків, які з'єднують кожен регіон</param>
        /// <summary>
        private void GenerateRegionsAndCommunications(int regionCount, int connectionsPerRegion)
        {
            RegionGenerator generator = new RegionGenerator();
            Tree generatedTree = generator.Generate(regionCount, connectionsPerRegion, 50);

            // Установлюємо сгенероване дерево як поточне
            tree = generatedTree;

            // Оновлюємо відображення
            pbCanvas.Invalidate();
        }

        private void btnGenerate_Click(object? sender, EventArgs e)
        {
            int regionCount = (int)numericRegions.Value;
            int connectionsPerRegion = (int)numericCommunication.Value;

            GenerateRegionsAndCommunications(regionCount, connectionsPerRegion);
        }


        /// <summary>
        /// Знаходить вузол, найближчий до заданих екранних координат.
        /// </summary>
        /// <param name="screenX">координата X</param>
        /// <param name="screenY">координата Y</param>
        /// <returns>Знайдений вузол, або null, якщо вузол не знайдено</returns>
        /// <remarks>
        /// Метод перетворює екранні координати в графові, 
        /// знаходить найближчий вузол до заданих координат, 
        /// і повертає його, якщо відстань до нього менше радіусу захоплення вузла.
        /// </remarks>
        private Node? FindNodeAt(float screenX, float screenY)
        {
            // Перетворюємо екранні координати в графові
            PointF graphPosition = ScreenToGraph(screenX, screenY);

            // Радіус області захоплення вузла
            float hitRadius = tree.Nodes.First().Radius * zoomScale; // Розмір вузла

            // Шукаємо найближчий вузол
            foreach (var node in tree.Nodes)
            {
                float dx = node.X - graphPosition.X;
                float dy = node.Y - graphPosition.Y;
                float distanceSquared = dx * dx + dy * dy;

                // Якщо відстань до вузла менше радіусу захоплення, вузол знайдено
                if (distanceSquared <= hitRadius * hitRadius)
                {
                    return node;
                }
            }

            return null; // Вузол не знайдено
        }

        /// <summary>
        /// Знаходить ребро, найближче до заданих екранних координат.
        /// </summary>
        /// <param name="x">координата X</param>
        /// <param name="y">координата Y</param>
        /// <returns>Знайдене ребро, або null, якщо ребро не знайдено</returns>
        /// <remarks>
        /// Метод перетворює екранні координати в графові, 
        /// знаходить найближче ребро до заданих координат, 
        /// і повертає його, якщо відстань до нього менше допустимої відстані.
        /// </remarks>
        private Edge? FindEdgeAt(float x, float y)
        {
            PointF graphPosition = ScreenToGraph(x, y);
            foreach (var edge in tree.Edges)
            {
                // Перевірка потрапляння в область ребра
                if (IsPointNearEdge(graphPosition, edge))
                {
                    return edge;
                }
            }
            return null;
        }

        /// <summary>
        /// Перевірка, чи знаходиться точка near до ребра.
        /// </summary>
        /// <param name="point">точка для перевірки</param>
        /// <param name="edge">ребро для перевірки</param>
        /// <returns>true, якщо точка near до ребра, false в іншому випадку</returns>
        /// <remarks>
        /// Метод використовує рівняння лінії для обчислення відстані від точки до лінії,
        /// і перевіряє, чи ця відстань менше допустимої відстані.
        /// </remarks>
        private bool IsPointNearEdge(PointF point, Edge edge)
        {
            const float tolerance = 5.0f; // Допустима відстань до ребра

            // Рівняння лінії (відстань від точки до лінії)
            float distance = Math.Abs((edge.To.Y - edge.From.Y) * point.X -
                                      (edge.To.X - edge.From.X) * point.Y +
                                      edge.To.X * edge.From.Y - edge.To.Y * edge.From.X) /
                             (float)Math.Sqrt(Math.Pow(edge.To.Y - edge.From.Y, 2) + Math.Pow(edge.To.X - edge.From.X, 2));

            // Перевірка відстані
            return distance <= tolerance &&
                   IsPointWithinEdgeBounds(point, edge);
        }

        /// <summary>
        /// Перевірка, чи знаходиться точка point всередині кордонів ребра edge.
        /// </summary>
        /// <param name="point">точка для перевірки</param>
        /// <param name="edge">ребро для перевірки</param>
        /// <returns>true, якщо точка лежить всередині кордонів ребра, false в іншому випадку</returns>
        /// <remarks>
        /// Метод перевіряє, чи координати точки point лежать всередині прямокутника, який утворюється двома точками ребра edge.
        /// </remarks>
        private bool IsPointWithinEdgeBounds(PointF point, Edge edge)
        {
            float minX = Math.Min(edge.From.X, edge.To.X);
            float maxX = Math.Max(edge.From.X, edge.To.X);
            float minY = Math.Min(edge.From.Y, edge.To.Y);
            float maxY = Math.Max(edge.From.Y, edge.To.Y);

            return point.X >= minX && point.X <= maxX &&
                   point.Y >= minY && point.Y <= maxY;
        }


        /// <summary>
        /// Видаляє ребро з графа, якщо воно було виділене.
        /// </summary>
        /// <param name="edge">Виділене ребро</param>
        /// <remarks>
        /// Метод видаляє ребро з графа, якщо воно було виділене.
        /// </remarks>
        private void DeleteSelectedEdge(Edge edge)
        {
            if (edge != null)
            {
                tree.Edges.Remove(edge);
                pbCanvas.Invalidate();
            }
        }


        /// <summary>
        /// Перетворює координати екрана в координати графа.
        /// </summary>
        /// <param name="x">координата X на екрані</param>
        /// <param name="y">координата Y на екрані</param>
        /// <returns>координати X та Y на графі</returns>
        /// <remarks>
        /// Метод використовує масштабування та зміщення канвасу, щоб
        /// перетворити координати екрана в координати графа.
        /// </remarks>
        private PointF ScreenToGraph(float x, float y)
        {
            float graphX = (x / zoomScale - canvasOffset.X) ;
            float graphY = (y / zoomScale - canvasOffset.Y) ;
            return new PointF(graphX, graphY);
        }


        /// <summary>
        /// Перетворює координати графа в координати екрана.
        /// </summary>
        /// <param name="x">координата X на графі</param>
        /// <param name="y">координата Y на графі</param>
        /// <returns>координати X та Y на екрані</returns>
        /// <remarks>
        /// Метод використовує масштабування та зміщення канвасу, щоб
        /// перетворити координати графа в координати екрана.
        /// </remarks>
        private PointF GraphToScreen(float x, float y)
        {
            float screenX = (x + canvasOffset.X) * zoomScale;
            float screenY = (y + canvasOffset.Y) * zoomScale;
            return new PointF(screenX, screenY);
        }


        private void Form2Closed(object? sender, EventArgs e) => form2closed = true;

        /// <summary>
        /// Обробник натискання кнопки "Маршрути вузлів".
        /// </summary>
        /// <param name="sender">Джерело події</param>
        /// <param name="e">Параметри події</param>
        /// <remarks>
        /// Створює новий екземпляр Form2, реєструє подію закриття,
        /// завантажує дані до Form2, і відображає його.
        /// </remarks>
        private void btnOpenForm2_Click(object? sender, EventArgs e)
        {
            form2 = new Form2();
            form2closed = false;
            form2.Closed += Form2Closed;
            LoadDataToForm2();

            form2.Show();
        }

        /// <summary>
        /// Обробник натискання кнопки "Аналіз мережі".
        /// </summary>
        /// <param name="sender">Джерело події</param>
        /// <param name="e">Параметри події</param>
        /// <remarks>
        /// Створює новий екземпляр Form3, відображає його.
        /// </remarks>
        private void btnOpenForm3_Click(object? sender, EventArgs e)
        {
            form3 = new Form3(this);
            form3.Show();
        }

        /// <summary>
        /// Завантажує дані до Form2.
        /// </summary>
        /// <remarks>
        /// Обирає початковий вузол (можна покращити вибір через інтерфейс),
        /// виконує пошук найкоротших шляхів, отримує таблицю відстаней та маршрути,
        /// і завантажує дані до другої форми.
        /// </remarks>
        private void LoadDataToForm2()
        {
            // Вибираємо початковий вузол (можна покращити вибір через інтерфейс)
            Node? startNode = selectedNodes.FirstOrDefault();

            if (startNode != null)
            {
                // Виконуємо пошук найкоротших шляхів
                ShortestPathSolver solver = new ShortestPathSolver(tree);
                solver.FindShortestPaths(startNode);

                // Отримуємо таблицю відстаней та маршрути
                var distances = solver.GetDistanceTable();
                var routes = solver.GetRoutes();

                // Створюємо другу форму і передаємо дані
                if (form2 != null)
                {
                    form2.SetForm1(this);
                    form2.LoadData(distances, routes);
                }
            }
        }

        /// <summary>
        /// Видаляє виділені вузли з графа.
        /// </summary>
        /// <remarks>
        /// Видаляє виділені вузли з списку вузлів дерева,
        /// видаляє ребра, які пов'язані з цими вузлами,
        /// очищує список виділених вузлів,
        /// і перерисовує канвас.
        /// </remarks>
        private void DeleteSelectedNodes()
        {
            foreach (var node in selectedNodes)
            {
                tree.Nodes.Remove(node);
                tree.Edges.RemoveAll(e => e.From == node || e.To == node);
            }
            selectedNodes.Clear();
            pbCanvas.Invalidate();
        }

        /// <summary>
        /// Додайте зв'язки між виділеними вузлами.
        /// </summary>
        /// <remarks>
        /// Якщо є хоча б два виділені вузли, то додає зв'язки між усіма парами вузлів.
        /// Вага зв'язків за замовчуванням - 1, тип зв'язку - duplex.
        /// </remarks>
        private void AddConnectionsToSelectedNodes()
        {
            if (selectedNodes.Count > 1)
            {
                for (int i = 0; i < selectedNodes.Count - 1; i++)
                {
                    for (int j = i + 1; j < selectedNodes.Count; j++)
                    {
                        tree.AddEdge(selectedNodes[i], selectedNodes[j], 1, ChannelType.Duplex); // Вес и тип можно настроить
                    }
                }
                pbCanvas.Invalidate();
            }
        }

        /// <summary>
        /// Додає новий вузол на місці, де була натиснута миша.
        /// </summary>
        /// <remarks>
        /// Метод перетворює координати миші в графові,
        /// і додає новий вузол з номером, що збільшується на 1,
        /// на позицію, що відповідає координатам миші.
        /// </remarks>
        private void AddNodeAtMousePosition()
        {
            PointF graphPosition = ScreenToGraph(mouseStart.X, mouseStart.Y);
            Node newNode = new Node($"Node{tree.Nodes.Count + 1}", (int)graphPosition.X, (int)graphPosition.Y);
            tree.AddNode(newNode);
            pbCanvas.Invalidate();
        }

        /// <summary>
        /// Відкриває редактор ваги ребра, якщо миша була натиснута на ребро.
        /// </summary>
        /// <remarks>
        /// Метод знаходить ребро за координатами миші,
        /// і відкриває форму з редагуванням ваги ребра.
        /// </remarks>
        private void OpenEdgeWeightEditor()
        {
            Edge? edge = FindEdgeAt(mouseStart.X, mouseStart.Y);
            if (edge != null)
            {
                using (Form weightForm = new Form())
                {
                    weightForm.Text = "Змінити вагу ребра";
                    weightForm.Size = new Size(300, 150);

                    NumericUpDown weightInput = new NumericUpDown
                    {
                        Minimum = 1,
                        Maximum = 100,
                        Value = edge.Weight,
                        Location = new Point(50, 30)
                    };

                    Button applyButton = new Button
                    {
                        Text = "Застосувати",
                        Location = new Point(50, 70),
                        AutoSize = true
                    };
                    applyButton.Click += (s, e) =>
                    {
                        edge.UpdateWeight((int)weightInput.Value);
                        weightForm.Close();
                        pbCanvas.Invalidate();
                    };

                    weightForm.Controls.Add(weightInput);
                    weightForm.Controls.Add(applyButton);
                    weightForm.ShowDialog();
                }
            }
        }

    }
}


