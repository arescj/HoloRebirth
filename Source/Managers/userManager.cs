using System;
using System.Data;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

using Holo.Virtual.Users;
using Ion.Storage;

namespace Holo.Managers
{
    /// <summary>
    /// Provides management for logged in users, aswell for retrieving details such as ID/name and vice versa from the database.
    /// </summary>
    public static class userManager
    {
        public static Hashtable _Users = new Hashtable();
        private static Thread pingChecker;
        private static int _peakUserCount;


        /// <summary>
        /// Starts the pingchecker thread.
        /// </summary>
        public static void Init()
        {
            using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
            {
                _peakUserCount = dbClient.getInt("SELECT onlinecount_peak FROM system");
            }
            try { pingChecker.Abort(); }
            catch { }
            pingChecker = new Thread(new ThreadStart(checkPings));
            pingChecker.Priority = ThreadPriority.Lowest;
            pingChecker.Start();
        }
        /// <summary>
        /// Adds a virtualUser class together with the userID to the userManager. Login ticket will be nulled and previous logged in instances of this user will be dropped.
        /// </summary>
        /// <param name="userID">The ID of the user to add.</param>
        /// <param name="User">The virtualUser class of this user.</param>
        public static void addUser(int userID, virtualUser User)
        {
            if (_Users.ContainsKey(userID))
            {
                virtualUser oldUser = ((virtualUser)_Users[userID]);
                oldUser.Disconnect();
                if (_Users.ContainsKey(userID))
                    _Users.Remove(userID);
            }
            using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
            {
                dbClient.AddParamWithValue("name", User._Username);
                if (User.connectionRemoteIP == dbClient.getString("SELECT ipaddress_last FROM users WHERE name = @name LIMIT 1"))
                {
                    _Users.Add(userID, User);
                    dbClient.runQuery("UPDATE users SET ticket_sso = NULL WHERE id = '" + userID + "' LIMIT 1");
                    Out.WriteLine("Username " + User._Username + " logged in. [ " + userID + " ]");
                }

                else
                {
                    User.Disconnect(1000);
                    User.sendData("BK" + "Invalid Session Ticket, please use the SSO Fix!");
                }

                if (_Users.Count > dbClient.getInt("SELECT onlinecount_peak FROM system"))
                {
                    _peakUserCount = _Users.Count;
                }

            }
        }

        /// <summary>
        /// Removes a user from the userManager. [if it exists]
        /// </summary>
        /// <param name="userID">The ID of the user to remove.</param>
        public static void removeUser(int userID)
        {
            if (_Users.ContainsKey(userID))
            {
                _Users.Remove(userID);
                //Out.WriteLine("User [" + userID + "] disconnected.", Out.logFlags.BelowStandardAction);
            }
        }
        /// <summary>
        /// Returns a bool that indicates if the userManager contains a certain user.
        /// </summary>
        /// <param name="userID">The ID of the user.</param>
        public static bool containsUser(int userID)
        {
            return _Users.ContainsKey(userID);
        }
        /// <summary>
        /// Returns a bool that indicates if the userManager contains a certain user.
        /// </summary>
        /// <param name="userName">The username of the user.</param>
        public static bool containsUser(string userName)
        {
            int userID = getUserID(userName);
            return _Users.ContainsKey(userID);
        }

        /// <summary>
        /// Returns the current amount of users in the userManager.
        /// </summary>
        public static int userCount
        {
            get
            {
                return _Users.Count;
            }
        }
        /// <summary>
        /// Returns the peak amount of users in the userManager since boot.
        /// </summary>
        public static int peakUserCount
        {
            get
            {
                return _peakUserCount;
            }
        }

        /// <summary>
        /// Retrieves the ID of a user from the database.
        /// </summary>
        /// <param name="userName">The username of the user.</param>
        public static int getUserID(string userName)
        {
            using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
            {
                dbClient.AddParamWithValue("name", userName);
                return dbClient.getInt("SELECT id FROM users WHERE name = @name LIMIT 1");
            }
            
        }
        /// <summary>
        /// Retrieves the username of a user from the database.
        /// </summary>
        /// <param name="userID">The ID of the user.</param>
        public static string getUserName(int userID)
        {
            using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
            {
                return dbClient.getString("SELECT name FROM users WHERE id = '" + userID + "' LIMIT 1");
            }
        }
        /// <summary>
        /// Returns a bool that indicates if a user with a certain user ID exists in the database.
        /// </summary>
        /// <param name="userID">The ID of the user to check.</param>
        public static bool userExists(int userID)
        {
            using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
            {
                return dbClient.findsResult("SELECT id FROM users WHERE id = '" + userID + "'");
            }
        }

