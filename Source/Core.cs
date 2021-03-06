﻿using System;
using System.Threading;

using Holo.Managers;
using Holo.Socketservers;
using Microsoft.VisualBasic;
using Holo.Source.Managers;
using Ion.Storage;

namespace Holo
{
    /// <summary>
    /// The core of Holograph Emulator codename "Eucalypt", contains Main() void for booting server, plus monitoring thread and shutdown void.
    /// </summary>
    public class Eucalypt
    {
        private static Thread serverMonitor = new Thread(new ThreadStart(monitorServer));
        public delegate void commonDelegate();

        public static string serverVersion = "Dissi Total DB Pooling";

        public static int gameMaxConnections;
        public static string dbHost;
        public static uint dbPort;
        public static string dbUsername;
        public static string dbPassword;
        public static string dbName;
        public static uint dbMaxConnections;
        public static int dbPool;
        public static string habboVersion;
        public static DatabaseManager dbManager;
        public static Ion.Storage.Database database;

        public static int serverInviteID;
        public static int serverInviteSenderID;
        public static string serverInviteName;
        public static int serverInviteAccepted;
        public static int serverInviteAnswered;
        public static int serverInviteSendTo;
        /// <summary>
        /// Starts up Holograph Emulator codename "Eucalypt".
        /// </summary>
        private static void Main()
        {
            Console.WindowHeight = Console.LargestWindowHeight - 25;
            Console.WindowWidth = Console.LargestWindowWidth - 25;
            Console.CursorVisible = false;
            Console.Title = "Holo TDbP EXTREME edition - Dissi\'s edit";
            Out.WritePlain(serverVersion);





            Boot();

            //while (true) // Infinite loop, keeping console window open and rejecting all input
            //    Console.ReadKey(true);

        }
        /// <summary>
        /// Boots the emulator.
        /// </summary>
        private static void Boot()
        {
            ThreadPool.SetMaxThreads(300, 400);
            DateTime _START = DateTime.Now;
            //Out.WriteLine("Starting up Holograph Emulator for " + Environment.UserName + "...");
            string sqlConfigLocation = IO.workingDirectory + @"\bin\mysql.ini";
            if (System.IO.File.Exists(sqlConfigLocation) == false)
            {
                Out.WriteError("mysql.ini not found at " + sqlConfigLocation);
                Shutdown();
                return;
            }

            //Out.WriteLine("mysql.ini found at " + sqlConfigLocation);
            Out.WriteBlank();

            dbHost = IO.readINI("mysql", "host", sqlConfigLocation);
            dbPort = uint.Parse(IO.readINI("mysql", "port", sqlConfigLocation));
            dbUsername = IO.readINI("mysql", "username", sqlConfigLocation);
            dbPassword = IO.readINI("mysql", "password", sqlConfigLocation);
            dbName = IO.readINI("mysql", "database", sqlConfigLocation);
            habboVersion = "r26";
            dbMaxConnections = uint.Parse(IO.readINI("mysql", "clientamount", sqlConfigLocation));

            Out.WriteBlank();
            dbManager = new DatabaseManager(dbHost, dbPort, dbUsername, dbPassword, dbName, 1, 100);
            dbManager.SetClientAmount(dbMaxConnections);
            dbManager.StartMonitor();
            int gamePort;
            int gameMaxConnections;
            int musPort;
            int musMaxConnections;
            string musHost;

            try
            {
                gamePort = int.Parse(Config.getTableEntry("server_game_port"));
                gameMaxConnections = int.Parse(Config.getTableEntry("server_game_maxconnections"));
                musPort = int.Parse(Config.getTableEntry("server_mus_port"));
                musMaxConnections = int.Parse(Config.getTableEntry("server_mus_maxconnections"));
                musHost = Config.getTableEntry("server_mus_host");
            }
            catch
            {
                Out.WriteError("system_config table contains invalid values for socket server configuration!");
                Shutdown();
                return;
            }

            string langExt = Config.getTableEntry("lang");
            if (langExt == "")
            {
                Out.WriteError("No valid language extension [field: lang] was set in the system_config table!");
                Shutdown();
                return;
            }

            stringManager.Init(langExt, false);
            Out.WriteBlank();

            stringManager.initFilter(false);
            Out.WriteBlank();


            catalogueManager.Init(false);
            Out.WriteBlank();

            navigatorManager.Init();
            Out.WriteBlank();

            //buddyManager.init();
            Out.WriteBlank();

            recyclerManager.Init(false);
            Out.WriteBlank();

            rankManager.Init(false);
            Out.WriteBlank();

            Config.Init(false);
            Out.WriteBlank();

            userManager.Init();
            eventManager.Init();

            if (gameSocketServer.Init(gamePort, gameMaxConnections) == false)
            {
                Shutdown();
                return;
            }
            Out.WriteBlank();

            if (musSocketServer.Init(musPort, musHost) == false)
            {
                Shutdown();
                return;
            }
            Out.WriteBlank();

            resetDynamics();

            printDatabaseStats();
            Out.WriteBlank();

            DateTime _STOP = DateTime.Now;
            TimeSpan _TST = _STOP - _START;
            Out.WriteLine("Total DB Pooling >> MySQL Net Connector 5.2.5 >> Hotel Emulator - Startup Time: " + _TST.TotalMilliseconds.ToString() + ".");

            GC.Collect();
            //Out.WriteLine("Holo TDbP EXTREME edition Hotel Emulator - Ready!");
            Out.WriteBlank();
            string date = DateAndTime.Now.ToString();
            //Out.WritePlain(date);

            Out.minimumImportance = Out.logFlags.MehAction; // All logs
            serverMonitor.Priority = ThreadPriority.Lowest;
            serverMonitor.Start();
        }
        /// <summary>
        /// Safely shutdowns Holograph Emulator, closing database and socket connections. Requires key press from user for final abort.
        /// </summary>
        public static void Shutdown()
        {
            Out.WriteBlank();
            if (serverMonitor.IsAlive)
                serverMonitor.Abort();
            Out.WriteLine("Holo TDbP EXTREME edition Emulator Shutdown!");
            Console.ReadKey(true);
            Console.Beep(1400, 1000);
            Environment.Exit(2);
        }
        /// <summary>
        /// Prints the usercount, guestroomcount and furniturecount in datebase to console.
        /// </summary>
        private static void printDatabaseStats()
        {
            int userCount;
            int roomCount;
            int itemCount;
            using (DatabaseClient dbClient = dbManager.GetClient())
            {
                userCount = dbClient.getInt("SELECT COUNT(*) FROM users");
                roomCount = dbClient.getInt("SELECT COUNT(*) FROM rooms");
                itemCount = dbClient.getInt("SELECT COUNT(*) FROM furniture");
            }
            Out.WriteLine("Result: " + userCount + " users, " + roomCount + " rooms and " + itemCount + " furnitures.");
        }
        private static void resetDynamics()
        {
            int maxOnline = 0;
            using (DatabaseClient dbClient = dbManager.GetClient())
            {
                maxOnline = dbClient.getInt("SELECT onlinecount_peak FROM system");
                dbClient.runQuery("UPDATE system SET onlinecount = '0',onlinecount_peak = '" + maxOnline + "',connections_accepted = '0',activerooms = '0'");
                dbClient.runQuery("UPDATE users SET ticket_sso = NULL");
                dbClient.runQuery("UPDATE rooms SET visitors_now = '0'");
            }

            Out.WriteLine("Reset connections");
            Out.WriteLine("Reset Room/User counters");
            Out.WriteLine("Reset SSO Tickets");
        }
        /// <summary>
        /// Threaded void. Ran on background thread at lowest priority, interval = 10000 ms. Updates console title and online users count, active rooms count, peak connections count and peak online users count in database.
        /// </summary>
        private static void monitorServer()
        {
            while (true)
            {
                int onlineCount = userManager.userCount;
                int peakOnlineCount = userManager.peakUserCount;
                int roomCount = roomManager.roomCount;
                int peakRoomCount = roomManager.peakRoomCount;
                //int databaseConnections =
                int starvationNumber = dbManager.getStarvationNumber();
                int acceptedConnections = gameSocketServer.acceptedConnections;
                long memUsage = GC.GetTotalMemory(false) / 1024;
                Console.Title = "Connected Users: " + onlineCount + " | Loaded Rooms: " + roomCount + " | RAM usage: " + memUsage + "KB | Max Connections: " + peakOnlineCount + " | SQL Starvation: " + starvationNumber + " | SQL Connections: " + dbManager.databaseClients + " | Habbo Version " + habboVersion;
                using (DatabaseClient dbClient = dbManager.GetClient())
                {
                    dbClient.runQuery("UPDATE system SET onlinecount = '" + onlineCount + "',onlinecount_peak = '" + peakOnlineCount + "',activerooms = '" + roomCount + "',activerooms_peak = '" + peakRoomCount + "',connections_accepted = '" + acceptedConnections + "'");
                }
                Thread.Sleep(10000);
                //Out.WritePlain("Servermonitor loop");
            }
        }
    }
}