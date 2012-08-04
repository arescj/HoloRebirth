using System;
using System.Data;
using System.Text;
using System.Collections;
using System.Threading;
using Ion.Storage;

namespace Holo.Managers
{
    /// <summary>
    /// Provides information about the various user ranks/levels, aswell as ranks for games such as 'BattleBall' and 'SnowStorm'.
    /// </summary>
    public static class rankManager
    {
        private static Hashtable userRanks;
        private static gameRank[] gameRanksBB;
        private static gameRank[] gameRanksSS;
        private static gameRank[] gameRanksWS;

        /// <summary>
        /// Initializes the various user ranks, aswell as the ranks for games such as 'BattleBall' and 'SnowStorm'.
        /// </summary>
        public static void Init(bool Update)
        {
           // Out.WriteLine("Intializing user rank fuserights...");
            userRanks = new Hashtable();

            for (byte b = 1; b <= 10; b++)
                userRanks.Add(b, new userRank(b));

            Out.WriteLine("Fuserights for 10 ranks loaded.");
            Out.WriteBlank();

           // Out.WriteLine("Initializing game ranks...");

            DataTable dTable;
            using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
            {
                dTable = dbClient.getTable("SELECT title, minpoints, maxpoints FROM games_ranks WHERE type = 'bb' ORDER BY id ASC");
            }
            gameRanksBB = new gameRank[dTable.Rows.Count];
            int i = 0;
            foreach (DataRow dRow in dTable.Rows)
            {
                gameRanksBB[i] = new gameRank(Convert.ToString(dRow["title"]), Convert.ToInt32(dRow["minpoints"]), Convert.ToInt32(dRow["maxpoints"]));
                i++;
            }
            Out.WriteLine("Loaded " + gameRanksBB.Length + " ranks for game 'BattleBall'.");
            using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
            {
            //    dTable = dbClient.getTable("SELECT title, minpoints, maxpoints FROM games_ranks WHERE type = 'ss' ORDER BY id ASC");
            }
            
            gameRanksSS = new gameRank[dTable.Rows.Count];
            i = 0;
            foreach (DataRow dRow in dTable.Rows)
            {
                gameRanksSS[i] = new gameRank(Convert.ToString(dRow["title"]), Convert.ToInt32(dRow["minpoints"]), Convert.ToInt32(dRow["maxpoints"]));
                i++;
            }
            //Out.WriteLine("Loaded " + gameRanksSS.Length + " ranks for game 'SnowStorm'.");
            using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
            {
                dTable = dbClient.getTable("SELECT title, minpoints, maxpoints FROM games_ranks WHERE type = 'ws' ORDER BY id ASC");
            }
            gameRanksWS = new gameRank[dTable.Rows.Count];
            i = 0;
            foreach (DataRow dRow in dTable.Rows)
            {
                gameRanksWS[i] = new gameRank(Convert.ToString(dRow["title"]), Convert.ToInt32(dRow["minpoints"]), Convert.ToInt32(dRow["maxpoints"]));
                i++;
            }
            //fOut.WriteLine("Loaded " + gameRanksWS.Length + " ranks for game 'Wobble Squabble'.");

            if (Update)
                Thread.CurrentThread.Abort();
        }
        /// <summary>
        /// Returns the fuserights string for a certain user rank.
        /// </summary>
        /// <param name="rankID">The ID of the user rank.</param>
        /// <param name="userID">The ID of the user.</param>
        public static string fuseRights(byte rankID, int userID)
        {
            string[] fuseRights = ((userRank)userRanks[rankID]).fuseRights;
            StringBuilder strBuilder = new StringBuilder();

            for (int i = 0; i < fuseRights.Length; i++)
                strBuilder.Append(fuseRights[i] + Convert.ToChar(2));

            foreach (string fuseright in userManager.getUser(userID)._fuserights)
                strBuilder.Append(fuseright + Convert.ToChar(2));

            return strBuilder.ToString();
        }