        /// <summary>
        /// Returns an int array with the ID's of the messenger friends of a certain user.
        /// </summary>
        /// <param name="userID">The ID of the user to get the friend ID's from.</param>
        public static int[] getUserFriendIDs(int userID)
        {
            using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
            {
                try
                {
                    ArrayList idBuilder = new ArrayList();
                    DataColumn dCol = dbClient.getColumn("SELECT friendid FROM messenger_friendships WHERE userid = '" + userID + "'");
                    int[] friendIDs = dataHandling.dColToArray(dCol, null);
                    foreach (int i in friendIDs)
                        idBuilder.Add(i);
                    dCol = dbClient.getColumn("SELECT userid FROM messenger_friendships WHERE friendid = '" + userID + "'");

                    friendIDs = dataHandling.dColToArray(dCol, null);
                    foreach (int i in friendIDs)
                        idBuilder.Add(i);

                    return (int[])idBuilder.ToArray(typeof(int));
                }
                catch
                {
                    return new int[0];
                }
            }
        }
        /// <summary>
        /// Returns a virtualUser class for a certain user
        /// </summary>
        /// <param name="userID">The ID of the user.</param>
        public static virtualUser getUser(int userID)
        {
            try { return (virtualUser)_Users[userID]; }
            catch { return null; }
        }
        /// <summary>
        /// Returns a virtualUser class for a certain user.
        /// </summary>
        /// <param name="userName">The username of the user.</param>
        public static virtualUser getUser(string userName)
        {
            int userID = getUserID(userName);
            return getUser(userID);
        }
        /// <summary>
        /// Sends a single packet to all connected clients.
        /// </summary>
        /// <param name="Data">The packet to send.</param>
        public static void sendData(string Data)
        {
            try
            {
                Hashtable Users = (Hashtable)_Users.Clone();
                foreach (virtualUser User in Users.Values)
                {
                    try { User.sendData(Data); }
                    catch { Out.WritePlain("Username " + User._Username + " is currently off of Echo Hotel."); }
                }
             }
            catch { }
        }
        /// <summary>
        /// Sends a single packet to all active virtual users with the specified rank. Optionally you can include users who have a higher rank than the specified rank.
        /// </summary>
        /// <param name="Rank">The minimum rank that the virtual user required to receive the data.</param>
        /// <param name="includeHigher">Indicates if virtual users with a rank that's higher than the specified rank should also receive the packet.</param>
        /// <param name="Data">The packet to send.</param>
        public static void sendToRank(byte Rank, bool includeHigher, string Data)
        {
            try
            {
                {
                    foreach (virtualUser User in _Users.Values)
                    {
                        if (User._Rank < Rank || (includeHigher == false && User._Rank > Rank))
                            continue;
                        else
                            User.sendData(Data);
                    }
                }
            }
            catch { }
        }
        /// <summary>
        /// Inserts a single 'chat saying' to the system_chatlog table, together with username of sayer, room ID of sayer and the current timestamp.
        /// </summary>
        /// <param name="userName">The username of the sayer.</param>
        /// <param name="roomID">The ID of the room where the sayer is in.</param>
        /// <param name="Message">The message the sayer said.</param>
        public static void addChatMessage(string userName, int roomID, string Message)
        {
            using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
            {
                dbClient.AddParamWithValue("username", userName);
                dbClient.AddParamWithValue("roomid", roomID);
                dbClient.AddParamWithValue("message", Message);
                //dbClient.runQuery("INSERT INTO system_chatlog (username,roomid,mtime,message) VALUES (@username,@roomid,CURRENT_TIMESTAMP,@message)");
            }
        }
        /// <summary>
        /// Generates an info list about a certain user. If the user isn't found or has a higher rank than the info requesting user, then an access error message is returned. Otherwise, a report with user ID, username, rank, mission, credits amount, tickets amount, virtual birthday (signup date), real birthday, email address and last IP address. If the user is online, then information about the room the user currently is in (including ID and owner name) is supplied, otherwise, the last server access date is supplied.
        /// </summary>
        /// <param name="userID">The database ID of the user to generate the info of.</param>
        /// <param name="Rank">The rank of the user that requests this info. If this rank is lower than the rank of the target user, then there is no info returned.</param>
        public static string generateUserInfo(int userID, byte Rank)
        {
            DataRow dRow;
            using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
            {
                dRow = dbClient.getRow("SELECT name,rank,mission,credits,tickets,email,birth,hbirth,ipaddress_last,lastvisit FROM users WHERE id = '" + userID + "' AND rank <= " + Rank);
            }
            if (dRow.Table.Rows.Count == 0)
            {
                return stringManager.getString("userinfo_accesserror");
            }
            else
            {
                StringBuilder Info = new StringBuilder(stringManager.getString("userinfo_header") + "\r"); // Append header
                Info.Append(stringManager.getString("common_userid") + ": " + userID + "\r"); // Append user ID
                Info.Append(stringManager.getString("common_username") + ": " + Convert.ToString(dRow[0]) + "\r"); // Append username
                Info.Append(stringManager.getString("common_userrank") + ": " + Convert.ToString(dRow[1]) + "\r"); // Append rank
                Info.Append(stringManager.getString("common_usermission") + ": " + Convert.ToString(dRow[2]) + "\r"); // Append user's mission
                Info.Append(stringManager.getString("common_credits") + ": " + Convert.ToString(dRow[3]) + "\r"); // Append user's amount of credits
                Info.Append(stringManager.getString("common_tickets") + ": " + Convert.ToString(dRow[4]) + "\r"); // Append user's amount of tickets
                Info.Append(stringManager.getString("common_hbirth") + ": " + Convert.ToString(dRow[7]) + "\r\r"); // Append 'registered at' date + blank line
                Info.Append(stringManager.getString("common_birth") + ": " + Convert.ToString(dRow[6]) + "\r"); // Append real birthday
                Info.Append(stringManager.getString("common_email") + ": " + Convert.ToString(dRow[5]) + "\r"); // Append email address
                Info.Append(stringManager.getString("common_ip") + ": " + Convert.ToString(dRow[8]) + "\r\r"); // Append user's last used IP address

                if (_Users.ContainsKey(userID)) // User online
                {
                    virtualUser User = (virtualUser)_Users[userID];
                    string Location = "";
                    if (User._roomID == 0)
                        Location = stringManager.getString("common_hotelview");
                    else
                    {
                        using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
                        {
                            Location = stringManager.getString("common_room") + " '" + dbClient.getString("SELECT name FROM rooms WHERE id = '" + User._roomID + "'") + "' [id: " + User._roomID + ", " + stringManager.getString("common_owner") + ": " + dbClient.getString("SELECT owner FROM rooms WHERE id = '" + User._roomID + "'") + "]"; // Roomname, room ID and name of the user that owns the room
                        }
                    }
                    Info.Append(stringManager.getString("common_location") + ": " + Location);
                }
                else // User is offline
                    Info.Append(stringManager.getString("common_lastaccess") + ": " + Convert.ToString(dRow[9])); // Append last server access date
                return Info.ToString();
            }
        }
        /// <summary>
        /// Generates a string containing the packet to send the the user.
        /// </summary>
        /// <param name="Rank">The rank of the user.</param>
        public static string generateCommands(byte Rank, int userID)
        {
            //Hashtable Commands = new Hashtable();
            string[] Commands = null;
            StringBuilder Packet = new StringBuilder("BKYour Commands:");

            // % = next
            string Cmds = "";


            Cmds += "%";
            Cmds += ":im NAME [MESSAGE] - Stuur een bericht naar een vriend%";
            Cmds += ":events - Laat evenementen venster zien%";
            Cmds += ":about - Laat informatie over de server zien%";
            Cmds += ":whosonline - Laat alle gebruikers + rank die online zijn zien%";
            Cmds += ":cleanhand - Verwijderd ALLE items in je hand (geen vergoeding)%";
            Cmds += ":brb - Zegt tegen iedereen dat je weg bent (Max 15 minutes)%";
            Cmds += ":back - Stop met het vertellen dat je weg bent%";
            Cmds += ":hotel/:info/:version - laat de informatie over dit hotel zien.%";
            Cmds += ":staff - Verteld je hoe je de staff kan bereiken%";


            if (Rank > 1)
            {
                Cmds += "%";
                Cmds += ":chooser - Laat iedereen zien in de kamer%";
                Cmds += ":furni - Laat alle meubels zien in de kamer%";

                if (rankManager.containsRight(Rank, "fuse_alert", userID))
                {
                    Cmds += "%";
                    Cmds += ":alert NAME MESSAGE - Sends a moderator alert%";
                }

                if (rankManager.containsRight(Rank, "fuse_room_alert", userID))
                {
                    Cmds += "%";
                    Cmds += ":roomalert MESSAGE - Sends a moderator alert to a whole room%";
                }

                if (rankManager.containsRight(Rank, "fuse_kick", userID))
                {
                    Cmds += "%";
                    Cmds += ":kick [MESSAGE] - Kicks a user from the room they are in%";
                }

                if (rankManager.containsRight(Rank, "fuse_room_kick", userID))
                {
                    Cmds += "%";
                    Cmds += ":roomkick [MESSAGE] - Kicks all users but you from the room";
                }

                if (rankManager.containsRight(Rank, "fuse_mute", userID))
                {
                    Cmds += "%";
                    Cmds += ":shutup NAME [MESSAGE] - Stops a user from using say/shout%";
                    Cmds += ":unmute NAME - Allows a user to say/shout again%";
                }

                if (rankManager.containsRight(Rank, "fuse_room_mute", userID))
                {
                    Cmds += "%";
                    Cmds += ":roomshutup [MESSAGE] - Stops all users in the room from using say/shout%";
                    Cmds += ":roomunmute Allows all users in the room to say/shout again%";
                }

                if (rankManager.containsRight(Rank, "fuse_ban", userID))
                {
                    Cmds += "%";
                    Cmds += ":ban NAME HOURS REASON - Bans a user for the how many hours%";
                }

                if (rankManager.containsRight(Rank, "fuse_superban", userID))
                {
                    Cmds += "%";
                    Cmds += ":superban NAME HOURS REASON - Bans a user and there IP for how many hours%";
                }

                if (rankManager.containsRight(Rank, "fuse_administrator_access", userID))
                {
                    Cmds += "%";
                    Cmds += ":ha MESSAGE - Sends an alert to all online users%";
                    Cmds += ":offline MINUTES - Informs all online users that the hotel is going offline in how long you said%";
                    Cmds += ":refresh - Updates certain parts of the server%";
                    Cmds += ":cords - Display your current room position%";
                    Cmds += ":teleport - Turns on teleport mode (WARNING: It causes the room to screw up)%";
                    Cmds += ":warp X Y -  Teleports you the the the location input%";
                    Cmds += ":sendme PACKET - Sends a packet to your self (For debuging)%";
                }

                if (rankManager.containsRight(Rank, "fuse_alert", userID))
                {
                    Cmds += "%";
                    Cmds += ":ra MESSAGE - Sends an alert to all online users with the same rank as you%";
                }

                if (rankManager.containsRight(Rank, "fuse_teleport", userID))
                {
                    Cmds += "%";
                    Cmds += ":teleport - Toggles your movent to teleport mode on/off%";
                    Cmds += ":warp X Y - Teleports you to the co-ordinates entered%";
                }

                if (rankManager.containsRight(Rank, "fuse_moderator_access", userID))
                {
                    Cmds += "%";
                    Cmds += ":userinfo NAME/:ui NAME - Displays infomation about a user%";
                }
            }
            Commands = Cmds/*.Replace("%", "|")*/.Split("%".ToCharArray());


            int r = 30;
            foreach (string Command in Commands)
            {
                if (r == 0)
                {
                    Packet.Append(Convert.ToChar(1) + "BKYour Commands:");
                    r = 30;
                }
                Packet.Append("\r" + Command);
                r--;
            }
            return Packet.ToString();
        }
        /// <summary>
        /// Generates a string containing the packet to send the the user.
        /// </summary>
        /// 
        public static string generateWhosOnline(bool Admin)
        {
            int i = userCount;
            int alerts = 0;
            while (i > 30)
            {
                alerts++;
                i -= 30;
            }
            alerts++;
            StringBuilder packet = new StringBuilder("BKOnline Users");
            if (alerts > 1)
                packet.Append(" page 1/" + alerts);
            packet.Append(" Total online: " + userCount);
            int r = 30;
            int p = 1;
            if (Admin)
                foreach (DictionaryEntry user in _Users)
                {
                    if (r == 0)
                    {
                        p++;
                        packet.Append(Convert.ToChar(1) + "BKOnline Users" + " page " + p + "/" + alerts + " Total online: " + userCount);
                        r = 30;
                    }
                    packet.Append("\r" + getUserName((int)user.Key) + " [" + user.Key.ToString() + "] {" + getUser((int)user.Key)._Rank + "}   " + getUser((int)user.Key).connectionRemoteIP);
                    r--;
                }
            else
            {
                foreach (DictionaryEntry user in _Users)
                {
                    if (r == 0)
                    {
                        p++;
                        packet.Append(Convert.ToChar(1) + "BKOnline Users" + " page " + p + " Total online: " + userCount);
                        r = 30;
                    }
                    packet.Append("\r" + getUserName((int)user.Key) + " {" + getUser((int)user.Key)._Rank + "}");
                    r--;
                }
            }
            return packet.ToString();
        }

