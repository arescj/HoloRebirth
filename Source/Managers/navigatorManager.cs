using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Holo;



using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Holo.Managers;
using Ion.Storage;

namespace Holo.Source.Managers
{
    public static class navigatorManager
    {
        //public List<>;

        public static Dictionary<string, DataTable> guestRooms;
        public static Dictionary<string, string> roomsInsideCatagories;
        public static Dictionary<int, int> type;
        public static Dictionary<int, int> parent;
        public static Dictionary<int, string> name;
        public static Dictionary<string, string> roomAccesName;
        public static Dictionary<string, DataColumn> roomAccesParent;
        private static Thread randomizerThread;
        private static Thread roomAccesSort;
        private static Thread reNewRooms;
        private static Thread updateInRooms;
        private static string Rooms;
        private static Random random = new Random();
        /// <summary>
        /// Initializes the navigtor
        /// </summary>
        public static void Init()
        {
            type = new Dictionary<int, int>();
            parent = new Dictionary<int, int>();
            name = new Dictionary<int, string>();
            roomAccesName = new Dictionary<string, string>();
            roomAccesParent = new Dictionary<string, DataColumn>();
            roomsInsideCatagories = new Dictionary<string, string>();
            guestRooms = new Dictionary<string, DataTable>();
            DataTable dTable;
            using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
            {
                dTable = dbClient.getTable("SELECT id , type FROM room_categories");
            }
            foreach (DataRow dRow in dTable.Rows)
            {
                type.Add(Convert.ToInt32(dRow[0]), Convert.ToInt32(dRow[1]));
            }
            using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
            {
                dTable = dbClient.getTable("SELECT id, parent FROM room_categories");
            }
            foreach (DataRow dRow in dTable.Rows)
            {
                parent.Add(Convert.ToInt32(dRow[0]), Convert.ToInt32(dRow[1]));
            }
            using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
            {
                dTable = dbClient.getTable("SELECT id, name FROM room_categories");
            }

            foreach (DataRow dRow in dTable.Rows)
            {
                name.Add(Convert.ToInt32(dRow[0]), Convert.ToString(dRow[1]));
            }

            ThreadStart roomAccesThread = new ThreadStart(roomAcces);
            roomAccesSort = new Thread(roomAccesThread);
            roomAccesSort.Priority = ThreadPriority.BelowNormal;
            roomAccesSort.Start();

            ThreadStart guestRoomRefresh = new ThreadStart(reNewRoomsThread);
            reNewRooms = new Thread(guestRoomRefresh);
            reNewRooms.Priority = ThreadPriority.BelowNormal;
            reNewRooms.Name = "Updates guestrooms catagories";
            reNewRooms.Start();

            ThreadStart randoms = new ThreadStart(randomRooms);
            randomizerThread = new Thread(randoms);
            randomizerThread.Priority = ThreadPriority.Lowest;
            randomizerThread.Name = "Update Random Rooms";
            randomizerThread.Start();

            ThreadStart guestRoomsInroom = new ThreadStart(refreshGuestrooms);
            updateInRooms = new Thread(guestRoomsInroom);
            updateInRooms.Priority = ThreadPriority.Lowest;
            updateInRooms.Name = "Update In Rooms";
            updateInRooms.Start();

            Out.WriteLine("Navigator names cached: " + name.Count);
            Out.WriteLine("Navigator \"Parent id's\" cached: " + parent.Count);
            Out.WriteLine("Navigator type's cached: " + type.Count);
        }


        /// <summary>
        /// returns the name of the catagory with the given ID
        /// </summary>
        public static string getName(int id)
        {
            return name[id];
        }

        /// <summary>
        /// hets the type of thr room with the given id
        /// </summary>
        public static int getType(int id)
        {
            return type[id];
        }


        /// <summary>
        /// returns the ID of the parent
        /// </summary>
        public static int getParent(int id)
        {
            return parent[id];
        }


        /// <summary>
        /// Gets the name of the acces rank returns "" if the user hasn't got the rank to acces it
        /// </summary>
        /// <param name="rank">Rank of the user</param>
        /// <param name="id">ID of room</param>
        /// <returns>Name of the room</returns>
        public static string getNameAcces(int rank, int id)
        {
            if (roomAccesName.ContainsKey(rank.ToString() + id.ToString()))
                return roomAccesName[rank.ToString() + id.ToString()];
            else
                return "";
        }


        /// <summary>
        /// Gets a Parent of the Database, or from the local collection
        /// </summary>
        /// <param name="rank">Rank of the user</param>
        /// <param name="cataid">Index of the catalogus</param>
        /// <returns></returns>
        public static DataColumn getAccesParent(int rank, int cataid)
        {

            if (roomAccesParent.ContainsKey(rank.ToString() + cataid.ToString()))
            {
                return roomAccesParent[rank.ToString() + cataid.ToString()];
            }
            else
            {
                DataColumn dCol;
                using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
                {
                    dCol = dbClient.getColumn("SELECT id FROM room_categories WHERE parent = '" + cataid + "' AND (access_rank_min <= " + rank + " OR access_rank_hideforlower = '0') ORDER BY id ASC");
                }
                roomAccesParent.Add(rank.ToString() + cataid.ToString(), dCol);
                return dCol;
            }
        }






