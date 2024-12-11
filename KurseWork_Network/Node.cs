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

        /// <summary>
        /// Малює узел на графіку
        /// </summary>
        /// <param name="g"> - об'єкт для малювання</param>
        public void Draw(Graphics g)
        {
            //   круг для вузла
            g.FillEllipse(Brushes.LightBlue, X - Radius, Y - Radius, Radius * 2, Radius * 2);

            //   текст усередині вузла (ідентифікатор)
            g.DrawString(Id, new Font("Arial", 10), Brushes.Black, X - 12, Y - 10);
        }
    }
}
