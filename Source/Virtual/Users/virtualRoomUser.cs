using System;
using System.Threading;
using System.Collections;

using Holo.Managers;
using Holo.Virtual.Rooms;

namespace Holo.Virtual.Users
{
    /// <summary>
    /// Represents a virtual user in a virtual room.
    /// </summary>
    public class virtualRoomUser
    {
        /// <summary>
        /// The database ID of the user.
        /// </summary>
        internal int userID;
        /// <summary>
        /// The ID of the virtual room where the user is in.
        /// </summary>
        internal int roomID;
        /// <summary>
        /// The ID of the user in the virtual room.
        /// </summary>
        internal int roomUID;
        /// <summary>
        /// The X position of the user in the virtual room.
        /// </summary>
        internal int X;
        /// <summary>
        /// The Y position of the user in the virtual room.
        /// </summary>
        internal int Y;
        /// <summary>
        /// The height of the user in the virtual room.
        /// </summary>
        internal double H;
        /// <summary>
        /// The rotation of the user's head in the virtual room.
        /// </summary>
        internal byte Z1;
        /// <summary>
        /// The rotation of the user's body in the virtual room.
        /// </summary>
        internal byte Z2;
        /// <summary>
        /// The X position where the user wants to walk to. If not walking, -1.
        /// </summary>
        internal int goalX;
        /// <summary>
        /// The Y position where the user wants to walk to. If not walking, -1.
        /// </summary>
        internal int goalY;
        /// <summary>
        /// Specifies if the user can walk.
        /// </summary>
        internal bool walkLock;
        /// <summary>
        /// Specifies if the user is on his way of leaving the room after clicking the door.
        /// </summary>
        internal bool walkDoor;
        /// <summary>
        /// Guestroom only. Specifies if the user has voted on the guestroom already.
        /// </summary>
        internal bool hasVoted;
        /// The room user ID (rUID) of the virtual user where this virtual user is currently trading items with. If not trading, then this value is -1.
        /// </summary>
        internal int _tradePartnerRoomUID = -1;
        /// <summary>
        /// Swimming pools only. Specifies the swimming pool outfit.
        /// </summary>
        internal string swimOutfit;
        /// <summary>
        /// Gamelobbies only. Specifies the amount of gamepoints that this user has for the game in the current gamelobby.
        /// </summary>
        internal int gamePoints;
        /// <summary>
        /// The parent virtual user of this room user.
        /// </summary>
        internal virtualUser User;
        /// <summary>
        /// The status manager of this room user.
        /// </summary>
        internal virtualRoomUserStatusManager statusManager;
        /// <summary>
        /// Indicates if the room user is currently typing a chat message.
        /// </summary>
        internal bool isTyping;
        /// <summary>
        /// Indicates if the room user is able to use the special teleport cast, which allows him to teleport around the room.
        /// </summary>
        internal bool SPECIAL_TELEPORTABLE = false;
        private delegate void walkSleepPointer();
        /// <summary>
        /// Initializes a virtual room user.
        /// </summary>
        /// <param name="userID">The database ID of the virtual user of this room user object.</param>
        /// <param name="roomID">The database ID of the room where the room user is in.</param>
        /// <param name="roomUID">The ID that identifies the room user in the virtual room.</param>
        /// <param name="User">The parent vittual user of this room user.</param>
        /// <param name="statusManager">The status manager of this room user.</param>
        internal virtualRoomUser(int userID, int roomID, int roomUID, virtualUser User, virtualRoomUserStatusManager statusManager)
        {
            this.userID = userID;
            this.roomID = roomID;
            this.roomUID = roomUID;
            this.User = User;
            this.statusManager = statusManager;
        }
        /// <summary>
        /// The virtualRoom object that represents the room where this room user is in.
        /// </summary>
        internal virtualRoom Room
        {
            get
            {
                return roomManager.getRoom(roomID);
            }
        }
        /// <summary>
        /// Refreshes the status of this room user in the virtual room.
        /// </summary>
        internal void Refresh()
        {
            Room.Refresh(this);
        }
        /// <summary>
        /// Returns the details string for this room user, containing username etc.
        /// </summary>
        internal string detailsString
        {
            get
            {
                string petTransformAdd = "";
                string petTransformFigure = User._Figure;

                string s = "i:" + roomUID + Convert.ToChar(13) + "a:" + userID + Convert.ToChar(13) + "n:" + petTransformAdd + User._Username + Convert.ToChar(13) + "f:" + petTransformFigure + Convert.ToChar(13) + "l:" + X + " " + Y + " " + H + Convert.ToChar(13);
                if (User._Mission != "") { s += "c:" + User._Mission + Convert.ToChar(13); }
                else { s += "c:" + " Welcome Echo Hotel users!" + Convert.ToChar(13); }
                string szNotify = "";
                for (int x = 0; x < User._Badges.Count; x++)
                {
                    if (User._badgeSlotIDs[x] > 0) // Badge enabled
                    {
                        szNotify += User._badgeSlotIDs[x];
                        szNotify += ":";
                        szNotify += User._Badges[x];
                        szNotify += ",";
                    }
                }
                //Out.WritePlain(szNotify);
                if (szNotify != "") 
                { s += "b:" + szNotify + Convert.ToChar(13);}
                
                if (swimOutfit != "") { s += "p:" + swimOutfit + Convert.ToChar(13); }
                if (User._groupID > 0) { s += "g:" + User._groupID + Convert.ToChar(13) + "t:" + (int)User._groupMemberRank + Convert.ToChar(13); }
                return s;
            }
        }
        /// <summary>
        /// Returns the action string for this room user, containing position, movements, statuses etc.
        /// </summary>
        internal string statusString
        {
            get
            {
               return roomUID + " " + X + "," + Y + "," + H.ToString().Replace(",",".") + "," + Z1 + "," + Z2 + "/" + statusManager.ToString();
            }
        }
    }
}
