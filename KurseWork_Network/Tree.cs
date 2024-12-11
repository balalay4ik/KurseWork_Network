using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurseWork_Network
{
    public class Tree
    {
        public List<Node> Nodes { get; private set; } = new List<Node>();
        public List<Edge> Edges { get; private set; } = new List<Edge>();


        public void AddNode(Node node)
        {
            Nodes.Add(node);
        }


        public void AddEdge(Node from, Node to, int weight, ChannelType type, int errorProbability = 4)
        {
            if (Nodes.Contains(from) && Nodes.Contains(to))
            {
                Edges.Add(new Edge(from, to, weight, type, errorProbability));
            }
        }


        public void RemoveEdge(Node from, Node to)
        {
            Edges.RemoveAll(e => e.From == from && e.To == to);
        }


        /// <summary>
        /// Малює графік на зображенні.
        /// </summary>
        /// <param name="g">Графічний контекст для малювання.</param>
        /// <remarks>
        /// Спочатку малює ребра, потім вузли.
        /// </remarks>
        public void Draw(Graphics g)
        {
            // Рисуем рёбра
            foreach (var edge in Edges)
            {
                edge.Draw(g);
            }

            // Рисуем узлы
            foreach (var node in Nodes)
            {
                node.Draw(g);
            }
        }

        public Edge GetEdge(Node node1, Node node2)
        {
            return Edges.FirstOrDefault(edge =>
                (edge.From == node1 && edge.To == node2) ||
                (edge.From == node2 && edge.To == node1));
        }

    }


    public class Edge
    {
        public Node From { get; private set; } // Вузол-джерело
        public Node To { get; private set; } // Вузол-призначення
        public int Weight { get; private set; } // Вага ребра
        public ChannelType Type { get; set; } // Тип каналу
        public double ErrorProbability { get; set; } // Ймовірність помилок (0-100%)

        public Edge(Node from, Node to, int weight, ChannelType type, int errorProbability = 4)
        {
            From = from;
            To = to;
            Weight = weight;
            Type = type;
            ErrorProbability = errorProbability;
        }


        /// <summary>
        /// Малює ребро на графіку
        /// </summary>
        /// <param name="g"> - об'єкт для малювання</param>
        /// <remarks>
        /// Лінія малюється з урахуванням її нахилу, а вага ребра малюється по центру лінії.
        /// </remarks>
        public void Draw(Graphics g)
        {
            // Розрахунок середньої точки лінії
            int midX = (From.X + To.X) / 2;
            int midY = (From.Y + To.Y) / 2;

            // Розрахунок кута нахилу лінії
            double angle = Math.Atan2(To.Y - From.Y, To.X - From.X) * (180 / Math.PI);

            // Перетворення кута для пріоритету нахилу вниз
            if (angle > 90)
                angle -= 180;
            else if (angle < -90)
                angle += 180;

            // Розмір тексту
            string weightText = Weight.ToString();
            using (Font font = new Font("Arial", 10))
            {
                SizeF textSize = g.MeasureString(weightText, font);

                // Розрахунок зміщення для лінії (половина довжини тексту)
                float textWidth = textSize.Width;
                float dx = (float)((To.X - From.X) / Math.Sqrt(Math.Pow(To.X - From.X, 2) + Math.Pow(To.Y - From.Y, 2)));
                float dy = (float)((To.Y - From.Y) / Math.Sqrt(Math.Pow(To.X - From.X, 2) + Math.Pow(To.Y - From.Y, 2)));

                float breakOffsetX = dx * (textWidth / 2 + 5); // Відступ для розриву
                float breakOffsetY = dy * (textWidth / 2 + 5);

                // Лінія до тексту
                g.DrawLine(Pens.Black, From.X, From.Y, midX - breakOffsetX, midY - breakOffsetY);

                // Лінія після тексту
                g.DrawLine(Pens.Black, midX + breakOffsetX, midY + breakOffsetY, To.X, To.Y);

                // Збереження стану графіки перед обертанням
                var state = g.Save();

                // Переміщення і обертання графіки для тексту
                g.TranslateTransform(midX, midY);
                g.RotateTransform((float)angle);

                // Малюємо вагу ребра
                g.DrawString(weightText, font, Brushes.Black, -textWidth / 2, -textSize.Height / 2);

                // Відновлення стану графіки
                g.Restore(state);
            }
        }

        public void UpdateWeight(int newWeight)
        {
            Weight = newWeight;
        }


    }

    public enum ChannelType
    {
        Duplex,      // Дуплексный
        HalfDuplex   // Полудуплексный
    }
}
