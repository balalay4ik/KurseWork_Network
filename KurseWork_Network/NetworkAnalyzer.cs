using KurseWork_Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class NetworkAnalyzer
{
    private Tree network;

    public NetworkAnalyzer(Tree network)
    {
        this.network = network;
    }

    public double AnalyzeTraffic(
        Node startNode,
        Node endNode,
        int messageSize,
        int packetSize,
        int serviceInfoSize,
        bool isVirtualChannel
    )
    {
        ShortestPathSolver solver = new ShortestPathSolver(network);
        solver.FindShortestPaths(startNode);
        var distances = solver.GetDistanceTable()
            .ToDictionary(item => network.Nodes.First(n => n.Id == item.NodeId), item => item.Distance);

        if (!distances.ContainsKey(endNode) || distances[endNode] == int.MaxValue)
        {
            MessageBox.Show("Нет маршрута к конечной ноде!");
            return 0;
        }

        double routeDistance = distances[endNode];
        //int packetCount = (int)Math.Ceiling((double)messageSize / packetSize);
        //int servicePackets = isVirtualChannel ? 2 : packetCount;

        //double totalTraffic = routeDistance * (packetCount * packetSize + servicePackets * serviceInfoSize);
        return routeDistance;
    }

    public (double totalTime, Status status) AnalyzeTraffic(Node startNode, Node endNode, bool isFirstConnection = false)
    {
        double totalTime = 0;

        var route = GetRoute(startNode, endNode);

        // Время на установление соединения (учитывается только для первого соединения)
        if (isFirstConnection)
        {
            totalTime += route.Sum(edge => edge.Weight) * 2; // Время для 2 пакетов на установление соединения
        }

        // Проверка на ошибки
        Edge errorEdge = CalculateRouteErrorEdge(route);

        if (errorEdge == null)
        {
            // Если ошибки нет, добавляем время на полный маршрут
            totalTime += route.Sum(edge => edge.Weight);
            return (totalTime, Status.Ok);  
        }

        // Если ошибка есть, считаем время до рёбра с ошибкой
        foreach (var edge in route)
        {
            totalTime += edge.Weight;

            if (edge == errorEdge)
            {
                return (totalTime, Status.Error); // Возвращаем время до ошибки и статус
            }
        }

        return (totalTime, Status.Error); // На всякий случай
    }

    private List<Edge> GetRoute(Node startNode, Node endNode)
    {
        // Создаём экземпляр алгоритма Дейкстры
        ShortestPathSolver shortestPathSolver = new ShortestPathSolver(network);

        // Выполняем поиск кратчайших путей от startNode
        shortestPathSolver.FindShortestPaths(startNode);

        // Получаем путь как список узлов
        var path = shortestPathSolver.GetPathTo(startNode, endNode);

        if (path == null || path.Count < 2)
        {
            throw new Exception("Маршрут не найден!");
        }

        // Преобразуем путь (список узлов) в список рёбер
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
                throw new Exception($"Ребро между узлами {path[i].Id} и {path[i + 1].Id} не найдено!");
            }
        }

        return route;
    }


    private Edge CalculateRouteErrorEdge(List<Edge> route)
    {
        Random random = new Random();

        foreach (var edge in route)
        {
            // Сравниваем вероятность ошибки на рёбре
            if (random.NextDouble() < edge.ErrorProbability / 100.0)
            {
                return edge; // Возвращаем ребро, где произошла ошибка
            }
        }

        return null; // Если ошибки не произошло
    }
}

public enum Status
{
    Ok,
    Error
}

