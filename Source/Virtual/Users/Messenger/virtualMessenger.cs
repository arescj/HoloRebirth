using System;
using System.Data;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using Holo.Managers;
using Holo.Virtual;
using Holo.Virtual.Users;
using Holo.Virtual.Rooms;
using Ion.Storage;

namespace Holo.Virtual.Users.Messenger
{
    /// <summary>
    /// Represents the messenger for a virtual user, which provides keeping buddy lists, instant messaging, inviting friends to a user's virtual room and various other features. The virtual messenger object provides voids for updating status of friends, instant messaging and more.
    /// </summary>
    class virtualMessenger
    {
        #region Declares
        /// <summary>
        /// The database ID of the parent virtual user.
        /// </summary>
        private int userID;
        private Hashtable Buddies;
        #endregion

        #region Constructors/destructors
        /// <summary>
        /// Initializes the virtual messenger for the parent virtual user, generating friendlist, friendrequests etc.
        /// </summary>
        /// <param name="userID">The database ID of the parent virtual user.</param>
        internal virtualMessenger(int userID)
        {
            this.userID = userID;
            this.Buddies = new Hashtable();
        }
        internal string friendList()
        {
            int[] userIDs = userManager.getUserFriendIDs(userID);
            StringBuilder Buddylist = new StringBuilder(Encoding.encodeVL64(200) + Encoding.encodeVL64(200) + Encoding.encodeVL64(600) + "H" + Encoding.encodeVL64(userIDs.Length));

            virtualBuddy Me = new virtualBuddy(userID);
            DataRow dRow;

            for (int i = 0; i < userIDs.Length; i++)
            {
                virtualBuddy Buddy = new virtualBuddy(userIDs[i]);
                try
                {
                    if (Buddy.Online)
                    {
                        userManager.getUser(userIDs[i]).Messenger.addBuddy(Me, true);

                    }
                }
                catch { }
                if (Buddies.Contains(userIDs[i]) == false)
                    Buddies.Add(userIDs[i], Buddy);
                using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
                {
                    dRow = dbClient.getRow("SELECT lastvisit,mission FROM users WHERE id = " + userIDs[i] + " LIMIT 1");
                }
                Buddylist.Append(Buddy.ToString(true));

                Buddylist.Append(Convert.ToString(dRow[1]) + Convert.ToChar(2));
                Buddylist.Append(Convert.ToString(dRow[0]));


                Buddylist.Append(Convert.ToChar(2));

            }
            
            Buddylist.Append(Encoding.encodeVL64(100) + "H");
            //Out.WritePlain(Buddylist.ToString());
            return Buddylist.ToString();
        }
        internal string friendRequests()
        {
            DataColumn dCol;
            using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
            {
                dCol = dbClient.getColumn("SELECT userid_from,requestid FROM messenger_friendrequests WHERE userid_to = '" + this.userID + "' ORDER by requestid ASC");
            }
            StringBuilder Requests = new StringBuilder(Encoding.encodeVL64(dCol.Table.Rows.Count) + Encoding.encodeVL64(dCol.Table.Rows.Count));
            if (dCol.Table.Rows.Count > 0)
            {
                int i = 0;
                foreach (DataRow dRow in dCol.Table.Rows)
                {
                    using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
                    {
                        Requests.Append(Encoding.encodeVL64(Convert.ToInt32(dRow["requestid"])) + dbClient.getString("SELECT name FROM users WHERE id = '" + Convert.ToString(dRow["userid_from"]) + "'") + Convert.ToChar(2) + Convert.ToString(dRow["userid_from"]) + Convert.ToChar(2));
                    }
                    i++;
                }
            }
            
            
            return Requests.ToString();
        }
        internal void Clear()
        {

        }

        internal void addBuddy(virtualBuddy Buddy, bool Update)
        {
            if (Buddies.ContainsKey(Buddy.userID) == false)
                Buddies.Add(Buddy.userID, Buddy);
            if (Update)
                User.sendData("@MHII" + Buddy.ToString(true));
        }
        /// <summary>
        /// Deletes a buddy from the friendlist and virtual messenger of this user, but leaves the database row untouched.
        /// </summary>
        /// <param name="ID">The database ID of the buddy to delete from the friendlist.</param>
        internal void removeBuddy(int ID)
        {
            User.sendData("@MHI" + "M" + Encoding.encodeVL64(ID));
            if (Buddies.Contains(ID))
                Buddies.Remove(ID);
        }
        #region needs update
        internal string getUpdates()
        {
            int updateAmount = 0;
            //StringBuilder Updates = new StringBuilder();
            string Updates = "";
            string PacketAdd = "";

            try
            {
                DataRow dRow;
                foreach (virtualBuddy Buddy in ((Hashtable)Buddies.Clone()).Values)
                {
                    if (Buddy.Updated)
                    {

                        updateAmount++;
                        using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
                        {
                            dRow = dbClient.getRow("SELECT lastvisit,mission FROM users WHERE id = " + Buddy.userID + " LIMIT 1");
                        }
                        string[] IDs;
                        using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
                        {
                            IDs = dataHandling.dColToArray((dbClient.getColumn("SELECT id FROM users WHERE id = " + Buddy.userID + " LIMIT 1")));
                        }

                        // Loop through results 
                        for (int i = 0; i < IDs.Length; i++)
                        {

                            int thisID = Convert.ToInt16(IDs[i]);
                            bool online = userManager.containsUser(thisID);
                            string onlineStr = online ? "I" : "H";

                            DataRow row;
                            using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
                            {
                                row = dbClient.getRow("SELECT name, mission, lastvisit, figure FROM users WHERE id = " + thisID.ToString());
                            }
                            PacketAdd = Encoding.encodeVL64(thisID)
                                    + row[0] + ""
                                    + row[1] + "" + onlineStr + onlineStr + ""
                                    + onlineStr + (online ? row[3] : "")
                                    + "" + (online ? "" : row[2]) + "";
                            Updates += PacketAdd;
                        }
                    }
                }
                return "H" + Encoding.encodeVL64(updateAmount) + Updates;
            }
            catch { return "HH"; }
        }
        #endregion
        #endregion
        /// <summary>
        /// Returns a boolean that indicates if the messenger contains a certain buddy, and this buddy is online.
        /// </summary>
        /// <param name="userID">The database ID of the buddy to check.</param>
        internal bool containsOnlineBuddy(int userID)
        {
            if (Buddies.ContainsKey(userID) == false)
                return false;
            else
                return userManager.containsUser(userID);
        }
        /// <summary>
        /// Returns a bool that indicates if there is a friendship between the parent virtual user and a certain user.
        /// </summary>
        /// <param name="userID">The database ID of the user to check.</param>
        internal bool hasFriendship(int userID)
        {
            return Buddies.ContainsKey(userID);
        }
        /// <summary>
        /// Returns a bool that indicates if there are friend requests hinth and forth between the the parent virtual user and a certain user.
        /// </summary>
        /// <param name="userID">The database ID of the user to check.</param>
        internal bool hasFriendRequests(int userID)
        {
            using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
            {
                return dbClient.findsResult("SELECT requestid FROM messenger_friendrequests WHERE (userid_to = '" + this.userID + "' AND userid_from = '" + userID + "') OR (userid_to = '" + userID + "' AND userid_from = '" + this.userID + "')");
            }
        }

        #region Object management
        /// <summary>
        /// Returns the parent virtual user instance of this virtual messenger.
        /// </summary>
        internal virtualUser User
        {
            get
            {
                return userManager.getUser(this.userID);
            }
        }
        #endregion
    }
}
