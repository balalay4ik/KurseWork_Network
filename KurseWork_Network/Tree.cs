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

        // Добавление узла
        public void AddNode(Node node)
        {
            Nodes.Add(node);
        }

        // Добавление ребра
        public void AddEdge(Node from, Node to, int weight, ChannelType type, int errorProbability = 4)
        {
            if (Nodes.Contains(from) && Nodes.Contains(to))
            {
                Edges.Add(new Edge(from, to, weight, type, errorProbability));
            }
        }

        // Удаление ребра
        public void RemoveEdge(Node from, Node to)
        {
            Edges.RemoveAll(e => e.From == from && e.To == to);
        }

        // Метод для рисования дерева
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
            // Предполагаем, что рёбра хранятся в списке Edges
            return Edges.FirstOrDefault(edge =>
                (edge.From == node1 && edge.To == node2) ||
                (edge.From == node2 && edge.To == node1)); // Для неориентированных графов
        }

    }

    // Класс рёбра (связи между узлами)
    public class Edge
    {
        public Node From { get; private set; } // Узел-источник
        public Node To { get; private set; } // Узел-назначение
        public int Weight { get; private set; } // Вес рёбра
        public ChannelType Type { get; set; } // Тип канала
        public double ErrorProbability { get; set; } // Вероятность ошибок (0-100%)

        public Edge(Node from, Node to, int weight, ChannelType type, int errorProbability = 4)
        {
            From = from;
            To = to;
            Weight = weight;
            Type = type;
            ErrorProbability = errorProbability; // Значение от 0 до 100
        }

        // Метод для рисования рёбра
        public void Draw(Graphics g)
        {
            // Расчёт средней точки линии
            int midX = (From.X + To.X) / 2;
            int midY = (From.Y + To.Y) / 2;

            // Расчёт угла наклона линии
            double angle = Math.Atan2(To.Y - From.Y, To.X - From.X) * (180 / Math.PI); // В градусах

            // Преобразование угла для приоритета наклона вниз
            if (angle > 90)
                angle -= 180;
            else if (angle < -90)
                angle += 180;

            // Размер текста
            string weightText = Weight.ToString();
            using (Font font = new Font("Arial", 10))
            {
                SizeF textSize = g.MeasureString(weightText, font);

                // Вычисление смещения для линии (половина длины текста)
                float textWidth = textSize.Width;
                float dx = (float)((To.X - From.X) / Math.Sqrt(Math.Pow(To.X - From.X, 2) + Math.Pow(To.Y - From.Y, 2)));
                float dy = (float)((To.Y - From.Y) / Math.Sqrt(Math.Pow(To.X - From.X, 2) + Math.Pow(To.Y - From.Y, 2)));

                float breakOffsetX = dx * (textWidth / 2 + 5); // Отступ для разрыва
                float breakOffsetY = dy * (textWidth / 2 + 5);

                // Линия до текста
                g.DrawLine(Pens.Black, From.X, From.Y, midX - breakOffsetX, midY - breakOffsetY);

                // Линия после текста
                g.DrawLine(Pens.Black, midX + breakOffsetX, midY + breakOffsetY, To.X, To.Y);

                // Сохранение состояния графики перед поворотом
                var state = g.Save();

                // Перемещение и поворот графики для текста
                g.TranslateTransform(midX, midY);
                g.RotateTransform((float)angle);

                // Рисуем вес рёбра
                g.DrawString(weightText, font, Brushes.Black, -textWidth / 2, -textSize.Height / 2);

                // Восстановление состояния графики
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
