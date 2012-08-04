using System;
using System.Data;
using System.Text;
using System.Threading;
using System.Collections.ObjectModel;

using Holo.Virtual.Users;
using Holo.Virtual.Rooms.Pathfinding;
using Ion.Storage;
namespace Holo.Virtual.Rooms.Games
{
    internal class Game
    {
        #region Declares
        /// <summary>
        /// The gameLobby object where this game is hosted in.
        /// </summary>
        internal gameLobby Lobby;
        /// <summary>
        /// Indicates if this game is of the type 'BattleBall'. If false, then the game is of the type 'SnowStorm'.
        /// </summary>
        internal bool isBattleBall;
        /// <summary>
        /// The ID of this game.
        /// </summary>
        internal int ID;
        /// <summary>
        /// The display name of this game.
        /// </summary>
        internal string Name;
        /// <summary>
        /// The ID of the map that is played with this game.
        /// </summary>
        internal int mapID;
        /// <summary>
        /// The total amount of gametime in seconds.
        /// </summary>
        internal int totalTime;
        /// <summary>
        /// The amount of gametime is seconds that is still left on this game.
        /// </summary>
        internal int leftTime;
        internal gameState State;
        /// <summary>
        /// BattleBall only. The numbers of the enabled powerups for this game.
        /// </summary>
        internal int[] enabledPowerups;
        /// <summary>
        /// The gamePlayer object (bbPlayer or ssPlayer) of the virtual user that has created this game.
        /// </summary>
        internal gamePlayer Owner;
        /// <summary>
        /// Collection with the gamePlayer objects of the virtual users that have 'checked in' to this game sub and should receive updates about team status etc.
        /// </summary>
        internal Collection<gamePlayer> Subviewers = new Collection<gamePlayer>();
        internal Collection<gamePlayer>[] Teams;
        #region Map declares
        #region Enumerators etc
        internal enum gameState { Waiting = 0, Started = 1, Ended = 2 };
        /// <summary>
        /// Represents the state of a tile on the gamemap in a game of 'BattleBall'.
        /// </summary>
        private enum bbTileState { Default = 0, Touched = 1, Clicked = 2, Pressed = 3, Sealed = 4 }
        /// <summary>
        /// Represents the color of the team that owns a tile on the gamemap in a game of 'BattleBall'.
        /// </summary>
        private enum bbTileColor { Disabled = -2, Default = -1, Red = 0, Blue = 1, Yellow = 2, Green = 3 }
        #endregion
        /// <summary>
        /// The amount of seconds that the 'loading game' bar still has to be displayed.
        /// </summary>
        private int leftCountdownSeconds = Config.Game_Countdown_Seconds;
        /// <summary>
        /// The length of the arena.
        private int bX;
        /// <summary>
        /// The max width of the arena.
        /// </summary>
        private int bY;
        /// <summary>
        /// The heightmap of the arena.
        /// </summary>
        internal string Heightmap;
        private byte[,] Height;
        private bool[,] Blocked;
        private bbTileState[,] bbTState;
        private bbTileColor[,] bbTColor;
        private Thread updateHandler;
        #endregion
        #endregion

