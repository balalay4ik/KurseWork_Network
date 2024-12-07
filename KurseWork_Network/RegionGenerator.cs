using KurseWork_Network;
using System;
using System.Collections.Generic;

public class RegionGenerator
{
    public int[] Weights { get; set; } = { 3, 5, 7, 8, 11, 12, 15, 18, 21, 25 };
    private Random random = new Random();

    // Метод для генерации дерева
    public Tree Generate(int regionCount, int nodesPerRegion, int minDistance)
    {
        Tree tree = new Tree(); // Создаём дерево для хранения данных
        List<List<Node>> allRegions = new List<List<Node>>(); // Хранение всех узлов по регионам

        int centerX = 400; // Центр всей структуры по X
        int centerY = 400; // Центр всей структуры по Y
        int globalRadius = 300; // Радиус для расположения регионов
        double angleStep = 2 * Math.PI / regionCount; // Угол между регионами

        // Генерация регионов
        for (int region = 0; region < regionCount; region++)
        {
            // Вычисляем центр региона
            double angle = region * angleStep;
            int regionCenterX = centerX + (int)(globalRadius * Math.Cos(angle));
            int regionCenterY = centerY + (int)(globalRadius * Math.Sin(angle));

            List<Node> regionNodes = new List<Node>(); // Узлы текущего региона

            // Генерация узлов внутри региона
            for (int i = 0; i < nodesPerRegion; i++)
            {
                Node node;
                do
                {
                    int x = regionCenterX + random.Next(-100, 100);
                    int y = regionCenterY + random.Next(-100, 100);
                    //node = new Node($"R{region + 1}N{i + 1}", x, y);
                    node = new Node($"{region + 1}_{i + 1}", x, y);
                } while (IsTooCloseToOtherNodes(node, regionNodes, minDistance)); // Проверяем пересечения
                tree.AddNode(node);
                regionNodes.Add(node);
            }

            // Соединение узлов внутри региона
            ConnectNodesWithinRegion(tree, regionNodes);

            allRegions.Add(regionNodes); // Сохраняем узлы региона
        }

        // Соединение регионов
        ConnectRegions(tree, allRegions);

        return tree;
    }

    // Проверка минимального расстояния между узлами
    private bool IsTooCloseToOtherNodes(Node newNode, List<Node> nodes, int minDistance)
    {
        foreach (var node in nodes)
        {
            int dx = newNode.X - node.X;
            int dy = newNode.Y - node.Y;
            if (Math.Sqrt(dx * dx + dy * dy) < minDistance)
                return true;
        }
        return false;
    }

    // Соединение узлов внутри региона
    private void ConnectNodesWithinRegion(Tree tree, List<Node> nodes)
    {
        int nodeCount = nodes.Count;
        int targetEdgeCount = (int)(nodeCount * 3.5 / 2); // Целевое количество рёбер
        int currentEdgeCount = 0;

        Random random = new Random();

        // Перебираем пары узлов
        for (int i = 0; i < nodeCount; i++)
        {
            for (int j = i + 1; j < nodeCount; j++)
            {
                // Проверяем, нужно ли добавлять ребро, чтобы достичь целевого количества
                if (currentEdgeCount < targetEdgeCount && random.NextDouble() < 0.5)
                {
                    int weight = GetRandomWeight(); // Случайный вес рёбра
                    tree.AddEdge(nodes[i], nodes[j], weight, ChannelType.Duplex);
                    currentEdgeCount++;

                    // Прерываем цикл, если достигли цели
                    if (currentEdgeCount >= targetEdgeCount)
                    {
                        return;
                    }
                }
            }
        }
    }

    public int GetRandomWeight()
    {
        return Weights[random.Next(Weights.Length)];
    }


    // Соединение регионов одной связью
    private void ConnectRegions(Tree tree, List<List<Node>> regions)
    {
        HashSet<Node> usedNodes = new HashSet<Node>(); // Узлы, уже связанные с другими регионами

        for (int i = 0; i < regions.Count - 1; i++)
        {
            Node? fromNode = null;
            Node? toNode = null;
            double minDistance = double.MaxValue;

            // Находим ближайшие узлы между регионами, исключая уже использованные
            foreach (var nodeA in regions[i])
            {
                if (usedNodes.Contains(nodeA)) continue; // Пропускаем уже использованные узлы

                foreach (var nodeB in regions[i + 1])
                {
                    if (usedNodes.Contains(nodeB)) continue; // Пропускаем уже использованные узлы

                    double distance = CalculateDistance(nodeA, nodeB);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        fromNode = nodeA;
                        toNode = nodeB;
                    }
                }
            }

            // Создаём связь, если нашли подходящие узлы
            if (fromNode != null && toNode != null)
            {
                int weight = GetRandomWeight();
                tree.AddEdge(fromNode, toNode, weight, ChannelType.Duplex);

                // Отмечаем узлы как использованные
                usedNodes.Add(fromNode);
                usedNodes.Add(toNode);
            }
        }

        // Опционально: замыкание в кольцо
        if (regions.Count > 2)
        {
            Node? fromNode = null;
            Node? toNode = null;
            double minDistance = double.MaxValue;

            foreach (var nodeA in regions[^1])
            {
                if (usedNodes.Contains(nodeA)) continue;

                foreach (var nodeB in regions[0])
                {
                    if (usedNodes.Contains(nodeB)) continue;

                    double distance = CalculateDistance(nodeA, nodeB);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        fromNode = nodeA;
                        toNode = nodeB;
                    }
                }
            }

            if (fromNode != null && toNode != null)
            {
                int weight = GetRandomWeight();
                tree.AddEdge(fromNode, toNode, weight, ChannelType.Duplex);
                usedNodes.Add(fromNode);
                usedNodes.Add(toNode);
            }
        }
    }


    // Метод для вычисления расстояния между узлами
    private double CalculateDistance(Node nodeA, Node nodeB)
    {
        if(nodeA == null || nodeB == null) return 0;
        int dx = nodeA.X - nodeB.X;
        int dy = nodeA.Y - nodeB.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

}
