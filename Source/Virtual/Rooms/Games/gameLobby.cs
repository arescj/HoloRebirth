using System;
using System.Text;
using System.Collections;
using System.Collections.ObjectModel;

using Holo.Managers;
using Holo.Virtual.Users;
using Ion.Storage;

namespace Holo.Virtual.Rooms.Games
{
    /// <summary>
    /// A generic game lobby object for a 'BattleBall' or 'SnowStorm' game lobby.
    /// </summary>
    internal class gameLobby
    {
        #region Declares
        /// <summary>
        /// The virtualRoom object where this lobby is hosted in.
        /// </summary>
        internal virtualRoom Room;
        /// <summary>
        /// The rankManager.gameRank object for this lobby, containing the rank title and the minimum and maximimum game points for this lobby.
        /// </summary>
        internal rankManager.gameRank Rank;
        /// <summary>
        /// The collection that contains the games in this lobby.
        /// </summary>
        internal Hashtable Games = new Hashtable();
        /// <summary>
        /// Indicates if this gamelobby hosts 'BattleBall' games. If false, then 'SnowStorm' games are hosted.
        /// </summary>
        internal bool isBattleBall = true;
        /// <summary>
        /// 'BattleBall' only. Specifies the IDs of the powerup objects that are allowed in this lobby.
        /// </summary>
        private int[] allowedPowerups;
        #endregion

        #region Constructors/destructor
        /// <summary>
        /// Intializes the game lobby with a 'BattleBall'/'SnowStorm' mode.
        /// </summary>
        /// <param name="Room">The minimum amount of points that a virtual user requires to start/join a game here.</param>
        /// <param name="isBattleBall">Indicates if this gamelobby hosts 'BattleBall' games. If false, then 'SnowStorm' games are hosted.</param>
        /// <param name="rankTitle">The title of the rank for players that are playing in this lobby. The matching minimum and maximum amounts for game points are loaded.</param>
        /// <param name="roomID">The ID of the room that the user is currently in.</param>
        internal gameLobby(virtualRoom Room, bool isBattleBall, string rankTitle)
        {
            this.Room = Room;
            this.Rank = rankManager.getGameRank(isBattleBall, rankTitle);
            this.isBattleBall = isBattleBall;
            if (isBattleBall)
            {
                string[] Powerups;
                using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
                {
                    Powerups = dbClient.getString("SELECT bb_allowedpowerups FROM games_lobbies WHERE id = '" + Room.roomID + "'").Split(',');
                }
                this.allowedPowerups = new int[Powerups.Length];
                for (int i = 0; i < Powerups.Length; i++)
                    this.allowedPowerups[i] = int.Parse(Powerups[i]);
            }
        }
        /// <summary>
        /// Clears the games from the game lobby and nulls the virtualRoom object.
        /// </summary>
        internal void Clear()
        {
            Games.Clear();
            Games = null;
            Room = null;
        }
        #endregion

