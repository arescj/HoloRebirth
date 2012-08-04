using System;
using System.Collections;

namespace Holo.Virtual.Rooms.Pathfinding
{
    /// <summary>
    /// Provides pathfinding in the Astar algorithm. This class is written by Nillus.
    /// </summary>
    public class DenDribbelaerPathfinder
    {
        #region Declares
        private virtualRoom.squareState[,] squareStates;
        private double[,] squareHeights;
        private bool[,] squareUnitFlags;
        private int maxX;
        private int maxY;
        private mapNode Start;
        private mapNode Goal;
        private Heap Open = new Heap();
        private Heap Closed = new Heap();
        private ArrayList Successors = new ArrayList();
        #endregion

        #region Constructors
        public DenDribbelaerPathfinder(virtualRoom.squareState[,] squareStates, double[,] squareHeights, bool[,] squareUnitFlags)
        {
            this.squareStates = squareStates;
            this.squareHeights = squareHeights;
            this.squareUnitFlags = squareUnitFlags;
            this.maxX = squareUnitFlags.GetLength(1) - 1;
            this.maxY = squareUnitFlags.GetLength(0) - 1;
        }
        #endregion

        #region Methods
        public Coord getNext(int X, int Y, int goalX, int goalY)
        {
            if (X == goalX && Y == goalY)
                return new Coord(-1, 0);

            int maxCycles = (maxX * maxY) ^ 2;
            int Cycles = 0;

            ArrayList Solution = new ArrayList();
            this.Goal = new mapNode(goalX, goalY, 0, null, null, null);
            this.Start = new mapNode(X, Y, 0, null, Goal, null);
            Goal.Start = Goal;
            Start.Start = Start;

            Open.Add(Start);
            while (Open.Count > 0)
            {
                if (Cycles >= maxCycles)
                    return new Coord(-1, 0);
                else
                    Cycles++;

                mapNode Current = (mapNode)Open.Pop();
                if (Current.X == Goal.X && Current.Y == Goal.Y)
                {
                    while (Current != null)
                    {
                        Solution.Insert(0, Current);
                        Current = Current.Parent;
                    }
                    break;
                }
                else
                {
                    foreach (mapNode Successor in Current.Successors(this))
                    {
                        mapNode openNode = null;
                        if (Open.Contains(Successor))
                        {
                            openNode = (mapNode)Open[Open.IndexOf(Successor)];
                            if (Successor.totalCost > openNode.totalCost)
                                continue;
                        }

                        mapNode closedNode = null;
                        if (Closed.Contains(Successor))
                        {
                            closedNode = (mapNode)Closed[Closed.IndexOf(Successor)];
                            if (Successor.totalCost > closedNode.totalCost)
                                continue;
                        }

                        Open.Remove(Successor);
                        Closed.Remove(Successor);
                        Open.Push(Successor);
                    }
                    Closed.Add(Current);
                }
            }
            if (Solution.Count == 0)
                return new Coord(-1, 0);
            else
            {
                mapNode Next = (mapNode)Solution[1];
                return new Coord(Next.X, Next.Y);
            }
        }
        public dribbelaerPath getPath(int X, int Y, int goalX, int goalY)
        {
            if (X == goalX && Y == goalY)
                return null;

            int maxCycles = (maxX * maxY) ^ 2;
            int Cycles = 0;

            dribbelaerPath Path = new dribbelaerPath();
            this.Goal = new mapNode(goalX, goalY, 0, null, null, null);
            this.Start = new mapNode(X, Y, 0, null, Goal, null);
            Goal.Start = Goal;
            Start.Start = Start;

            Open.Add(Start);
            while (Open.Count > 0)
            {
                if (Cycles >= maxCycles)
                    return null;
                else
                    Cycles++;

                mapNode Current = (mapNode)Open.Pop();
                if (Current.X == Goal.X && Current.Y == Goal.Y)
                {
                    while (Current != null)
                    {
                        Path.Coords.Push(new Coord(Current.X, Current.Y));
                        Current = Current.Parent;
                    }
                    break;
                }
                else
                {
                    foreach (mapNode Successor in Current.Successors(this))
                    {
                        mapNode openNode = null;
                        if (Open.Contains(Successor))
                        {
                            openNode = (mapNode)Open[Open.IndexOf(Successor)];
                            if (Successor.totalCost > openNode.totalCost)
                                continue;
                        }

                        mapNode closedNode = null;
                        if (Closed.Contains(Successor))
                        {
                            closedNode = (mapNode)Closed[Closed.IndexOf(Successor)];
                            if (Successor.totalCost > closedNode.totalCost)
                                continue;
                        }

                        Open.Remove(Successor);
                        Closed.Remove(Successor);
                        Open.Push(Successor);
                    }
                    Closed.Add(Current);
                }
            }
            if (Path.Coords.Count == 0)
                return null;
            else
                return Path;
        }
        public bool Passable(int X, int Y, int goalX, int goalY)
        {
            try
            {
                if (squareUnitFlags[goalX, goalY] || (squareStates[goalX, goalY] != virtualRoom.squareState.Open && squareStates[goalX, goalY] != virtualRoom.squareState.Rug))
                    return false;
                else
                    return true;// !(Math.Abs(squareHeights[X, Y] - squareHeights[goalX, goalY]) > 1);
            }
            catch { return false; }
        }
        #endregion
    }
}
