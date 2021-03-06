﻿using System;
using System.Data;
using System.Text;
using System.Collections;
using System.Threading;

using Holo.Virtual.Rooms;
using Ion.Storage;

namespace Holo.Managers
{
    /// <summary>
    /// Provides management for virtual rooms, aswell as some misc tasks for rooms.
    /// </summary>
    public static class roomManager
    {
        #region Declares
        /// <summary>
        /// Contains the hooked virtual room objects.
        /// </summary>
        private static Hashtable _Rooms = new Hashtable();
        public static Hashtable _roomLayOut = new Hashtable();
        /// <summary>
        /// The peak amount of rooms that has been in the room manager since start of the emulator.
        /// </summary>
        private static int _peakRoomCount;
        #endregion

        #region Virtual room management
        /// <summary>
        /// Adds a virtualRoom class together with the roomID to the roomManager.
        /// </summary>
        /// <param name="roomID">The ID of the room to add..</param>
        /// <param name="Room">The virtualRoom class of this room.</param>
        public static void Init()
        {

        }

        public static void addRoom(int roomID, virtualRoom Room)
        {
            if (_Rooms.ContainsKey(roomID) == false)
            {
                _Rooms.Add(roomID, Room);
                Out.WriteLine("Room [" + roomID + ", publicroom: " + Room.isPublicroom.ToString().ToLower() + "] has booted up successfully.", Out.logFlags.StandardAction);
                if (_Rooms.Count > _peakRoomCount)
                    _peakRoomCount = _Rooms.Count;
            }

        }
        /// <summary>
        /// Removes a room from the roomManager. [if it exists]
        /// </summary>
        /// <param name="roomID">The ID of the room to remove.</param>
        public static void removeRoom(int roomID)
        {
            if (_Rooms.ContainsKey(roomID))
            {
                bool boolPublicroom = ((virtualRoom)_Rooms[roomID]).isPublicroom;
                _Rooms.Remove(roomID);
                updateRoomVisitorCount(roomID, 0);
                Out.WriteLine("Room [" + roomID + ", publicroom: " + boolPublicroom.ToString().ToLower() + "] destroyed.", Out.logFlags.StandardAction);
                GC.Collect();
            }
        }
        /// <summary>
        /// Returns a bool that indicates if the roomManager contains a certain room.
        /// </summary>
        /// <param name="roomID">The ID of the room.</param>
        public static bool containsRoom(int roomID)
        {
            return _Rooms.ContainsKey(roomID);
        }

        /// <summary>
        /// Returns the current amount of rooms in the roomManager.
        /// </summary>
        public static int roomCount
        {
            get
            {
                return _Rooms.Count;
            }
        }
        /// <summary>
        /// Returns the peak amount of rooms in the roomManager since boot.
        /// </summary>
        public static int peakRoomCount
        {
            get
            {
                return _peakRoomCount;
            }
        }

        /// <summary>
        /// Returns a virtualRoom class for a certain room.
        /// </summary>
        /// <param name="roomID">The ID of the room.</param>
        public static virtualRoom getRoom(int roomID)
        {
            return (virtualRoom)_Rooms[roomID];
        }

        #endregion

        #region Misc room related functions
        /// <summary>
        /// Updates the inside visitors count in the database for a certain room.
        /// </summary>
        /// <param name="roomID">The ID of the room to update.</param>
        /// <param name="visitorCount">The new visitors count.</param>
        public static void updateRoomVisitorCount(int roomID, int visitorCount)
        {
            using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
            {
                dbClient.runQuery("UPDATE rooms SET visitors_now = '" + visitorCount + "' WHERE id = '" + roomID + "' LIMIT 1");
            }
        }
        /// <summary>
        /// Returns the int ID for a certain room state.
        /// </summary>
        /// <param name="State">The room state ID.</param>
        public static int getRoomState(string State)
        {
            if (State == "closed")
                return 1;
            else if (State == "password")
                return 2;
            else
                return 0;
        }
        /// <summary>
        /// Returns the string state for a certain room state byte.
        /// </summary>
        /// <param name="State">The room state ID.</param>
        public static string getRoomState(int State)
        {
            if (State == 1)
                return "closed";
            else if (State == 2)
                return "password";
            else
                return "open";
        }
        #endregion