        /// <summary>
        /// Returns a bool that indicates if a certain user has a certain fuseright.
        /// </summary>
        /// <param name="rankID">The ID of the user rank.</param>
        /// <param name="fuseRight">The fuseright to look for.</param>
        /// <param name="userID">The ID of the user.</param>
        public static bool containsRight(byte rankID, string fuseRight, int userID)
        {
            userRank objRank = ((userRank)userRanks[rankID]);
            for (int i = 0; i < objRank.fuseRights.Length; i++)
                if (objRank.fuseRights[i] == fuseRight)
                    return true;
            return userManager.getUser(userID)._fuserights.Contains(fuseRight);
        }

        /// <summary>
        /// Returns a bool that indicates if a certain user has a certain fuseright.
        /// </summary>
        /// <param name="rankID">The ID of the user rank.</param>
        /// <param name="fuseRight">The fuseright to look for.</param>
        /// <returns></returns>
        public static bool containsRankRight(byte rankID, string fuseRight)
        {
            userRank objRank = ((userRank)userRanks[rankID]);
            for (int i = 0; i < objRank.fuseRights.Length; i++)
                if (objRank.fuseRights[i] == fuseRight)
                    return true;
            return false;
        }
        
        public static gameRank getGameRank(bool isBattleBall, string Title)
        {
            gameRank[] Ranks = null;
            if (isBattleBall)
                Ranks = gameRanksBB;
            else
                Ranks = gameRanksSS;

            foreach (gameRank Rank in Ranks)
                if (Rank.Title == Title)
                    return Rank;

            return new gameRank("holo.cast.gamerank.null", 0, 0);
        }
        /// <summary>
        /// Returns the game rank title as a string for a certain game type ('BattleBall' or 'SnowStorm') and score.
        /// </summary>
        /// <param name="isBattleBall">Specifies if to lookup a 'BattleBall' game. If false, then the rank for a 'SnowStorm' game will be returned.</param>
        /// <param name="Score">The score to get the rank for.</param>
        public static string getGameRankTitle(bool isBattleBall, int Score)
        {
            gameRank[] Ranks = null;
            if (isBattleBall)
                Ranks = gameRanksBB;
            else
                Ranks = gameRanksSS;

            foreach (gameRank Rank in Ranks)
                if (Score >= Rank.minPoints && (Rank.maxPoints == 0 || Score <= Rank.maxPoints))
                    return Rank.Title;

            return "holo.cast.gamerank.null";
        }
        /// <summary>
        /// Represents a user rank.
        /// </summary>
        private struct userRank
        {
            internal string[] fuseRights;
            /// <summary>
            /// Initializes a user rank.
            /// </summary>
            /// <param name="rankID">The ID of the rank to initialize.</param>
            internal userRank(byte rankID)
            {
                DataColumn dCol;
                using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
                {
                    dCol = dbClient.getColumn("SELECT fuseright FROM system_fuserights WHERE minrank <= " + rankID);
                }
                fuseRights = dataHandling.dColToArray(dCol);
            }
        }
        /// <summary>
        /// Represents a game rank, containing the min and max score and the rank name.
        /// </summary>
        public struct gameRank
        {
            /// <summary>
            /// The minimum amount of points of this rank.
            /// </summary>
            internal int minPoints;
            /// <summary>
            /// The maximum amount of points of this rank.
            /// </summary>
            internal int maxPoints;
            /// <summary>
            /// The title of this rank.
            /// </summary>
            internal string Title;
            /// <summary>
            /// Initializes the gamerank with given values.
            /// </summary>
            /// <param name="Title">The title of this rank.</param>
            /// <param name="minPoints">The minimum amount of points of this rank.</param>
            /// <param name="maxPoints">The maximum amount of points of this rank.</param>
            internal gameRank(string Title, int minPoints, int maxPoints)
            {
                this.Title = Title;
                this.minPoints = minPoints;
                this.maxPoints = maxPoints;
            }
        }
    }
}
