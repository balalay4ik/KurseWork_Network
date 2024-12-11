using KurseWork_Network;
using System;
using System.Collections.Generic;

public class RegionGenerator
{
    public int[] Weights { get; set; } = { 3, 5, 7, 8, 11, 12, 15, 18, 21, 25 };
    private Random random = new Random();

   
        /// <summary>
        /// Генерація дерева з заданою кількістю регіонів, вузлів в кожному регіоні та мінімальним відстанем між вузлами.
        /// </summary>
        /// <param name="regionCount">Кількість регіонів</param>
        /// <param name="nodesPerRegion">Кількість вузлів в кожному регіоні</param>
        /// <param name="minDistance">Мінімальна відстань між вузлами</param>
        /// <returns>Сгенероване дерево</returns>
    public Tree Generate(int regionCount, int nodesPerRegion, int minDistance)
    {
        Tree tree = new Tree(); // Створюємо дерево для зберігання даних
        List<List<Node>> allRegions = new List<List<Node>>(); // Зберігання всіх вузлів за регіонами

        int centerX = 400; // Центр всієї структури за X
        int centerY = 400; // Центр всієї структури за Y
        int globalRadius = 300; // Радіус для розташування регіонів
        double angleStep = 2 * Math.PI / regionCount; // Кут між регіонами

        // Генерація регіонів
        for (int region = 0; region < regionCount; region++)
        {
            // Розрахунок центру регіону
            double angle = region * angleStep;
            int regionCenterX = centerX + (int)(globalRadius * Math.Cos(angle));
            int regionCenterY = centerY + (int)(globalRadius * Math.Sin(angle));

            List<Node> regionNodes = new List<Node>(); // Вузли поточного регіону

            // Генерація вузлів всередині регіону
            for (int i = 0; i < nodesPerRegion; i++)
            {
                Node node;
                do
                {
                    int x = regionCenterX + random.Next(-150, 150);
                    int y = regionCenterY + random.Next(-150, 150);
                    node = new Node($"{region + 1}_{i + 1}", x, y);
                } while (IsTooCloseToOtherNodes(node, regionNodes, minDistance)); // Перевірка на перетину
                tree.AddNode(node);
                regionNodes.Add(node);
            }

            // Сполучення вузлів всередині регіону
            ConnectNodesWithinRegion(tree, regionNodes);

            allRegions.Add(regionNodes); // Зберігання вузлів регіону
        }

        // Сполучення регіонів
        ConnectRegions(tree, allRegions);

        return tree;
    }


        /// <summary>
        /// Перевірка на те, чи є новий вузол надто близько до інших вузлів.
        /// </summary>
        /// <param name="newNode">Новий вузол</param>
        /// <param name="nodes">Вузли, які вже є в регіоні</param>
        /// <param name="minDistance">Мінімальна відстань між вузлами</param>
        /// <returns>True, якщо новий вузол надто близько до інших вузлів, інакше - False</returns>
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


        /// <summary>
        /// Сполучення вузлів всередині регіону. Мета - отримати граф з приблизно 3.5 ребрами на вузол.
        /// </summary>
        /// <param name="tree">Дерево, до якого додаються ребра</param>
        /// <param name="nodes">Вузли, які сполучаються</param>
    private void ConnectNodesWithinRegion(Tree tree, List<Node> nodes)
    {
        int nodeCount = nodes.Count;
        int targetEdgeCount = (int)(nodeCount * 3.5 / 2); // Целевое количество рёбер
        int currentEdgeCount = 0;

        Random random = new Random();

        // Перебiраємо пари вузлiв
        for (int i = 0; i < nodeCount; i++)
        {
            for (int j = i + 1; j < nodeCount; j++)
            {
                // Перевiряємо, чи потрібно додавати ребро, щоб досягти цільового кiлькiстi
                if (currentEdgeCount < targetEdgeCount && random.NextDouble() < 0.5)
                {
                    int weight = GetRandomWeight(); // Випадковий вiс рiбра
                    tree.AddEdge(nodes[i], nodes[j], weight, ChannelType.Duplex);
                    currentEdgeCount++;

                    // Прериваємо цикл, якщо досягли цiлi
                    if (currentEdgeCount >= targetEdgeCount)
                    {
                        return;
                    }
                }
            }
        }
    }

        /// <summary>
        /// Повертає випадковий вiс рiбра. Вiс рiбра випадково вибирається з масиву Weights.
        /// </summary>
        /// <returns>Випадковий вiс рiбра</returns>
    public int GetRandomWeight()
    {
        return Weights[random.Next(Weights.Length)];
    }


        /// <summary>
        /// Сполучення регіонів між собою. Мета - отримати зв'язок між регіонами, виключаючи вже використані вузли.
        /// </summary>
        /// <param name="tree">Дерево, до якого додаються ребра</param>
        /// <param name="regions">Список регіонів, які сполучаються</param>
    private void ConnectRegions(Tree tree, List<List<Node>> regions)
    {
        HashSet<Node> usedNodes = new HashSet<Node>(); // Вузли, вже зв'язані з іншими регіонами

        for (int i = 0; i < regions.Count - 1; i++)
        {
            Node? fromNode = null;
            Node? toNode = null;
            double minDistance = double.MaxValue;

            // Знаходимо найближчі вузли між регіонами, виключаючи вже використані
            foreach (var nodeA in regions[i])
            {
                if (usedNodes.Contains(nodeA)) continue; // Пропускаємо вже використані вузли

                foreach (var nodeB in regions[i + 1])
                {
                    if (usedNodes.Contains(nodeB)) continue; // Пропускаємо вже використані вузли

                    double distance = CalculateDistance(nodeA, nodeB);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        fromNode = nodeA;
                        toNode = nodeB;
                    }
                }
            }

            // Створюємо зв'язок, якщо знайшли придатні вузли
            if (fromNode != null && toNode != null)
            {
                int weight = GetRandomWeight();
                tree.AddEdge(fromNode, toNode, weight, ChannelType.Duplex);

                // Маруємо вузли як використані
                usedNodes.Add(fromNode);
                usedNodes.Add(toNode);
            }
        }

        // Опціяльно: замикання в кільце
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


    /// <summary>
    /// Обчислює відстань між двома вузлами на площині.
    /// </summary>
    /// <param name="nodeA">Перший вузол</param>
    /// <param name="nodeB">Другий вузол</param>
    /// <returns>Відстань між вузлами або 0, якщо один з вузлів є null</returns>
    private double CalculateDistance(Node nodeA, Node nodeB)
    {
        if(nodeA == null || nodeB == null) return 0;
        int dx = nodeA.X - nodeB.X;
        int dy = nodeA.Y - nodeB.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

}