        /// <summary>
        /// (Re)bans a single user for a specified amount of hours and reason. If the user is online, then it receives the ban message and get's disconnected.
        /// </summary>
        /// <param name="userID">The database ID of the user to ban.</param>
        /// <param name="Hours">The amount of hours (starts now) till the ban is lifted.</param>
        /// <param name="Reason">The reason for the ban, that describes the user why it's account is blocked from the system.</param>
        public static void setBan(int userID, int Hours, string Reason)
        {
            string Expires = DateTime.Now.AddHours(Hours).ToString().Replace("/", "-").Replace(".", "-");
            using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
            {
                dbClient.AddParamWithValue("userid", userID);
                dbClient.AddParamWithValue("expires", Expires);
                dbClient.AddParamWithValue("reason", Reason);
                dbClient.runQuery("INSERT INTO users_bans (userid,date_expire,descr) VALUES (@userid,@expires,@reason)");
            }
            if (_Users.ContainsKey(userID))
            {
                virtualUser User = ((virtualUser)_Users[userID]);
                User.sendData("@c" + Reason);
                User.Disconnect(1000);
            }
        }
        /// <summary>
        /// Checks if there are system bans for a certain IP address.
        /// If a ban is detected, it checks if it's already expired.
        /// If that is the case, then it lifts the ban.
        /// If there is a pending ban, it returns the reason that was supplied with the banning, otherwise, it returns "".
        /// </summary>
        /// <param name="IP">The IP address to check bans for.</param>
        public static string getBanReason(string IP)
        {
            using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
            {
                if (dbClient.findsResult("SELECT ipaddress FROM users_bans WHERE ipaddress = '" + IP + "'"))
                {
                    string banExpires = dbClient.getString("SELECT date_expire FROM users_bans WHERE ipaddress = '" + IP + "'");
                    if (DateTime.Compare(DateTime.Parse(banExpires), DateTime.Now) > 0)
                        return dbClient.getString("SELECT descr FROM users_bans WHERE ipaddress = '" + IP + "'"); // Still banned, return reason
                    else
                        dbClient.runQuery("DELETE FROM user_bans WHERE ipaddress = '" + IP + "' LIMIT 1");
                }
            }
            return ""; // No pending ban/ban expired
        }
        /// <summary>
        /// (Re)bans all the users on a certain IP address, making them unable to login, and making them unable to connect to the system. The ban is applied with a specified amount and reason. All affected users receive the ban message (which contains the reason) and they are disconnected.
        /// </summary>
        /// <param name="IP">The IP address to ban.</param>
        /// <param name="Hours">The amount of hours (starts now) till the ban is lifted.</param>
        /// <param name="Reason">The reason for the ban, that describes thes user why their IP address/accounts are blocked from the system.</param>
        public static void setBan(string IP, int Hours, string Reason)
        {
            string Expires = DateTime.Now.AddHours(Hours).ToString().Replace("/", "-").Replace(".", "-");
            DataColumn dCol;
            using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
            {
                dbClient.AddParamWithValue("ip", IP);
                dbClient.AddParamWithValue("expires", Expires);
                dbClient.AddParamWithValue("reason", Reason);
                dbClient.runQuery("INSERT INTO users_bans (ipaddress,date_expire,descr) VALUES (@ip,@expires,@reason)");

                dCol = dbClient.getColumn("SELECT id FROM users WHERE ipaddress_last = '" + IP + "'");
            }
            virtualUser User;

            foreach (DataRow dRow in dCol.Table.Rows)
            {
                if (_Users.ContainsKey(Convert.ToInt32(dRow["id"])))
                {
                    User = ((virtualUser)_Users[Convert.ToInt32(dRow["ID"])]);
                    User.sendData("@c" + Reason);
                    User.Disconnect(1000);
                }
            }
        }
        /// <summary>
        /// Checks if there is a system ban for a certain user.
        /// If a ban is detected, it checks if it's already expired.
        /// If that is the case, then it lifts the ban.
        /// If there is a pending ban, it returns the reason that was supplied with the banning, otherwise, it returns "".
        /// </summary>
        /// <param name="userID">The database ID of the user to check for bans.</param>
        public static string getBanReason(int userID)
        {
            using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
            {
                if (dbClient.findsResult("SELECT userid FROM users_bans WHERE userid = '" + userID + "'"))
                {
                    string banExpires = dbClient.getString("SELECT date_expire FROM users_bans WHERE userid = '" + userID + "'");
                    if (DateTime.Compare(DateTime.Parse(banExpires), DateTime.Now) > 0) // Still banned, return reason
                        return dbClient.getString("SELECT descr FROM users_bans WHERE userid = '" + userID + "'");
                    else
                        dbClient.runQuery("DELETE FROM users_bans WHERE userid = '" + userID + "' LIMIT 1");
                }
            }
            return ""; // No pending ban/ban expired
        }
        /// <summary>
        /// Generates a ban report for a certain ban on a user, including all details that could be of use. If there was no ban found, or the user that was banned doesn't exist, then a holo.cast.banreport.null is returned.
        /// </summary>
        /// <param name="userID">The database ID of the user to generate the ban report for.</param>
        public static string generateBanReport(int userID)
        {
            try
            {
                DataRow dbRow;
                DataRow dRow;
                using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
                {
                    dRow = dbClient.getRow("SELECT date_expire,descr,ipaddress FROM users_bans WHERE userid = '" + userID + "'");
                    dbRow = dbClient.getRow("SELECT name,rank,ipaddress_last FROM users WHERE id = '" + userID + "'");
                }

                if (dRow.Table.Rows.Count == 0 || dbRow.Table.Rows.Count == 0)
                {
                    return "holo.cast.banreport.null";
                }
                else
                {
                    string Note = "-";
                    string banPoster = "not available";
                    string banPosted = "not available";
                    DataRow DRow;
                    using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
                    {
                        DRow = dbClient.getRow("SELECT userid,note,timestamp FROM system_stafflog WHERE action = 'ban' AND targetid = '" + userID + "' ORDER BY id ASC"); // Get latest stafflog entry for this action (if exists)
                    }
                    if (DRow.Table.Rows.Count > 0) // system_stafflog table could be cleaned up
                    {
                        if (Convert.ToString(DRow[1]) != "")
                            Note = Convert.ToString(DRow[1]);
                        using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
                        {
                            banPoster = dbClient.getString("SELECT name FROM users WHERE id = '" + Convert.ToString(DRow[0]) + "'");
                        }
                        banPosted = Convert.ToString(DRow[2]);
                    }
                    StringBuilder Report = new StringBuilder(stringManager.getString("banreport_header") + " ");
                    Report.Append(Convert.ToString(dbRow[0]) + " [" + userID + "]" + "\r"); // Append username and user ID
                    Report.Append(stringManager.getString("common_userrank") + ": " + Convert.ToString(dbRow[1]) + "\r"); // Append user's rank
                    Report.Append(stringManager.getString("common_ip") + ": " + Convert.ToString(dbRow[2]) + "\r"); // Append the IP address of user
                    Report.Append(stringManager.getString("banreport_banner") + ": " + banPoster + "\r"); // Append username of banner
                    Report.Append(stringManager.getString("banreport_posted") + ": " + banPosted + "\r"); // Append datetime when ban was posted
                    Report.Append(stringManager.getString("banreport_expires") + ": " + Convert.ToString(dRow[0]) + "\r"); // Append datetime when ban expires
                    Report.Append(stringManager.getString("banreport_reason") + ": " + Convert.ToString(dRow[1]) + "\r"); // Append the reason that went with the ban
                    Report.Append(stringManager.getString("banreport_ipbanflag") + ": " + (Convert.ToString(dRow[2]) != "").ToString().ToLower() + "\r"); // Append true/false for the IP ban status
                    Report.Append(stringManager.getString("banreport_staffnote") + ": " + Note); // Append the staffnote that went with the ban
                    return Report.ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// Generates a ban report for a certain IP address, including all details that could be of use. If there was no ban found, or the user that was banned doesn't exist, then a holo.cast.banreport.null is returned.
        /// </summary>
        /// <param name="IP">The IP address to generate the ban report for.</param>
        public static string generateBanReport(string IP)
        {
            try
            {
                DataRow dRow;
                using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
                {
                    dRow = dbClient.getRow("SELECT userid,date_expire,descr FROM users_bans WHERE ipaddress = '" + IP + "'");
                }
                if (dRow.Table.Rows.Count == 0)
                {
                    return "holo.cast.banreport.null";
                }
                else
                {
                    string Note = "-";
                    string banPoster = "not available";
                    string banPosted = "not available";
                    DataRow DRow;
                    using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
                    {
                        DRow = dbClient.getRow("SELECT userid,note,timestamp FROM system_stafflog WHERE action = 'ban' AND targetid = '" + Convert.ToString(dRow[0]) + "' ORDER BY id ASC"); // Get latest stafflog entry for this action (if exists)
                    }
                    if (DRow.Table.Rows.Count > 0) // system_stafflog table could be cleaned up
                    {
                        if (Convert.ToString(DRow[1]) != "")
                            Note = Convert.ToString(DRow[1]);
                        using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
                        {
                            banPoster = dbClient.getString("SELECT name FROM users WHERE id = '" + Convert.ToString(DRow[0]) + "'");
                        }
                        banPosted = Convert.ToString(DRow[2]);
                    }

                    StringBuilder Report = new StringBuilder(stringManager.getString("banreport_header") + " ");
                    Report.Append(IP + "\r"); // Append IP address
                    Report.Append(stringManager.getString("banreport_banner") + ": " + banPoster + "\r"); // Append username of banner
                    Report.Append(stringManager.getString("banreport_posted") + ": " + banPosted + "\r"); // Append datetime when ban was posted
                    Report.Append(stringManager.getString("banreport_expires") + ": " + Convert.ToString(dRow[0]) + "\r"); // Append datetime when ban expires
                    Report.Append(stringManager.getString("banreport_reason") + ": " + Convert.ToString(dRow[1]) + "\r"); // Append the reason that went with the ban
                    Report.Append(stringManager.getString("banreport_ipbanflag") + ": " + (Convert.ToString(dRow[2]) != "").ToString().ToLower() + "\r"); // Append true/false for the IP ban status
                    Report.Append(stringManager.getString("banreport_staffnote") + ": " + Note + "\r\r"); // Append the staffnote that went with the ban

                    DataColumn dCol;
                    using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
                    {
                        dCol = dbClient.getColumn("SELECT name FROM users WHERE ipaddress_last = '" + IP + "'");
                    }
                    //string[] affectedUsernames = dataHandling.dColToArray(dCol);
                    Report.Append(stringManager.getString("banreport_affectedusernames") + ":");
                    foreach (DataRow dbRow in dCol.Table.Rows)
                        Report.Append("\r - " + Convert.ToString(dRow["name"]));
                    return Report.ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            }
        /// <summary>
        /// Ran on a thread at interval 60000ms, checks ping status of users and disconnects timed out users.
        /// </summary>
        private static void checkPings()
        {
            List<virtualUser> vir = new List<virtualUser>();
            while (true)
            {
                try
                {
                    foreach (virtualUser User in _Users.Values)
                    {
                        if (User.pingOK)
                        {
                            User.pingOK = false;
                            User.sendData("@r");
                        }
                        else
                        {
                            Holo.Out.WriteLine(User._Username + " got disconnected for some reason.");
                            vir.Add(User);

                        }
                    }
                }
                catch { }
                foreach (virtualUser User in vir)
                {
                    User.Disconnect();
                }
                vir.Clear();

                Thread.Sleep(60000);
            
                Out.WriteTrace("Checking virtual user pings");
            }
        }
    
    }
}
    