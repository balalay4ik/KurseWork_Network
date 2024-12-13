using KurseWork_Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class NetworkAnalyzer
{
    public static Dictionary<string, int> ProtocolsInfoSize = new Dictionary<string, int>
        {
            { "TCP", 40 },
            { "UDP", 28 }
        };

    private Tree network;

    public NetworkAnalyzer(Tree network)
    {
        this.network = network;
    }

        /// <summary>
        /// Метод для аналізу трафіку в мережі з врахуванням помилок.
        /// </summary>
        /// <param name="startNode">Початковий вузол.</param>
        /// <param name="endNode">Кінцевий вузол.</param>
        /// <param name="isFirstConnection"> TRUE, якщо це перший зв'язок. </param>
        /// <returns>
        /// Пару з часом (у мілісекундах) та статусом (Ok або Error).
        /// </returns>
        /// <remarks>
        /// Метод аналізує трафік від початкового вузлу до кінцевого,
        /// враховуючи помилки на маршруті. Якщо помилка є, то повертається
        /// час до помилки та статус Error. Якщо помилки немає, то повертається
        /// загальний час на повний маршрут та статус Ok.
        /// </remarks>
    public (double totalTime, Status status) AnalyzeTraffic(Node startNode, Node endNode, bool isFirstConnection = false)
    {
        double totalTime = 0;

        var route = GetRoute(startNode, endNode);

        // Час на встановлення з'єднання (враховується тільки для першого з'єднання)
        if (isFirstConnection)
        {
            totalTime += route.Sum(edge => edge.Weight) * 2; // Час для 2 пакетів на встановлення з'єднання
        }

        // Перевірка на помилки
        Edge errorEdge = CalculateRouteErrorEdge(route);

        if (errorEdge == null)
        {
            // Якщо помилки немає, додаємо час на повний маршрут
            totalTime += route.Sum(edge => edge.Weight);
            return (totalTime, Status.Ok);  
        }

        // Якщо помилка є, рахуємо час до ребра з помилкою
        foreach (var edge in route)
        {
            totalTime += edge.Weight;

            if (edge == errorEdge)
            {
                return (totalTime, Status.Error); // Повертаємо час до помилки та статус
            }
        }

        return (totalTime, Status.Error);
    }

        /// <summary>
        /// Отримуємо кратчайший маршрут між startNode та endNode.
        /// </summary>
        /// <param name="startNode">Початковий вузол</param>
        /// <param name="endNode">Кінцевий вузол</param>
        /// <returns>Список ребер - кратчайший маршрут між startNode та endNode</returns>
        /// <exception cref="Exception">Маршрут не знайдено!</exception>
        /// <exception cref="Exception">Ребро между вузлами {path[i].Id} и {path[i + 1].Id} не знайдено!</exception>
    private List<Edge> GetRoute(Node startNode, Node endNode)
    {
        // Створimo екземпляр алгоритму Дейкстри
        ShortestPathSolver shortestPathSolver = new ShortestPathSolver(network);

        // Виконуємо пошук кратчайших шляхiв о startNode
        shortestPathSolver.FindShortestPaths(startNode);

        // Отримуємо шлях як список вузлів
        var path = shortestPathSolver.GetPathTo(startNode, endNode);

        if (path == null || path.Count < 2)
        {
            MessageBox.Show("Маршрут не знайдено!");
            return new List<Edge>();
        }

        // Перетворюємо шлях (список вузлів) у список ребер
        List<Edge> route = new List<Edge>();
        for (int i = 0; i < path.Count - 1; i++)
        {
            var edge = network.GetEdge(path[i], path[i + 1]); // Предполагаем, что Tree имеет метод GetEdge
            if (edge != null)
            {
                route.Add(edge);
            }
            else
            {
                MessageBox.Show($"Ребро між вузлами {path[i].Id} и {path[i + 1].Id} не знайдено!");
                return new List<Edge>();
            }
        }

        return route;
    }


    /// <summary>
    /// Обчислює рiвiнь помилки на маршруті.
    /// </summary>
    /// <param name="route">Список рiвiв, що представляє маршрут.</param>
    /// <returns>Повертає рiвo, де сталася помилка, або null, якщо помилки не було.</returns>
    /// <remarks>
    /// Метод генерує випадкове число для кожного ребра в маршруті та перевіряє, 
    /// чи трапилася помилка на цьому ребрі відповідно до заданої ймовірності помилки.
    /// </remarks>
    private Edge CalculateRouteErrorEdge(List<Edge> route)
    {
        Random random = new Random();

        foreach (var edge in route)
        {
            //  пор в неймання помилки на р брі
            if (random.NextDouble() < edge.ErrorProbability / 100.0)
            {
                return edge; //  повернення р бра, де сталас  помилка
            }
        }

        return null; // Якщо помилки не сталас 
    }

}

public enum Status
{
    Ok,
    Error
}

