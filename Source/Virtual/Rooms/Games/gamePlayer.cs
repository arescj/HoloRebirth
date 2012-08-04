using System;

using Holo.Managers;
using Holo.Virtual.Users;

namespace Holo.Virtual.Rooms.Games
{
    /// <summary>
    /// A generic player object for a virtual user that plays a 'BattleBall' or 'SnowStorm' game.
    /// </summary>
    internal class gamePlayer
    {
        #region Declares
        internal Game Game;
        /// <summary>
        /// The parent virtualUser object of this player.
        /// </summary>
        internal virtualUser User;
        /// <summary>
        /// The ID that represents this player in the arena.
        /// </summary>
        internal int roomUID;
        /// <summary>
        /// The ID of the team that this user has joined.
        /// </summary>
        internal int teamID;
        /// <summary>
        /// The X position of the user in the arena.
        /// </summary>
        internal int X;
        /// <summary>
        /// The Y position of the user in the arena.
        /// </summary>
        internal int Y;
        /// <summary>
        /// The rotation of this user in the arena. Range: 0-9
        /// </summary>
        internal int Z;
        /// <summary>
        /// The height of this user in the arena.
        /// </summary>
        internal int H;
        /// <summary>
        /// The amount of points that the user has gathered so far.
        /// </summary>
        internal int Score;
        /// <summary>
        /// The X position of the destination at moving in the arena. If not moving, then -1.
        /// </summary>
        internal int goalX;
        /// <summary>
        /// The Y position of the destination at moving in the arena.
        /// </summary>
        internal int goalY;
        internal bool enteringGame;
        internal bool bbColorTile;
        #endregion

        #region Constructor
        internal gamePlayer(virtualUser User, int roomUID, Game Game)
        {
            this.User = User;
            this.roomUID = roomUID;
            this.Game = Game;
            this.teamID = -1;
        }
        #endregion

        #region Functions
        /// <summary>
        /// Sends a single packet to the parent virtual user of this player. On error the action is skipped.
        /// </summary>
        /// <param name="Data">The packet to send.</param>
        internal void sendData(string Data)
        {
            try { User.sendData(Data); }
            catch { }
        }
        #endregion
    }
}