        #region Constructors/destructor
        /// <summary>
        /// Initializes a 'BattleBall' game.
        /// </summary>
        /// <param name="Lobby">The gameLobby object of the lobby where this game is hosted in.</param>
        /// <param name="ID">The ID of this game.</param>
        /// <param name="Name">The display name of this game.</param>
        /// <param name="mapID">The ID of the map that is played with this game.</param>
        /// <param name="teamAmount">The amount of teams that is used with the game.</param>
        /// <param name="enabledPowerups">The numbers of the enabled powerups for this game.</param>
        /// <param name="Owner">The gamePlayer instance of the virtual user that has created this game.</param>
        internal Game(gameLobby Lobby, int ID, string Name, int mapID, int teamAmount, int[] enabledPowerups, gamePlayer Owner)
        {
            this.Lobby = Lobby;
            this.ID = ID;
            this.Name = Name;
            this.mapID = mapID;
            this.Owner = Owner;
            this.enabledPowerups = enabledPowerups;
            this.totalTime = Config.Game_BattleBall_gameLength_Seconds;
            this.leftTime = Config.Game_BattleBall_gameLength_Seconds;
            this.isBattleBall = true;

            // Dimension teams
            this.Teams = new Collection<gamePlayer>[teamAmount];
            for (int i = 0; i < teamAmount; i++)
                Teams[i] = new Collection<gamePlayer>();
        }
        /// <summary>
        /// Initializes a 'SnowStorm' game.
        /// </summary>
        /// <param name="Lobby">The gameLobby object of the lobby where this game is hosted in.</param>
        /// <param name="ID">The ID of this game.</param>
        /// <param name="Name">The display name of this game.</param>
        /// <param name="mapID">The ID of the map that is played with this game.</param>
        /// <param name="teamAmount">The amount of teams that is used with the game.</param>
        /// <param name="totalTime">The total amount of gametime in seconds.</param>
        /// <param name="Owner">The gamePlayer instance of the virtual user that has created this game.</param>
        internal Game(gameLobby Lobby, int ID, string Name, int mapID, int teamAmount, int totalTime, gamePlayer Owner)
        {
            this.Lobby = Lobby;
            this.ID = ID;
            this.Name = Name;
            this.mapID = mapID;
            this.Owner = Owner;
            this.totalTime = totalTime;
            this.leftTime = totalTime;
            this.isBattleBall = false;

            // Dimension teams
            this.Teams = new Collection<gamePlayer>[teamAmount];
            for (int i = 0; i < teamAmount; i++)
                Teams[i] = new Collection<gamePlayer>();
        }
        /// <summary>
        /// Stops the game, and removes all players and subviewers from the game.
        /// </summary>
        internal void Abort()
        {
            try { updateHandler.Abort(); }
            catch { }

            string Data = "Cm" + "H";
            for (int i = 0; i < Teams.Length; i++)
                foreach (gamePlayer Member in Teams[i])
                {
                    if (Member.User != null)
                    {
                        if (State == gameState.Waiting)
                            Member.User.sendData(Data);
                        Member.User.gamePlayer = null;
                    }
                }

            foreach (gamePlayer subViewer in Subviewers)
            {
                if (subViewer.User != null)
                {
                    if (State == gameState.Waiting)
                        subViewer.User.sendData(Data);
                    subViewer.User.gamePlayer = null;
                }
            }
        }
        #endregion

        #region Player management
        /// <summary>
        /// Moves a player to a certain team, or removes a player from the game.
        /// </summary>
        /// <param name="Player">The gamePlayer object of the player to move.</param>
        /// <param name="fromTeamID">The ID of the team where the player was in before. If the player is new to the game, then this parameter should be -1.</param>
        /// <param name="toTeamID">The ID of the team where the player is moving to. If the player is leaving the game, then this parameter should be -1.</param>
        internal void movePlayer(gamePlayer Player, int fromTeamID, int toTeamID)
        {
            try
            {
                if (fromTeamID != -1) // Remove player from previous team
                    Teams[fromTeamID].Remove(Player);

                if (toTeamID != -1) // User added to team
                {
                    if (Teams[toTeamID].Contains(Player) == false)
                        Teams[toTeamID].Add(Player);
                    Player.teamID = toTeamID;
                }
                else
                {
                    Teams[Player.teamID].Remove(Player);
                    Player.User.gamePlayer = null;
                }

                sendData("Ci" + this.Sub);
            }
            catch { }
        }
        /// <summary>
        /// Indicates if the game can be launched. Games are 'launchable' when there are atleast two teams with players.
        /// </summary>
        internal bool Launchable
        {
            get
            {
                int activeTeamCount = 0;
                for (int i = 0; i < Teams.Length; i++)
                    if (Teams[i].Count != 0) // This team has got players
                        activeTeamCount++;
                return (activeTeamCount > 1); // Atleast two teams with players
            }
        }
        /// <summary>
        /// Returns a boolean that indicates if a certain team has space left for new team members. On error, false is returned.
        /// </summary>
        /// <param name="teamID">The ID of the team to check.</param>
        internal bool teamHasSpace(int teamID)
        {
            try
            {
                int m = 0;
                int l = Teams.Length;
                if (l == 2)
                    m = 6;
                else if (l == 3)
                    m = 4;
                else if (l == 4)
                    m = 3;

                return (Teams[teamID].Count < m);
            }
            catch { return false; }
        }
        /// <summary>
        /// Sends a single packet to all players, spectators and subviewers of this game.
        /// </summary>
        /// <param name="Data">The packet to send.</param>
        internal void sendData(string Data)
        {
            for (int i = 0; i < Teams.Length; i++)
                foreach (gamePlayer Member in Teams[i])
                    Member.sendData(Data);

            if (State == gameState.Waiting) // Game hasn't started yet
            {
                foreach (gamePlayer subViewer in Subviewers)
                    subViewer.sendData(Data);
            }
        }
        #endregion