        #region Properties
        /// <summary>
        /// The type of the games being played in this game lobby as a two character, lowercase string.
        /// </summary>
        internal string Type
        {
            get
            {
                if (isBattleBall)
                    return "bb";
                else
                    return "ss";
            }
        }
        /// <summary>
        /// The ranks and points of the virtual users inside this gamelobby.
        /// </summary>
        internal string playerRanks
        {
            get
            {
                ICollection Users = Room.Users;
                StringBuilder Ranks = new StringBuilder(Encoding.encodeVL64(Users.Count));
                foreach (virtualRoomUser roomUser in Users)
                    Ranks.Append(Encoding.encodeVL64(roomUser.roomUID) + roomUser.gamePoints + Convert.ToChar(2) + rankManager.getGameRankTitle(this.isBattleBall, roomUser.gamePoints) + Convert.ToChar(2));

                return Ranks.ToString();
            }
        }
        /// <summary>
        /// Returns a boolean that indicates if a certain amount of gamepoints is (skill level) valid for creating/joining a game in this lobby.
        /// </summary>
        /// <param name="Points">The amount of gamepoints.</param>
        internal bool validGamerank(int Points)
        {
            return (Points >= Rank.minPoints && (Rank.maxPoints == 0 || Points <= Rank.maxPoints));
        }
        /// <summary>
        /// Returns the list with 'BattleBall'/'SnowStorm' games, depending on the mode of this lobby. This list is for usage in the game browser's index.
        /// </summary>
        internal string gameList()
        {
            try
            {
                int[] Amounts = new int[3]; // [0] = waiting, [1] = started, [2] = ended
                StringBuilder List = new StringBuilder();

                foreach (Game Game in ((Hashtable)Games.Clone()).Values)
                {
                    List.Append(Encoding.encodeVL64(Game.ID) + Game.Name + Convert.ToChar(2) + Encoding.encodeVL64(Game.Owner.roomUID) + Game.Owner.User._Username + Convert.ToChar(2));
                    if (isBattleBall == false) // SnowStorm game
                        List.Append(Encoding.encodeVL64(Game.leftTime));
                    List.Append(Encoding.encodeVL64(Game.mapID));
                    Amounts[(int)Game.State]++;
                }

                if (Amounts[1] > 0 || Amounts[2] > 0) // There are started/ended games
                    return Encoding.encodeVL64(Amounts[0]) + Encoding.encodeVL64(Amounts[1]) + Encoding.encodeVL64(Amounts[2]) + List.ToString();
                else // Only games in 'waiting for players'-mode
                    return Encoding.encodeVL64(Amounts[0]) + List.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return "";
            }
        }
        internal string getCreateGameSettings()
        {
            if (isBattleBall)
            {
                StringBuilder Settings = new StringBuilder(Encoding.encodeVL64(4)); // 4 values
                Settings.Append("fieldType" + Convert.ToChar(2) + "HJIII" + Encoding.encodeVL64(5)); // Amount of maps to show
                //Settings.Append("maximumSimultaneousPowerups" + Convert.ToChar(2) + "HIJIII" + Encoding.encodeVL64(10)); // Maximum amount of spawned powerups on game field on same time
                //Settings.Append("powerupCreateChance" + Convert.ToChar(2) + "HI" + Encoding.encodeVL64(50) + "III" + Encoding.encodeVL64(100)); // Spawn ratio of powerups
                Settings.Append("numTeams" + Convert.ToChar(2) + "HJJIJI" + Encoding.encodeVL64(4)); // Max amount of teams selectable
                //Settings.Append("coloringForOpponentTimePulses" + Convert.ToChar(2) + "HI" + Encoding.encodeVL64(15) + "III" + Encoding.encodeVL64(100)); // ???
                //Settings.Append("gameLengthSeconds" + Convert.ToChar(2) + "HI" + Encoding.encodeVL64(Config.Game_BattleBall_gameLength_Seconds) + "IHH");

                Settings.Append("allowedPowerups" + Convert.ToChar(2) + "IJ");
                int powerupsAmount = allowedPowerups.Length;
                if (powerupsAmount == 0)
                    Settings.Append("9");
                else
                {
                    for (int i = 0; i < powerupsAmount; i++)
                    {
                        if (i == powerupsAmount - 1)
                            Settings.Append(allowedPowerups[i]);
                        else
                            Settings.Append(allowedPowerups[i] + ",");
                    }
                }
                Settings.Append(Convert.ToChar(2) + "H");

                //Settings.Append("cleaningTilesTimePulses" + Convert.ToChar(2) + "HI" + Encoding.encodeVL64(15) + "III" + Encoding.encodeVL64(100));
                //Settings.Append("powerupCreateFirstTimePulses" + Convert.ToChar(2) + "HIHIHI" + Encoding.encodeVL64(100));
                //Settings.Append("secondsUntilRestart" + Convert.ToChar(2) + "HI" + Encoding.encodeVL64(Config.Game_scoreWindow_restartGame_Seconds) + "IHH");
                //Settings.Append("powerupTimeToLivePulses" + Convert.ToChar(2) + "HI" + Encoding.encodeVL64(60) + "I" + Encoding.encodeVL64(5) + "I" + Encoding.encodeVL64(100));
                //Settings.Append("powerupCreateIntervalPulses" + Convert.ToChar(2) + "HI" + Encoding.encodeVL64(20) + "I" + Encoding.encodeVL64(5) + "I" + Encoding.encodeVL64(100));
                //Settings.Append("stunTimePulses" + Convert.ToChar(2) + "HI" + Encoding.encodeVL64(10) + "III" + Encoding.encodeVL64(100));
                //Settings.Append("highJumpsTimePulses" + Convert.ToChar(2) + "HI" + Encoding.encodeVL64(15) + "III" + Encoding.encodeVL64(100));
                Settings.Append("name" + Convert.ToChar(2) + "IJ" + Convert.ToChar(2) + "H");
                //Settings.Append("secondsUntilStart" + Convert.ToChar(2) + "HI" + Encoding.encodeVL64(Config.Game_Countdown_Seconds) + "IHH");

                return Settings.ToString();
            }
            else
                return "#";
        }
        internal bool allowsPowerup(int ID)
        {
            try
            {
                for (int i = 0; i < allowedPowerups.Length; i++)
                    if (allowedPowerups[i] == ID)
                        return true;
                return false;
            }
            catch { return false; }
        }
        #endregion

        #region Functions
        internal void createGame(gamePlayer Owner, string Name, int mapID, int teamAmount, int[] enabledPowerups)
        {
            int gameID = 0;
            while (Games.ContainsKey(gameID))
                gameID++;

            Owner.Game = new Game(this, gameID, Name, mapID, teamAmount, enabledPowerups, Owner);
            Owner.Game.movePlayer(Owner, -1, 0);
            Games.Add(gameID, Owner.Game);
        }
        #endregion
    }
}
