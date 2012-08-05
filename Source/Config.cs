using System;
using System.Threading;

using Holo.Managers;
using Ion.Storage;

namespace Holo
{
    /// <summary>
    /// Contains settings for the emulator. Config class is being initialized at boot of emulator.
    /// </summary>
    public static class Config
    {
        /// <summary>
        /// Specifies if chat animations should be used at chat.
        /// </summary>
        internal static bool enableChatAnims;
        /// <summary>
        /// Specifies if the word filter that filters swearwords should be enabled.
        /// </summary>
        internal static bool enableWordFilter;
        /// <summary>
        /// Specifies if the welcome message should be sent at login.
        /// </summary>
        internal static bool enableWelcomeMessage;
        /// <summary>
        /// Specifies if trading is enabled.
        /// </summary>
        internal static bool enableTrading;
        /// <summary>
        /// Specifies if the Recycler is enabled.
        /// </summary>
        internal static bool enableRecycler;
        /// <summary>
        /// Specifies the amount of sips that a virtual user should take from his drink/item before vanishing it.
        /// </summary>
        internal static int Statuses_itemCarrying_SipAmount; // Better, a byte
        /// <summary>
        /// Specifies the amount of milliseconds between the sips of the item carrying.
        /// </summary>
        internal static int Statuses_itemCarrying_SipInterval;
        /// <summary>
        /// Specifies the amount of milliseconds that a sip of a drink takes.
        /// </summary>
        internal static int Statuses_itemCarrying_SipDuration;
        /// <summary>
        /// Specifies the amount of milliseconds that the waving animation takes.
        /// </summary>
        internal static int Statuses_Wave_waveDuration;
        /// <summary>
        /// Specifies the amount of minutes until a roomban expires.
        /// </summary>
        internal static int Rooms_roomBan_banDuration;
        /// <summary>
        /// Specifies the max height of a stack of virtual items. If this height is overidden, then the height won't increase but stick at the max height. (merging virtual items)
        /// </summary>
        internal static int Items_Stacking_maxHeight;
        /// <summary>
        /// Specifies the max amount of virtual rooms that a virtual user can create.
        /// </summary>
        internal static int Navigator_createRoom_maxRooms;
        /// <summary>
        /// Specifies the max amount of guestrooms to display at the virtual room search engine in the Navigator.
        /// </summary>
        internal static int Navigator_roomSearch_maxResults;
        /// <summary>
        /// Specifies the max amount of guestrooms to display at opening a category in the Navigator.
        /// </summary>
        internal static int Navigator_openCategory_maxResults;
        /// <summary>
        /// Specifies the max amount of virtual rooms that virtual users can have in their 'favorite rooms' list.
        /// </summary>
        internal static int Navigator_Favourites_maxRooms;
        /// <summary>
        /// The template ID of the disk item to be used when burning a virtual song to disk.
        /// </summary>
        internal static int Soundmachine_burnToDisk_diskTemplateID;
        /// <summary>
        /// The link to the image that has to show while loading a room. If blank, then no image is shown.
        /// </summary>
        internal static string Rooms_LoadAvertisement_img;
        /// <summary>
        /// The url that has to be triggered when clicking the room load advertisement.
        /// </summary>
        internal static string Rooms_LoadAvertisement_uri;
        public static int WS_TICKET_COST = 1;
        public static int WS_GAMEMAX_TIME = 100000;
        public static int WS_MS_BETWEEN_EACH_HIT_MIN = 300;
        public static int WS_BALANCE_POINTS = 35;
        public static int WS_HIT_POINTS = 13;
        public static int WS_HIT_BALANCE_POINTS = 10;
        /// <summary>
        /// The amount of seconds that the 'preparing game...' bar has to display before starting the game.
        /// </summary>
        internal static int Game_Countdown_Seconds;
        /// <summary>
        /// The amount of seconds that it takes (at score window) before the game restarts for people that wished to replay the game.
        /// </summary>
        internal static int Game_scoreWindow_restartGame_Seconds;
        /// <summary>
        /// The amount of seconds that a game of 'BattleBall' takes.
        /// </summary>
        internal static int Game_BattleBall_gameLength_Seconds;
        internal static byte Minimum_CFH_Rank;
        /// <summary>
        /// Gets the value from a config entry in system_config table.
        /// </summary>
        /// <param name="strKey">The key of the config entry.</param>
        public static string getTableEntry(string Key)
        {
            using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
            {
                return dbClient.getString("SELECT sval FROM system_config WHERE skey = '" + Key + "' LIMIT 1");
            }
        }
        /// <summary>
        /// Initializes settings from system_config table for Config class.
        /// </summary>
        public static void Init(bool Update)
        {
            if(getTableEntry("chatanims_enable") == "1")
            {
                enableChatAnims = true;
                Out.WriteLine("Chat animations enabled.");
            }
            else
                Out.WriteLine("Chat animations disabled.");

            if(getTableEntry("trading_enable") == "1")
            {
                enableTrading = true;
                Out.WriteLine("Trading enabled.");
            }
            else
                Out.WriteLine("Trading disabled.");

            Rooms_LoadAvertisement_img = getTableEntry("rooms_loadadvertisement_img");
            if (Rooms_LoadAvertisement_img != "")
            {
                Rooms_LoadAvertisement_uri = getTableEntry("rooms_loadadvertisement_uri");
                if(stringManager.getStringPart(Rooms_LoadAvertisement_uri,0,7) != "null")
                    Rooms_LoadAvertisement_uri = "null";
            }
            Rooms_roomBan_banDuration = int.Parse(getTableEntry("rooms_roomban_duration"));

            Statuses_itemCarrying_SipAmount = int.Parse(getTableEntry("statuses_carryitem_sipamount"));
            Statuses_itemCarrying_SipInterval = int.Parse(getTableEntry("statuses_carryitem_sipinterval"));
            Statuses_itemCarrying_SipDuration = int.Parse(getTableEntry("statuses_carryitem_sipduration"));
            Statuses_Wave_waveDuration = int.Parse(getTableEntry("statuses_wave_duration"));
            Items_Stacking_maxHeight = int.Parse(getTableEntry("items_stacking_maxstackheight"));

            Navigator_createRoom_maxRooms = int.Parse(getTableEntry("navigator_createroom_maxrooms"));
            Navigator_roomSearch_maxResults = int.Parse(getTableEntry("navigator_roomsearch_maxresults"));
            Navigator_openCategory_maxResults = int.Parse(getTableEntry("navigator_opencategory_maxresults"));
            Navigator_Favourites_maxRooms = int.Parse(getTableEntry("navigator_favourites_maxrooms"));
            Soundmachine_burnToDisk_diskTemplateID = int.Parse(getTableEntry("soundmachine_burntodisk_disktid"));
            using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
            {
                Minimum_CFH_Rank = byte.Parse(dbClient.getString("SELECT minrank FROM rank_fuserights WHERE fuseright = 'fuse_receive_calls_for_help'")); // Not Dynamic Fuseright compatible
            }
            
            Game_Countdown_Seconds = int.Parse(getTableEntry("game_countdown_seconds"));
            Game_scoreWindow_restartGame_Seconds = int.Parse(getTableEntry("game_scorewindow_restartgame_seconds"));
            Game_BattleBall_gameLength_Seconds = int.Parse(getTableEntry("game_bb_gamelength_seconds"));
            
            if (Update)
                Thread.CurrentThread.Abort();
        }
    }
}