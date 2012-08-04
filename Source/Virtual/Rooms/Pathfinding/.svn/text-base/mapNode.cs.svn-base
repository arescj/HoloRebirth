using System;
using System.Collections.ObjectModel;

namespace Holo.Virtual.Rooms.Pathfinding
{
    /// <summary>
    /// Represents a node on the map for Astar pathfinding. This class implements the IComparable interface. :o
    /// </summary>
    public class mapNode : IComparable
    {
        #region Declares
        /// <summary>
        /// The X position of this node.
        /// </summary>
        internal int X;
        /// <summary>
        /// The Y position of this node.
        /// </summary>
        internal int Y;
        /// <summary>
        /// The mapNode object of the parent node of this node.
        /// </summary>
        internal mapNode Parent;
        internal mapNode Start;
        /// <summary>
        /// The mapNode object of the goal node of this node.
        /// </summary>
        private mapNode Goal;
        /// <summary>
        /// The cost for the pathfinder to pass this node.
        /// </summary>
        internal double Cost;
        /// <summary>
        /// The estimated cost for the pathfinder from this node to the goal node.
        /// </summary>
        private double _esToGoal;
        #endregion

        #region Constructors
        /// <summary>
        /// Intializes a new mapNode.
        /// </summary>
        /// <param name="X">The X position of this node.</param>
        /// <param name="Y">The Y position of t his node.</param>
        /// <param name="Cost">The cost for the pathfinder to pass this node.</param>
        /// <param name="Parent">The mapNode object of the parent node of this node.</param>
        /// <param name="Goal">The mapNode object of the goal node of this node.</param>
        public mapNode(int X, int Y, double Cost, mapNode Parent, mapNode Goal, mapNode Start)
        {
            this.Parent = Parent;
            this.Start = Start;
            this.Goal = Goal;
            this.X = X;
            this.Y = Y;
            this.Cost = Cost;
        }
        #endregion

        #region Properties
        /// <summary>
        /// The heuristic estimated cost to the goal. (Manhattan calculation)
        /// </summary>
        internal double goalEstimate
        {
            get
            {
                if (Goal == null)
                    return  0;
                else
                {
                    int a1 = 2 * this.X;
                    int a2 = 2 * this.Y + this.X % 2 - this.X;
                    int a3 = -2 * this.Y - this.X % 2 - this.X;
                    int b1 = 2 * Goal.X;
                    int b2 = 2 * Goal.Y + Goal.X % 2 - Goal.X;
                    int b3 = -2 * Goal.Y - Goal.X % 2 - Goal.X;
                    return Math.Max(Math.Abs(a1 - b1), Math.Max(Math.Abs(a2 - b2), Math.Abs(a3 - b3)));

                    //int dx1 = X - Goal.X;
                    //int dy1 = Y - Goal.Y;
                    //int dx2 = Start.X - Goal.X;
                    //int dy2 = Start.Y - Goal.Y;
                    //return Math.Abs((dx1 * dy2) - dx2 * dy1) * 0.001;

                    //return 

                    //double xD = X - Goal.X;
                    //double yD = Y - Goal.Y;
                    //return Math.Sqrt(Math.Pow(xD, 2) + Math.Pow(yD, 2));
                    //return Math.Max(Math.Abs(xD), Math.Abs(yD));
                }
                //return _esToGoal;
            }
            set { _esToGoal = value; }
        }
        /// <summary>
        /// The total cost of this node to the goal.
        /// </summary>
        internal double totalCost
        {
            get { return Cost + goalEstimate; }
        }
        #endregion

        #region IComparable additions
        /// <summary>
        /// Compares this node's total cost to the total cost of another mapNode and returns the result.
        /// </summary>
        /// <param name="obj">The mapNode object to compare the total cost with.</param>
        int IComparable.CompareTo(Object obj)
        {
            return -totalCost.CompareTo(((mapNode)obj).totalCost);
        }
        #endregion
        #region Methods
        /// <summary>
        /// Creates a mapNode object for a certain coordinate, with this node's cost. The parent node of the new node will be set to this node.
        /// </summary>
        /// <param name="X">The X position of the new node.</param>
        /// <param name="Y">The Y position of the new node.</param>
        private mapNode createNode(int X, int Y)
        {
            int dx1 = this.X - Goal.X;
            int dy1 = this.X - Goal.Y;
            int dx2 = Start.X - Goal.X;
            int dy2 = Start.Y - Goal.Y;
            double Tie = Math.Abs((dx1 * dy2) - (dx2 * dy1)) * 0.005;
            return new mapNode(X, Y, this.Cost + Tie, this, this.Goal,this.Start);
        }
        /// <summary>
        /// Returns an objectmodel collection of the type mapNode, which contains all the successor nodes to this node.
        /// </summary>
        internal Collection<mapNode> Successors(DenDribbelaerPathfinder Pathfinder)
        {
            Collection<mapNode> tmp = new Collection<mapNode>();
            if (Pathfinder.Passable(X, Y, X - 1, Y))
                tmp.Add(createNode(X - 1, Y)); // Left
            if (Pathfinder.Passable(X, Y, X + 1, Y))
                tmp.Add(createNode(X + 1, Y)); // Right
            if (Pathfinder.Passable(X, Y, X, Y + 1))
                tmp.Add(createNode(X, Y + 1)); // Up
            if (Pathfinder.Passable(X, Y, X, Y - 1))
                tmp.Add(createNode(X, Y - 1)); // Down
            if (Pathfinder.Passable(X, Y, X - 1, Y + 1))
                tmp.Add(createNode(X - 1, Y + 1)); // Left up
            if (Pathfinder.Passable(X, Y, X - 1, Y - 1))
                tmp.Add(createNode(X - 1, Y - 1)); // Left down
            if (Pathfinder.Passable(X, Y, X, Y + 1))
                tmp.Add(createNode(X, Y + 1)); // Mid up
            if (Pathfinder.Passable(X, Y, X, Y - 1))
                tmp.Add(createNode(X, Y - 1)); // Mid down
            if (Pathfinder.Passable(X, Y, X + 1, Y + 1))
                tmp.Add(createNode(X + 1, Y + 1)); // Right up
            if (Pathfinder.Passable(X, Y, X + 1, Y - 1))
                tmp.Add(createNode(X + 1, Y - 1)); // Right down

            return tmp;
        }
        #endregion
    }
}
