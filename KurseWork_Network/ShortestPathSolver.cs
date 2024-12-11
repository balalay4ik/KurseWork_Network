using KurseWork_Network;
using System;
using System.Collections.Generic;

public class ShortestPathSolver
{
    private Tree graph;
    private Dictionary<Node, int> distances; // Відстані від початкового вузла
    private Dictionary<Node, Node> previous; // Попередній вузол у шляху
    private Dictionary<Node, int> transitCount; // Кількість транзитних ребер

    public ShortestPathSolver(Tree graph)
    {
        this.graph = graph;
    }

        /// <summary>
        /// Метод пошуку найкоротших шляхів до всіх вузлів графа з заданого початкового вузла.
        /// </summary>
        /// <remarks>
        /// Метод використовує алгоритм Дейкстри з чергою та пріоритетом.
        /// </remarks>
        /// <param name="startNode">Початковий вузол</param>
    public void FindShortestPaths(Node startNode)
    {
        distances = new Dictionary<Node, int>();
        previous = new Dictionary<Node, Node>();
        transitCount = new Dictionary<Node, int>();
        HashSet<Node> visited = new HashSet<Node>();

        // Ініціалізація відстаней
        foreach (var node in graph.Nodes)
        {
            distances[node] = int.MaxValue;
            previous[node] = null;
            transitCount[node] = int.MaxValue;
        }

        distances[startNode] = 0;
        transitCount[startNode] = 0;

        // Черга з пріоритетом
        PriorityQueue<Node, (int, int)> queue = new PriorityQueue<Node, (int, int)>();
        queue.Enqueue(startNode, (0, 0));

        while (queue.Count > 0)
        {
            Node currentNode = queue.Dequeue();
            if (visited.Contains(currentNode))
                continue;

            visited.Add(currentNode);

            foreach (var edge in graph.Edges)
            {
                if (edge.From == currentNode || edge.To == currentNode)
                {
                    Node neighbor = edge.From == currentNode ? edge.To : edge.From;

                    if (visited.Contains(neighbor))
                        continue;

                    int newDistance = distances[currentNode] + edge.Weight;
                    int newTransitCount = transitCount[currentNode] + 1;

                    if (newDistance < distances[neighbor] ||
                        (newDistance == distances[neighbor] && newTransitCount < transitCount[neighbor]))
                    {
                        distances[neighbor] = newDistance;
                        previous[neighbor] = currentNode;
                        transitCount[neighbor] = newTransitCount;

                        queue.Enqueue(neighbor, (newDistance, newTransitCount));
                    }
                }
            }
        }
    }


        /// <summary>
        /// Отримуємо таблицю відстаней до всіх вузлів від початкового вузла.
        /// </summary>
        /// <returns>Список кортежів з ідентифікатором вузла, відстанню до нього та кількістю транзитних ребер</returns>
    public List<(string NodeId, int Distance, int TransitCount)> GetDistanceTable()
    {
        var result = new List<(string NodeId, int Distance, int TransitCount)>();
        foreach (var node in distances.Keys)
        {
            result.Add((node.Id, distances[node], transitCount[node]));
        }
        return result;
    }

        /// <summary>
        /// Отримуємо список маршрутів до всіх вузлів від початкового вузла.
        /// </summary>
        /// <returns>Список кортежів з ідентифікатором вузла та списком вузлів, що представляють маршрут</returns>
    public List<(string NodeId, string Path)> GetRoutes()
    {
        var result = new List<(string NodeId, string Path)>();
        foreach (var node in graph.Nodes)
        {
            if (distances[node] != int.MaxValue)
            {
                var path = GetPathToString(node);
                result.Add((node.Id, string.Join(" -> ", path)));
            }
        }
        return result;
    }


/// <summary>
/// Формує шлях від початкового вузла до заданого цільового вузла у вигляді списку ідентифікаторів вузлів.
/// </summary>
/// <param name="targetNode">Цільовий вузол, до якого формується шлях.</param>
/// <returns>Список ідентифікаторів вузлів, що представляє шлях до цільового вузла, у зворотному порядку.</returns>
    private List<string> GetPathToString(Node targetNode)
    {
        List<string> path = new List<string>();
        for (Node at = targetNode; at != null; at = previous[at])
        {
            path.Add(at.Id);
        }
        path.Reverse();
        return path;
    }

/// <summary>
/// Формує шлях від початкового вузла до цільового вузла у вигляді списку вузлів.
/// </summary>
/// <param name="startNode">Початковий вузол, з якого починається шлях.</param>
/// <param name="targetNode">Цільовий вузол, до якого формується шлях.</param>
/// <returns>Список вузлів, що представляє шлях від початкового вузла до цільового. Повертає порожній список, якщо шлях недоступний.</returns>
    public List<Node> GetPathTo(Node startNode, Node targetNode)
    {
        List<Node> path = new List<Node>();

        // Проходимо по словнику previous від цільового вузла до початкового
        for (Node at = targetNode; at != null; at = previous.ContainsKey(at) ? previous[at] : null)
        {
            path.Add(at);
        }

        // Якщо маршрут порожній або не починається з початкового вузла, значить маршруту немає
        if (path.Count == 0 || path.Last() != startNode)
        {
            return new List<Node>(); // Повертаємо порожній список, якщо шлях недоступний
        }

        path.Reverse(); // Перевертаємо список, щоб він ішов від початкового вузла до цільового
        return path;
    }


}
