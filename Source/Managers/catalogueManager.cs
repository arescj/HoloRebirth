using System;
using System.Data;
using System.Text;
using System.Collections;
using System.Threading;
using Ion.Storage;

namespace Holo.Managers
{
    /// <summary>
    /// Manager for catalogue page caching, catalogue item templates, catalogue purchase handling and few other catalogue related tasks.
    /// </summary>
    public static class catalogueManager
    {
        private static Hashtable cataloguePages;
        private static Hashtable itemCache;
        
        /// <summary>
        /// Initializes the catalogue manager, (re)caching all the pages and item templates.
        /// </summary>
        public static void Init(bool Update)
        {
            //Out.WriteLine("Starting caching of catalogue + items...");
            DataColumn dCol;
            using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
            {
                dCol = dbClient.getColumn("SELECT indexid FROM catalogue_pages ORDER BY indexid");
            }
            cataloguePages = new Hashtable();
            itemCache = new Hashtable();
            foreach (DataRow dRow in dCol.Table.Rows)
            {
                cachePage(Convert.ToInt32(dRow["indexid"]));
            }

            cachePage(-1);
            Out.WriteLine("Successfully cached " + cataloguePages.Count + " catalogue pages and " + itemCache.Count + " item templates!");

            if (Update)
                Thread.CurrentThread.Abort();
        }
        /// <summary>
        /// Caches a specified catalogue page, plus the items on this page.
        /// </summary>
        /// <param name="pageID">The ID of the page to cache. If -1 is specified, all the items that aren't on a page are cached.</param>
        private static void cachePage(int pageID)
        {
            DataRow dRow;
            using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
            {
                dRow = dbClient.getRow("SELECT indexname,minrank,displayname,style_layout,img_header,img_side,label_description,label_misc,label_moredetails FROM catalogue_pages WHERE indexid = '" + pageID + "' LIMIT 1");
            }
            
            if (pageID > 0 && dRow.Table.Rows.Count == 0)
            {
                return;
            }

            object[] pageObject = dRow.ItemArray;
            string[] pageData = new string[pageObject.Length];
            for (int a = 0; a < pageData.Length; a++)
                pageData[a] = pageObject[a].ToString();


            string pageIndexName = "";
            StringBuilder pageBuilder = new System.Text.StringBuilder();
            cataloguePage objPage = new cataloguePage();

            if (pageID > 0)
            {
                pageIndexName = pageData[0];
                objPage.displayName = pageData[2];
                objPage.minRank = Convert.ToByte(bool.Parse(pageData[1]));

                // Add the required fields for catalogue page (indexname, showname, page layout style (boxes etc)) 
                pageBuilder.Append("i:" + pageIndexName + Convert.ToChar(13) + "n:" + pageData[2] + Convert.ToChar(13) + "l:" + pageData[3] + Convert.ToChar(13));

                if (pageData[4] != "") // If there's a headline image set, add it 
                    pageBuilder.Append("g:" + pageData[4] + Convert.ToChar(13));
                if (pageData[5] != "")  // If there is/are side image(s) set, add it/them 
                    pageBuilder.Append("e:" + pageData[5] + Convert.ToChar(13));
                if (pageData[6] != "") // If there's a description set, add it 
                    pageBuilder.Append("h:" + pageData[6] + Convert.ToChar(13));
                if (pageData[8] != "") // If there's a 'Click here for more details' label set, add it 
                    pageBuilder.Append("w:" + pageData[8] + Convert.ToChar(13));
                if (pageData[7] != "") // If the misc additions field is not blank 
                {
                    string[] miscDetail = pageData[7].Split(Convert.ToChar(13));
                    for (int m = 0; m < miscDetail.Length; m++)
                        pageBuilder.Append(miscDetail[m] + Convert.ToChar(13));
               }
            }

            DataTable dTable;
            using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
            {
                dTable = dbClient.getTable("SELECT tid, typeid, length, width, catalogue_cost, door, tradeable, recycleable, catalogue_name, catalogue_description, name_cct, colour, top FROM catalogue_items WHERE catalogue_id_page = '" + pageID + "' ORDER BY catalogue_id_index ASC");
            }
            int count = dTable.Rows.Count;
            int[] itemTemplateIDs = new int[count];
            int[] itemTypeIDs = new int[count];
            int[] itemLengths = new int[count];
            int[] itemWidths = new int[count];
            int[] itemCosts = new int[count];
            int[] itemDoorFlags = new int[count];
            int[] itemTradeableFlags = new int[count];
            int[] itemRecycleableFlags = new int[count];
            string[] itemNames = new string[count];
            string[] itemDescs = new string[count];
            string[] itemCCTs = new string[count];
            string[] itemColours = new string[count];
            string[] itemTopHs = new string[count];
            int i = 0;
            //tid,
            //typeid,
            //length,
            //width,
            //catalogue_cost,
            //door,
            //tradeable,
            //recycleable,
            //catalogue_name,
            //catalogue_description,
            //name_cct,
            //colour,
            //top
            foreach (DataRow dbRow in dTable.Rows)
            {
                itemTemplateIDs[i] = Convert.ToInt32(dbRow["tid"]);
                itemTypeIDs[i] = Convert.ToInt32(dbRow["typeid"]);
                itemLengths[i] = Convert.ToInt32(dbRow["length"]);
                itemWidths[i] = Convert.ToInt32(dbRow["width"]);
                itemCosts[i] = Convert.ToInt32(dbRow["catalogue_cost"]);
                itemDoorFlags[i] = Convert.ToInt32(dbRow["door"]);
                itemTradeableFlags[i] = Convert.ToInt32(dbRow["tradeable"]);
                itemRecycleableFlags[i] = Convert.ToInt32(dbRow["recycleable"]);
                itemNames[i] = Convert.ToString(dbRow["catalogue_name"]);
                itemDescs[i] = Convert.ToString(dbRow["catalogue_description"]);
                itemCCTs[i] = Convert.ToString(dbRow["name_cct"]);
                itemColours[i] = Convert.ToString(dbRow["colour"]);
                itemTopHs[i] = Convert.ToString(dbRow["top"]);
                i++;
            }

            for (i = 0; i < itemTemplateIDs.Length; i++)
            {
                if(stringManager.getStringPart(itemCCTs[i],0,4) != "deal")
                {
                    itemCache.Add(itemTemplateIDs[i], new itemTemplate(itemCCTs[i], Convert.ToByte(itemTypeIDs[i]), itemColours[i], itemLengths[i], itemWidths[i], double.Parse(itemTopHs[i]), (itemDoorFlags[i] == 1), (itemTradeableFlags[i] == 1), (itemRecycleableFlags[i] == 1)));
                    if (pageID == -1)
                        continue;
                    
                    pageBuilder.Append("p:" + itemNames[i] + Convert.ToChar(9) + itemDescs[i] + Convert.ToChar(9) + itemCosts[i] + Convert.ToChar(9) + Convert.ToChar(9));
                    
                    if (itemTypeIDs[i] == 0)
                        pageBuilder.Append("i");
                    else
                        pageBuilder.Append("s");

                    pageBuilder.Append(Convert.ToChar(9) + itemCCTs[i] + Convert.ToChar(9));

                    if (itemTypeIDs[i] == 0)
                        pageBuilder.Append(Convert.ToChar(9));
                    else
                        pageBuilder.Append("0" + Convert.ToChar(9));

                    if (itemTypeIDs[i] == 0)
                        pageBuilder.Append(Convert.ToChar(9));
                    else
                        pageBuilder.Append(itemLengths[i] + "," + itemWidths[i] + Convert.ToChar(9));
                    
                    pageBuilder.Append(itemCCTs[i] + Convert.ToChar(9));
          
                    if (itemTypeIDs[i] > 0)
                        pageBuilder.Append(itemColours[i]);
                    
                    pageBuilder.Append(Convert.ToChar(13));
                }
                else
                {
                    int dealID = int.Parse(itemCCTs[i].Substring(4));
                    using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
                    {
                        dTable = dbClient.getTable("SELECT tid, amount FROM catalogue_deals WHERE id = '" + dealID + "'");
                    }
                    count = dTable.Rows.Count;
                    int[] dealItemIDs = new int[count];
                    int[] dealItemAmounts = new int[count];
                    int n = 0;
                    foreach(DataRow dbRow in dTable.Rows)
                    {
                        dealItemIDs[n] = Convert.ToInt32(dbRow["tid"]);
                        dealItemAmounts[n] = Convert.ToInt32(dbRow["amount"]);
                        n++;
                    }

                    pageBuilder.Append("p:" + itemNames[i] + Convert.ToChar(9) + itemDescs[i] + Convert.ToChar(9) + itemCosts[i] + Convert.ToChar(9) + Convert.ToChar(9) + "d");
                    pageBuilder.Append(Convert.ToChar(9), 4);
                    pageBuilder.Append("deal" + dealID + Convert.ToChar(9) + Convert.ToChar(9) + dealItemIDs.Length + Convert.ToChar(9));

                    for (int l = 0; l < dealItemIDs.Length; l++)
                    {
                        using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
                        {
                            dRow = dbClient.getRow("SELECT name_cct, colour FROM catalogue_items WHERE tid = '" + dealItemIDs[l] + "' LIMIT 1");
                        }

                        string itemCCT = Convert.ToString(dRow["name_cct"]);
                        string itemColour = Convert.ToString(dRow["colour"]);
                        pageBuilder.Append(itemCCT + Convert.ToChar(9) + dealItemAmounts[l] + Convert.ToChar(9) + itemColour + Convert.ToChar(9));
                    }
                }
            }
            if (pageID == -1)
                return;

            objPage.pageData = pageBuilder.ToString();
            cataloguePages.Add(pageIndexName, objPage);
        }
        /// <summary>
        /// Returns a bool that specifies if the catalogue manager contains a certain page, specified by name.
        /// </summary>
        /// <param name="pageName">The name of the catalogue page to check.</param>
        public static bool getPageExists(string pageName)
        {
            return cataloguePages.ContainsKey(cataloguePages.ContainsKey(pageName));
        }
        /// <summary>
        /// Returns the index of catalogue pages for a certain user rank.
        /// </summary>
        /// <param name="userRank">The rank of the user to handout the index to.</param>
        public static string getPageIndex(byte userRank)
        {
            try
            {
                StringBuilder listBuilder = new StringBuilder();
                DataColumn dCol;
                using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
                {
                    dCol = dbClient.getColumn("SELECT indexname FROM catalogue_pages WHERE minrank <= '" + userRank + "' ORDER BY indexid ASC");
                }
                string[] pageNames = dataHandling.dColToArray(dCol);
                
                for (int i = 0; i < pageNames.Length; i++)
                {
                    if (cataloguePages.ContainsKey(pageNames[i]))
                        listBuilder.Append(pageNames[i] + Convert.ToChar(9) + ((cataloguePage)cataloguePages[pageNames[i]]).displayName + Convert.ToChar(13));
                }

                return listBuilder.ToString();
            }

            catch
            {
                return Convert.ToChar(13).ToString();
            }
        }
        /// <summary>
        /// Returns the content of a certain catalogue page as string.
        /// </summary>
        /// <param name="pageName">The name of the catalogue page to retrieve the content of.</param>
        /// <param name="userRank">The rank of the user to handout the page content to. If this rank is lower than the required minimum rank to access this page, the 'access denied' cast is returned.</param>
        /// <returns></returns>
        public static string getPage(string pageName, byte userRank)
        {
            try
            {
                cataloguePage objPage = ((cataloguePage)cataloguePages[pageName]);
                if (userRank < objPage.minRank)
                    return "holo.cast.catalogue.access_denied";

                return objPage.pageData;
            }

            catch
            {
                return "cast_catalogue.access_denied";
            }
        }
        /// <summary>
        /// Handles special actions at purchase in the catalogue, such as decoration variables for items and items who are sold in pairs.
        /// </summary>
        /// <param name="templateID">The template ID of the item being purchased.</param>
        /// <param name="receiverID">The ID of the user that receives the item in his/her Hand.</param>
        /// <param name="roomID">The target room ID of this item. 0 = inhand, -1 = in presentbox, -2 = in Recycler</param>
        /// <param name="decorID">The wallpaper/floor value for wallpaper/floor purchases.</param>
        /// <param name="presentBoxID">If the item is bought as present, then specify the ID of the present box item. If not, specify 0.</param>
        /// <param name="teleportID1"> ItemID of the other teleporter</param>
        public static void handlePurchase(int templateID, int receiverID, int roomID, string decorID, int presentBoxID, int teleportID1)
        {
            string Sprite = getTemplate(templateID).Sprite;
            bool handlePresentbox = true;

            switch (Sprite)
            {
                case "landscape":
                case "wallpaper":
                case "floor":
                    {
                        int itemID = lastItemID;
                        using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
                        {
                            dbClient.runQuery("UPDATE furniture SET var = '" + decorID + "' WHERE id = '" + itemID + "' LIMIT 1");
                        }
                        break;
                    }

                case "roomdimmer":
                    {
                        int itemID = lastItemID;
                        string defaultPreset = "1,#000000,155";
                        string defaultSetPreset = "1,1,1,#000000,155";
                        using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
                        {
                            dbClient.runQuery("INSERT INTO furniture_moodlight(id,roomid,preset_cur,preset_1,preset_2,preset_3) VALUES ('" + itemID + "','0','1','" + defaultPreset + "','" + defaultPreset + "','" + defaultPreset + "')");
                            dbClient.runQuery("UPDATE furniture SET var = '" + defaultSetPreset + "' WHERE id = '" + itemID + "' LIMIT 1");
                        }
                        break;
                    }

                case "door":
                case "doorB":
                case "doorC":
                case "doorD":
                case "teleport_door":
                case "xmas08_telep":
                case "ads_cltele":
                case "ads_mall_tele":
                case "ads_idol_tele":
                case "env_telep":
                case "ads_calip_tele":
                case "sf_tele":
                    {
                        int itemID1 = teleportID1;
                        int itemID2;


                        using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
                        {
                            dbClient.runQuery("INSERT INTO furniture(tid,ownerid,roomid,teleportid) VALUES ('" + templateID + "','" + receiverID + "','" + roomID + "','" + itemID1 + "')");
                            itemID2 = lastItemID;
                            dbClient.runQuery("UPDATE furniture SET teleportid = '" + itemID2 + "' WHERE id = '" + itemID1 + "' LIMIT 1");
                            if (presentBoxID > 0)
                            {
                                dbClient.runQuery("INSERT INTO furniture_presents(id,itemid) VALUES ('" + presentBoxID + "','" + itemID1 + "')");
                                dbClient.runQuery("INSERT INTO furniture_presents(id,itemid) VALUES ('" + presentBoxID + "','" + itemID2 + "')");
                            }
                        }
                        handlePresentbox = false;
                        break;
                    }

                case "post.it":
                case "post.it.vd":
                    {
                        int itemID = lastItemID;
                        using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
                        {
                            dbClient.runQuery("UPDATE furniture SET var = '20' WHERE id = '" + itemID + "' LIMIT 1");
                        }
                        break;
                    }

                default:
                    {
                        if (stringManager.getStringPart(Sprite, 0, 10) == "sound_set_")
                        {
                            int itemID = lastItemID;
                            int soundSet = int.Parse(Sprite.Substring(10));
                            using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
                            {
                                dbClient.runQuery("UPDATE furniture SET soundmachine_soundset = '" + soundSet + "' WHERE id = '" + itemID + "' LIMIT 1");
                            }
                        }
                        break;
                    }
            }

            if (presentBoxID > 0 && handlePresentbox)
            {
                using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
                {
                    dbClient.runQuery("INSERT INTO furniture_presents(id,itemid) VALUES ('" + presentBoxID + "','" + lastItemID + "')");
                }
            }
        }
        /// <summary>
        /// Returns the last purchased/created item in the 'furniture' table of the HoloDB.
        /// </summary>
        public static int lastItemID
        {
            get
            {
                using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
                {
                    return dbClient.getInt("SELECT MAX(id) FROM furniture");
                }
            }
        }
        public static string tradeItemList(int[] itemIDs)
        {
            StringBuilder List = new StringBuilder();
            for (int i = 0; i < itemIDs.Length; i++)
            {
                if (itemIDs[i] == 0)
                    continue;



                DataRow dRow;
                using (DatabaseClient dbClient = Eucalypt.dbManager.GetClient())
                {
                    dRow = dbClient.getRow("SELECT tid, var FROM furniture WHERE id = '" + itemIDs[i] + "'");
                }
                int templateID = Convert.ToInt32(dRow["tid"]);
                itemTemplate Template = getTemplate(templateID);
                List.Append("SI" + Convert.ToChar(30).ToString() + itemIDs[i] + Convert.ToChar(30).ToString() + i + Convert.ToChar(30));
                if (Template.typeID > 0)
                    List.Append("S");
                else
                    List.Append("I");
                List.Append(Convert.ToChar(30).ToString() + itemIDs[i] + Convert.ToChar(30).ToString() + Template.Sprite + Convert.ToChar(30));
                if (Template.typeID > 0) { List.Append(Template.Length + Convert.ToChar(30).ToString() + Template.Width + Convert.ToChar(30).ToString() + Convert.ToString(dRow["var"]) + Convert.ToChar(30).ToString()); }
                List.Append(Template.Colour + Convert.ToChar(30).ToString() + i + Convert.ToChar(30).ToString() + "/");
            }
            return List.ToString();
        }
        /// <summary>
        /// Checks if the wallposition for a wallitem is correct, if it is, then the output should be exactly the same as the input. If not, then the wallposition is invalid.
        /// </summary>
        /// <param name="wallPosition">The original wallposition. [input]</param>
        public static string wallPositionOK(string wallPosition)
        {
            //:w=3,2 l=9,63 l
            try
            {
                string[] posD = wallPosition.Split(' ');
                if (posD[2] != "l" && posD[2] != "r")
                    return "";

                string[] widD = posD[0].Substring(3).Split(',');
                int widthX = int.Parse(widD[0]);
                int widthY = int.Parse(widD[1]);
                if (widthX < 0 || widthY < 0 || widthX > 200 || widthY > 200)
                    return "";

                string[] lenD = posD[1].Substring(2).Split(',');
                int lengthX = int.Parse(lenD[0]);
                int lengthY = int.Parse(lenD[1]);
                if (lengthX < 0 || lengthY < 0 || lengthX > 200 || lengthY > 200)
                    return "";

                return ":w=" + widthX + "," + widthY + " " + "l=" + lengthX + "," + lengthY + " " + posD[2];
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Represents a page in the Catalogue.
        /// </summary>
        private struct cataloguePage
        {
            /// <summary>
            /// The display name of the page in the client.
            /// </summary>
            internal string displayName;
            /// <summary>
            /// The page string of the page, containing the layout, items etc.
            /// </summary>
            internal string pageData;
            /// <summary>
            /// The minimum rank that a virtual user requires to access this rank.
            /// </summary>
            internal byte minRank;
        }
        /// <summary>
        /// Represents a cached virtual item template.
        /// </summary>
        public struct itemTemplate
        {
            /// <summary>
            /// The type ID of the item, eg, 0 = walliten, 1 = flooritem, 2 = seat etc.
            /// </summary>
            internal byte typeID;
            /// <summary>
            /// The sprite of the item.
            /// </summary>
            internal string Sprite;
            /// <summary>
            /// The colour of the item.
            /// </summary>
            internal string Colour;
            /// <summary>
            /// The length of the item.
            /// </summary>
            internal int Length;
            /// <summary>
            /// The width of the item.
            /// </summary>
            internal int Width;
            /// <summary>
            /// The topheight of the item, if seat, then this indicates the sitheight. If a solid stackable item, then this is the stackheight. If 0.0, then the item is classified as non-stackable.
            /// </summary>
            internal double topH;
            /// <summary>
            /// Specifies if the item can be used as door.
            /// </summary>
            internal bool isDoor;
            /// <summary>
            /// Specifies if the item can be traded between virtual users.
            /// </summary>
            internal bool isTradeable;
            /// <summary>
            /// Specifies if the item can be recycled in the item recycler. 
            /// </summary>
            internal bool isRecycleable;
            public itemTemplate(bool b)
            {
                this.typeID = 1;
                this.Sprite = "";
                this.Colour = "null";
                this.Length = 1;
                this.Width = 1;
                this.topH = 0.0;
                this.isDoor = false;
                this.isTradeable = true;
                this.isRecycleable = false;
            }
            /// <summary>
            /// Initializes the item template.
            /// </summary>
            /// <param name="Sprite">The sprite of the item.</param>
            /// <param name="typeID">The type ID of the item, eg, 0 = walliten, 1 = flooritem, 2 = seat etc.</param>
            /// <param name="Colour">The colour of the item.</param>
            /// <param name="Length">The length of the item.</param>
            /// <param name="Width">The width of the item.</param>
            /// <param name="topH">The topheight of the item, if seat, then this indicates the sitheight. If a solid stackable item, then this is the stackheight. If 0.0, then the item is classified as non-stackable.</param>
            /// <param name="isDoor">Specifies if the item can be used as door.</param>
            /// <param name="isTradeable">Specifies if the item can be traded between virtual users.</param>
            /// <param name="isRecycleable">Specifies if the item can be recycled in the item recycler.</param>
            public itemTemplate(string Sprite, byte typeID, string Colour, int Length, int Width, double topH, bool isDoor, bool isTradeable, bool isRecycleable)
            {
                if (Sprite.Contains(" "))
                {
                    this.Sprite = Sprite.Split(' ')[0];
                    this.Colour = Sprite.Split(' ')[1];
                }
                else
                {
                    this.Sprite = Sprite;
                    this.Colour = Colour;
                }
                this.typeID = typeID;
                this.Length = Length;
                this.Width = Width;
                this.topH = topH;
                this.isDoor = isDoor;
                this.isTradeable = isTradeable;
                this.isRecycleable = isRecycleable;
            }
        }

        /// <summary>
        /// Returns the itemTemplate object matching a certain template ID. If the specified item template is not loaded, then an empty item template is returned.
        /// </summary>
        /// <param name="templateID">The template ID to return the item template of.</param>
        public static itemTemplate getTemplate(int templateID)
        {
            try { return (itemTemplate)itemCache[templateID]; }
            catch { return new itemTemplate(); }
        }
    }
}
