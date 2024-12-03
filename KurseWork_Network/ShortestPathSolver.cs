using KurseWork_Network;
using System;
using System.Collections.Generic;

public class ShortestPathSolver
{
    private Tree graph;
    private Dictionary<Node, int> distances; // Расстояния от начального узла
    private Dictionary<Node, Node> previous; // Предыдущий узел в пути
    private Dictionary<Node, int> transitCount; // Количество транзитных рёбер

    public ShortestPathSolver(Tree graph)
    {
        this.graph = graph;
    }

    // Метод для поиска кратчайших путей
    public void FindShortestPaths(Node startNode)
    {
        distances = new Dictionary<Node, int>();
        previous = new Dictionary<Node, Node>();
        transitCount = new Dictionary<Node, int>();
        HashSet<Node> visited = new HashSet<Node>();

        // Инициализация расстояний
        foreach (var node in graph.Nodes)
        {
            distances[node] = int.MaxValue;
            previous[node] = null;
            transitCount[node] = int.MaxValue;
        }

        distances[startNode] = 0;
        transitCount[startNode] = 0;

        // Очередь с приоритетом
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

    // Получить таблицу расстояний
    public List<(string NodeId, int Distance, int TransitCount)> GetDistanceTable()
    {
        var result = new List<(string NodeId, int Distance, int TransitCount)>();
        foreach (var node in distances.Keys)
        {
            result.Add((node.Id, distances[node], transitCount[node]));
        }
        return result;
    }

    // Получить список маршрутов
    public List<(string NodeId, string Path)> GetRoutes()
    {
        var result = new List<(string NodeId, string Path)>();
        foreach (var node in graph.Nodes)
        {
            if (distances[node] != int.MaxValue)
            {
                var path = GetPathTo(node);
                result.Add((node.Id, string.Join(" -> ", path)));
            }
        }
        return result;
    }

    // Метод для восстановления пути до узла
    private List<string> GetPathTo(Node targetNode)
    {
        List<string> path = new List<string>();
        for (Node at = targetNode; at != null; at = previous[at])
        {
            path.Add(at.Id);
        }
        path.Reverse();
        return path;
    }
}