        #region Properties
        internal string Sub
        {
            get
            {
                StringBuilder Entry = new StringBuilder(Encoding.encodeVL64((int)State));
                if (Launchable) // Atleast two teams with waiting players
                    Entry.Append("I"); // 1 = Game can be started
                else
                    Entry.Append("H"); // 0 = Game can't be started yet

                Entry.Append(Name + Convert.ToChar(2) + Encoding.encodeVL64(Owner.roomUID) + Owner.User._Username + Convert.ToChar(2));
                if (isBattleBall == false) // SnowStorm game
                    Entry.Append(Encoding.encodeVL64(totalTime));
                Entry.Append(Encoding.encodeVL64(mapID) + Encoding.encodeVL64(0) + Encoding.encodeVL64(Teams.Length));

                for (int i = 0; i < Teams.Length; i++)
                {
                    Entry.Append(Encoding.encodeVL64(Teams[i].Count));
                    foreach (gamePlayer Member in Teams[i])
                        Entry.Append(Encoding.encodeVL64(Member.roomUID) + Member.User._Username + Convert.ToChar(2));
                }

                if (isBattleBall) // BattleBall game
                {
                    for (int i = 0; i < enabledPowerups.Length; i++)
                        Entry.Append(enabledPowerups[i] + ",");
                    Entry.Append("9" + Convert.ToChar(2));
                }
                else // SnowStorm game
                    Entry.Append(Encoding.encodeVL64(leftTime) + Encoding.encodeVL64(mapID));

                return Entry.ToString();
            }
        }
        #endregion

