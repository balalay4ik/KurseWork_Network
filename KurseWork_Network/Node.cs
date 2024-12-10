using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurseWork_Network
{
    public class Node
    {
        public string Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public readonly int Radius = 15;

        public Node(string id, int x, int y)
        {
            Id = id;
            X = x;
            Y = y;
        }

        public void Draw(Graphics g)
        {
            // Рисуем круг для узла
            g.FillEllipse(Brushes.LightBlue, X - Radius, Y - Radius, Radius * 2, Radius * 2);

            // Рисуем текст внутри узла (идентификатор)
            g.DrawString(Id, new Font("Arial", 10), Brushes.Black, X - 12, Y - 10);
        }

        public bool IsPointInside(int x, int y)
        {
            int dx = X - x;
            int dy = Y - y;
            return Math.Sqrt(dx * dx + dy * dy) <= Radius;
        }
    }
}