        #region Furni related functions
        /// <summary>
        /// Updates the inside visitors count in the database for a certain room.
        /// </summary>
        /// <param name="roomID">The ID of the room the item is in.</param>
        /// <param name="itemID">The ID of the item to refresh.</param>
        /// <param name="cctName">The cct name of the item to refresh.</param>
        /// <param name="wallPosition">The wall position of the item to refresh.</param>
        /// <param name="itemVariable">The variable of the item to refresh.</param>
        public static void refreshWallitem(int roomID, int itemID, string cctName, string wallPosition, string itemVariable)
        {
            getRoom(roomID).sendData("AU" + itemID + Convert.ToChar(9) + cctName + Convert.ToChar(9) + " " + wallPosition + Convert.ToChar(9) + itemVariable);
        }

        /// <summary>
        /// Contains functions for the moodlight.
        /// </summary>
        public static class moodlight
        {
            /// <summary>
            /// Returns a string containing the setting data for the moodlight in the room.
            /// </summary>
            /// <param name="roomID">The roomID to get the moodlight for.</param>
            public static string getSettings(int roomID)
            {
                try
                {
                    DataRow dRow;
                    using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
                    {
                        dRow = dbClient.getRow("SELECT preset_cur,preset_1,preset_2,preset_3 FROM furniture_moodlight WHERE roomid = '" + roomID + "'");
                    }
                    //object[] itemObject = dRow.ItemArray;
                    //string[] itemSettings = new string[itemObject.Length];
                    //for (int i = 0; i < itemObject.Length; i++)
                    //itemSettings[i] = itemObject[i].ToString();                    
                    string settingPack = Encoding.encodeVL64(3) + Encoding.encodeVL64(Convert.ToInt32(dRow["preset_cur"]));

                    for (int i = 1; i <= 3; i++)
                    {
                        string[] curPresetData = dRow[i].ToString().Split(Char.Parse(","));
                        settingPack += Encoding.encodeVL64(i) + Encoding.encodeVL64(int.Parse(curPresetData[0])) + curPresetData[1] + Convert.ToChar(2) + Encoding.encodeVL64(int.Parse(curPresetData[2]));
                    }
                    return settingPack;
                }
                catch
                {
                    return null;
                }
            }
            /// <summary>
            /// Saves the setting data for the moodlight in the room.
            /// </summary>
            /// <param name="roomID">The roomID to get the moodlight for.</param>
            /// <param name="isEnabled">The status of the moodlight (on/off).</param>
            /// <param name="presetID">The preset slot that is being used.</param>
            /// <param name="bgState">The status of the background only tick.</param>
            /// <param name="presetColour">The colour that is being saved.</param>
            /// <param name="alphaDarkF">The alpha value of the darkness level.</param>
            public static void setSettings(int roomID, bool isEnabled, int presetID, int bgState, string presetColour, int alphaDarkF)
            {
                int itemID;
                using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
                {
                    itemID = dbClient.getInt("SELECT id FROM furniture_moodlight WHERE roomid = '" + roomID + "'");
                }

                string newPresetValue;

                if (isEnabled == false)
                {
                    string curPresetValue;
                    using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
                    {
                        curPresetValue = dbClient.getString("SELECT var FROM furniture WHERE id = '" + itemID + "'");
                    }
                    if (curPresetValue.Substring(0, 1) == "2")
                    {
                        newPresetValue = "1" + curPresetValue.Substring(1);
                    }
                    else
                    {
                        newPresetValue = "2" + curPresetValue.Substring(1);
                    }
                    using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
                    {
                        dbClient.runQuery("UPDATE furniture SET var = '" + newPresetValue + "' WHERE id = '" + itemID.ToString() + "' LIMIT 1");
                    }
                }
                else
                {
                    newPresetValue = "2" + "," + presetID.ToString() + "," + bgState.ToString() + "," + presetColour + "," + alphaDarkF.ToString();
                    using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
                    {
                        dbClient.AddParamWithValue("var", newPresetValue);
                        dbClient.AddParamWithValue("preset", bgState.ToString() + ", " + presetColour + ", " + alphaDarkF.ToString());
                        dbClient.runQuery("UPDATE furniture SET var = @var WHERE id = '" + itemID.ToString() + "' LIMIT 1");
                        dbClient.runQuery("UPDATE furniture_moodlight SET preset_cur = '" + presetID.ToString() + "',preset_" + presetID.ToString() + "= @preset WHERE id = '" + itemID.ToString() + "' LIMIT 1");
                    }
                }
                string wallPosition;
                using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
                {
                    wallPosition = dbClient.getString("SELECT wallpos FROM furniture WHERE id = '" + itemID + "'");

                }
                refreshWallitem(roomID, itemID, "roomdimmer", wallPosition, newPresetValue);
            }
        }
        #endregion
    }
}