        #region Arena
        internal void startGame()
        {
            State = gameState.Started;
            foreach (gamePlayer subViewer in Subviewers)
                subViewer.sendData(this.Sub);

            using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
            {
                this.Heightmap = dbClient.getString("SELECT heightmap FROM games_maps WHERE type = '" + Lobby.Type + "' AND id = '" + mapID + "'");
            }
            string[] _H = Heightmap.Split(Convert.ToChar(13));
            this.bX = _H[0].Length;
            this.bY = _H.Length - 1;
            string[] _T = null;

            if (isBattleBall)
            {
                this.bbTState = new bbTileState[bX, bY];
                this.bbTColor = new bbTileColor[bX, bY];
                using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
                {
                    _T = dbClient.getString("SELECT bb_tilemap FROM games_maps WHERE type = 'bb' AND id = '" + mapID + "'").Split(Convert.ToChar(13));
                }
            }
            this.Height = new byte[bX, bY];
            this.Blocked = new bool[bX, bY];

            for (int y = 0; y < bY; y++)
            {
                for (int x = 0; x < bX; x++)
                {
                    string q = _H[y].Substring(x, 1);
                    if (q == "x")
                    {
                        Blocked[x, y] = true;
                        if (isBattleBall)
                        {
                            bbTColor[x, y] = bbTileColor.Disabled;
                            bbTState[x, y] = bbTileState.Sealed;
                        }
                    }
                    else
                    {
                        Height[x, y] = Convert.ToByte(q);
                        if (isBattleBall)
                        {
                            if (_T[y].Substring(x, 1) == "1")
                                bbTColor[x, y] = bbTileColor.Default;
                            else
                            {
                                bbTColor[x, y] = bbTileColor.Disabled;
                                bbTState[x, y] = bbTileState.Sealed;
                            }
                        }
                    }
                }
            }

            int j = 0;

            for (int i = 0; i < Teams.Length; i++)
            {
                if (Teams[i].Count == 0) // Empty team
                    continue;

                DataRow dRow;
                using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
                {
                    dRow = dbClient.getRow("SELECT x,y,z FROM games_maps_playerspawns WHERE type = '" + Lobby.Type + "' AND mapid = '" + mapID + "' AND teamid = '" + i + "'");
                }
                int spawnX = Convert.ToInt32(dRow["x"]);
                int spawnY = Convert.ToInt32(dRow["y"]);
                byte spawnZ = Convert.ToByte(dRow["z"]);
                bool Flip = false;

                foreach (gamePlayer Player in Teams[i])
                {
                    Player.roomUID = j;
                rs:
                    try
                    {
                        while (Blocked[spawnX, spawnY])
                        {
                            if (spawnZ == 0 || spawnZ == 2)
                            {
                                if (Flip)
                                    spawnX -= 1;
                                else
                                    spawnX += 1;
                            }
                            else if (spawnZ == 4 || spawnZ == 6)
                            {
                                if (Flip)
                                    spawnY -= 1;
                                else
                                    spawnY += 1;
                            }
                        }
                        Flip = (Flip != true);
                    }
                    catch { Flip = (Flip != true); goto rs; } // Out of range of map, reverse spawn

                    Blocked[spawnX, spawnY] = true;
                    Player.X = spawnX;
                    Player.Y = spawnY;
                    Player.Z = spawnZ;
                    Player.H = Height[spawnX, spawnY];
                    Player.goalX = -1;
                    Player.enteringGame = true;
                    j++;

                    Out.WriteLine("Assigned spawnpoint [" + spawnX + "," + spawnY + "] to player [" + j + ", team id: " + Player.teamID + "]");
                }
            }
            sendData("Cq" + Encoding.encodeVL64(-1)); // Send players to arena
            new Eucalypt.commonDelegate(countDownTicker).BeginInvoke(null, null); // Start countdown bar
        }
        /// <summary>
        /// Counts 
        /// </summary>
        private void countDownTicker()
        {
            while (leftCountdownSeconds > 0)
            {
                leftCountdownSeconds--;
                Thread.Sleep(1000);
            }
            using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
            {
                for (int i = 0; i < Teams.Length; i++)
                    foreach (gamePlayer Player in Teams[i])
                    {
                        Player.User._Tickets -= 2;
                        Player.User.sendData("A|" + Player.User._Tickets);
                        dbClient.runQuery("UPDATE users SET tickets = '" + Player.User._Tickets + "' WHERE id = '" + Player.User.userID + "' LIMIT 1");
                    }
            }
            // Start game
            updateHandler = new Thread(new ThreadStart(updateLoop));
            updateHandler.Start();
            sendData("Cw" + Encoding.encodeVL64(totalTime));
        }
        internal string getMap()
        {
            StringBuilder Setup = new StringBuilder();
            if (isBattleBall)
            {
                Setup.Append("I" + Encoding.encodeVL64(leftCountdownSeconds) + Encoding.encodeVL64(Config.Game_Countdown_Seconds) + "H" + Encoding.encodeVL64(bX) + Encoding.encodeVL64(bY));
                for (int y = 0; y < bY; y++)
                    for (int x = 0; x < bX; x++)
                        Setup.Append(Encoding.encodeVL64((int)bbTColor[x, y]) + Encoding.encodeVL64((int)bbTState[x, y]));

                Setup.Append("IH");
            }
            else
                Setup.Append("holo.cast.not_finished");

            return Setup.ToString();
        }
        internal string getPlayers()
        {
            StringBuilder Setup = null;
            StringBuilder Helper = new StringBuilder();
            Random RND = new Random(DateTime.Now.Millisecond * totalTime);
            if (isBattleBall)
            {
                int p = 0;
                for (int i = 0; i < Teams.Length; i++)
                    foreach (gamePlayer Player in Teams[i])
                    {
                        Out.WriteLine(Player.roomUID + "!");
                        Helper.Append("H" + Encoding.encodeVL64(p) + Encoding.encodeVL64(Player.X) + Encoding.encodeVL64(Player.Y) + Encoding.encodeVL64(Player.H) + Encoding.encodeVL64(Player.Z) + "HM" + Player.User._Username + Convert.ToChar(2) + Player.User._Mission + Convert.ToChar(2) + Player.User._Figure + Convert.ToChar(2) + Player.User._Sex + Convert.ToChar(2) + Encoding.encodeVL64(Player.teamID) + Encoding.encodeVL64(Player.roomUID));
                        p++;
                    }

                Setup = new StringBuilder("I" + Encoding.encodeVL64(leftCountdownSeconds) + Encoding.encodeVL64(Config.Game_Countdown_Seconds) + Encoding.encodeVL64(p));
                Setup.Append(Helper.ToString());
                Helper = null;

                Setup.Append(Encoding.encodeVL64(bX) + Encoding.encodeVL64(bY));
                for (int y = 0; y < bY; y++)
                    for (int x = 0; x < bX; x++)
                        Setup.Append(Encoding.encodeVL64((int)bbTColor[x, y]) + Encoding.encodeVL64((int)bbTState[x, y]));

                Setup.Append("IH");
            }
            else
                Setup.Append("holo.cast.not_finished");

            return Setup.ToString();
        }
        private void updateLoop()
        {
            if (isBattleBall)
            {
                int teamAmount = Teams.Length;
                int[] teamScores = new int[teamAmount];
                bool subtrSecond = false;
                int activeTeamAmount = 0;
                for (int i = 0; i < teamAmount; i++)
                {
                    if (Teams[i].Count > 0)
                        activeTeamAmount++;
                    else
                        teamScores[i] = -1;
                }

                Random RND = new Random(DateTime.Now.Millisecond * teamAmount);

                while (leftTime > 0)
                {
                    int[] Amounts = new int[3]; // [0] = unit amount, [1] = updated tile amount, [2] = moving units amount
                    StringBuilder Players = new StringBuilder();
                    StringBuilder updatedTiles = new StringBuilder();
                    StringBuilder Movements = new StringBuilder();

                    for (int i = 0; i < teamAmount; i++)
                        foreach (gamePlayer Player in Teams[i])
                        {
                            Players.Append("H" + Encoding.encodeVL64(Player.roomUID) + Encoding.encodeVL64(Player.X) + Encoding.encodeVL64(Player.Y) + Encoding.encodeVL64(Player.H) + Encoding.encodeVL64(Player.Z) + "HM");
                            if (Player.bbColorTile)
                            {
                                Player.bbColorTile = false;
                                updatedTiles.Append(Encoding.encodeVL64(Player.X) + Encoding.encodeVL64(Player.Y) + Encoding.encodeVL64((int)bbTColor[Player.X, Player.Y]) + Encoding.encodeVL64((int)bbTState[Player.X, Player.Y]));
                                Amounts[1]++;
                            }
                            if (Player.goalX != -1)
                            {
                                Coord Next = gamePathfinder.getNextStep(Player.X, Player.Y, Player.goalX, Player.goalY);
                                if (Next.X != -1 && Blocked[Next.X, Next.Y] == false)
                                {
                                    Amounts[2]++;
                                    Movements.Append("J" + Encoding.encodeVL64(Player.roomUID) + Encoding.encodeVL64(Next.X) + Encoding.encodeVL64(Next.Y));
                                    if (Next.X == Player.goalX && Next.Y == Player.goalY)
                                        Player.goalX = -1;

                                    Blocked[Player.X, Player.Y] = false;
                                    Blocked[Next.X, Next.Y] = true;
                                    Player.Z = Rotation.Calculate(Player.X, Player.Y, Next.X, Next.Y);

                                    Player.X = Next.X;
                                    Player.Y = Next.Y;
                                    Player.H = Height[Next.X, Next.Y];

                                    if (bbTState[Next.X, Next.Y] != bbTileState.Sealed)
                                    {
                                        if (bbTColor[Next.X, Next.Y] == (bbTileColor)Player.teamID) // Already property of this team, increment the state
                                            bbTState[Next.X, Next.Y]++;
                                        else
                                        {
                                            bbTState[Next.X, Next.Y] = bbTileState.Touched;
                                            bbTColor[Next.X, Next.Y] = (bbTileColor)Player.teamID;
                                        }

                                        int extraPoints = 0;
                                        switch ((int)bbTState[Next.X, Next.Y]) // Check the new status and calculate extra points
                                        {
                                            case 1:
                                                extraPoints = activeTeamAmount;
                                                break;

                                            case 2:
                                                extraPoints = activeTeamAmount * 3;
                                                break;

                                            case 3:
                                                extraPoints = activeTeamAmount * 5;
                                                break;

                                            case 4:
                                                extraPoints = activeTeamAmount * 7;
                                                break;
                                        }
                                        teamScores[Player.teamID] += extraPoints;
                                        Player.Score += extraPoints;
                                        Player.bbColorTile = true;
                                    }
                                }
                                else
                                    Player.goalX = -1; // Stop moving
                            }
                            Amounts[0]++;
                        }

                    string scoreString = "H" + Encoding.encodeVL64(teamAmount);
                    for (int i = 0; i < teamAmount; i++)
                        scoreString += Encoding.encodeVL64(teamScores[i]);

                    sendData("Ct" + Encoding.encodeVL64(Amounts[0]) + Players.ToString() + Encoding.encodeVL64(Amounts[1]) + updatedTiles.ToString() + scoreString + "I" + Encoding.encodeVL64(Amounts[2]) + Movements.ToString());

                    if (subtrSecond)
                        leftTime--;
                    subtrSecond = (subtrSecond != true);
                    Thread.Sleep(470);
                }
                State = gameState.Ended;

                StringBuilder Scores = new StringBuilder("Cx" + Encoding.encodeVL64(Config.Game_scoreWindow_restartGame_Seconds) + Encoding.encodeVL64(teamAmount));
               
                for (int i = 0; i < teamAmount; i++)
                {
                    int memberAmount = Teams[i].Count;
                    if (memberAmount > 0)
                    {
                        Scores.Append(Encoding.encodeVL64(memberAmount));
                        using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
                        {
                            foreach (gamePlayer Player in Teams[i])
                            {
                                Scores.Append(Encoding.encodeVL64(Player.roomUID));
                                if (Player.User != null)
                                {

                                    dbClient.runQuery("UPDATE users SET bb_playedgames = bb_playedgames + 1,bb_totalpoints = bb_totalpoints + " + Player.Score + " WHERE id = '" + Player.User.userID + "' LIMIT 1");

                                    Scores.Append(Player.User._Username + Convert.ToChar(2));
                                }
                                else
                                    Scores.Append("M");
                                Scores.Append(Encoding.encodeVL64(Player.Score));
                            }
                        }
                        Scores.Append(Encoding.encodeVL64(teamScores[i]));
                    }
                    else
                        Scores.Append("M");
                }
                sendData(Scores.ToString());
            }
        }
        private Coord[] getSurfaceTiles(Coord Start)
        {
            return null;
        }
        #endregion
    }
}