        /// <summary>
        /// gets a new guestroom which havn't been queried yet
        /// </summary>
        /// <param name="query">the query name</param>
        /// <param name="update">Does it need an update or not (only true in thread updateGuestrooms)</param>
        /// <returns></returns>
        public static DataTable getGuestroomQuery(string query, bool update)
        {
            if (!update)
            {

                if (guestRooms.ContainsKey(query))
                {
                    return guestRooms[query];
                }
                else
                {
                    DataTable dTable;
                    using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
                    {
                        dTable = dbClient.getTable(query);
                    }
                    guestRooms.Add(query, dTable);
                    return guestRooms[query];
                }
            }
            else
            {
                if (guestRooms.ContainsKey(query))
                {
                    DataTable dTable;
                    using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
                    {
                        dTable = dbClient.getTable(query);
                    }
                    guestRooms.Remove(query);
                    guestRooms.Add(query, dTable);
                    return guestRooms[query];
                }
                else
                {
                    DataTable dTable;
                    using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
                    {
                        dTable = dbClient.getTable(query);
                    }
                    guestRooms.Add(query, dTable);
                    return guestRooms[query];
                }
            }
        }
        public static string getRandomRooms()
        {
            return Rooms;
        }
        /// <summary>
        /// refreshes room acces every 40 seconds
        /// </summary>
        private static void roomAcces()
        {
            //int i = 0;
            DataTable dTable;
            //DataColumn dcol;


            while (true)
            {
                for (int i = 0; i <= 7; i++)
                {
                    using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
                    {
                        dTable = dbClient.getTable("SELECT id, name FROM room_categories WHERE (access_rank_min = " + i + " OR access_rank_hideforlower = '0')");
                    }
                    foreach (DataRow dRow in dTable.Rows)
                    {
                        try
                        {
                            roomAccesName.Add(i + (Convert.ToString(dRow[0])), Convert.ToString(dRow[1]));
                        }
                        catch (Exception e) { Out.WriteError("error: " + e.ToString()); }
                    }
                }
                Thread.Sleep(400000 + random.Next(10000));
                roomAccesName.Clear();
            }


        }

        /// <summary>
        /// refreshes random rooms every 30 seconds
        /// </summary>
        private static void randomRooms()
        {
            DataRow dRow;
            int nummer;
            int i;
            int randomNummer;
            int minimum;
            using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
            {
                minimum = dbClient.getInt("SELECT min(id) FROM rooms");
            }
            while (true)
            {
                i = 0;
                Rooms = "";
                bool findResult = false;
                using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
                {

                    nummer = dbClient.getInt("SELECT max(id)FROM rooms ");
                }
                while (i < 3)
                {
                    randomNummer = random.Next(minimum, nummer);
                    using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
                    {
                        findResult = dbClient.findsResult("SELECT id FROM rooms WHERE id = " + randomNummer);
                    }
                    if (findResult && (randomNummer > 0))
                    {
                        using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
                        {
                            dRow = dbClient.getRow("SELECT id,name,owner,description,state,visitors_now,visitors_max FROM rooms WHERE id = " + randomNummer);
                        }
                        Rooms += Encoding.encodeVL64(Convert.ToInt32(dRow["id"])) + Convert.ToString(dRow["name"]) + Convert.ToChar(2) + Convert.ToString(dRow["owner"]) + Convert.ToChar(2) + roomManager.getRoomState(Convert.ToInt32(dRow["state"])) + Convert.ToChar(2) + Encoding.encodeVL64(Convert.ToInt32(dRow["visitors_now"])) + Encoding.encodeVL64(Convert.ToInt32(dRow["visitors_max"])) + Convert.ToString(dRow["description"]) + Convert.ToChar(2);
                        i++;
                    }
                }

                Thread.Sleep(30000);
            }

        }


        private static void reNewRoomsThread()
        {
            List<string> toRenew = new List<string>();
            while (true)
            {
                lock (guestRooms)
                {
                    foreach (KeyValuePair<string, DataTable> var in guestRooms)
                    {
                        toRenew.Add(var.Key);
                    }
                }
                foreach (string s in toRenew)
                {
                    getGuestroomQuery(s, true);
                    Thread.Sleep(random.Next(1000));
                }
                toRenew.Clear();
                Thread.Sleep(30000 + random.Next(10000));
            }
        }


        private static void refreshGuestrooms()
        {
            List<string> roomToRenew = new List<string>();
            while (true)
            {
                lock (roomsInsideCatagories)
                {
                    foreach (KeyValuePair<String, String> var in roomsInsideCatagories)
                    {
                        roomToRenew.Add(var.Key);
                    }
                }
                foreach (string s in roomToRenew)
                {
                    //getGuestRoomCatagoryID(s, true);
                }
                roomToRenew.Clear();
                Thread.Sleep(20000 + random.Next(5000));
            }
        }
    }

}
