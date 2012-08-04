/* Habbo Hotel Encryption Class
 * Version: 24
 * Changelog: Encryption is now static - everything related to setKey, preMix etc
 * have been removed, as it is no longer need. Habbo seem to now ignore the Public Key
 * making this encryption class look even more less complex :-)
 * 
 * Cracked by: Erik [Sage] & Mike [Office.Boy]
 * 
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace HabboRC4_V24Release
{
    public class Habbo_V24_Crypto
    {
  // Class Variables
        int i;
        int j;

        int[] key = new int[256];
        int[] table = new int[256];

        public Habbo_V24_Crypto()
        {
            //Initialise the variables that store offset info
            this.initCalculators();

            //Set up the key and table arrays
            this.init();

            this.premixTable(this.premixString, this.premixCount);
        }

        private void initCalculators()
        {
            i = 0;
            j = 0;
        }

        internal void premixTable(string datain, int count)
        {
            for (int a = 0; a < count; a++)
            {
                this.encipher(datain);
            }
        }

        private void init()
        {
            int z;

            for (z = 0; z <= 254; z++)
            {
                table[z] = z + 1;
            }
            table[0] = 0;
            table[255] = 1;
        }

        public string encipher(string data)
        {
            StringBuilder cipher = new StringBuilder(data.Length * 2);

            int t = 0;
            int k = 0;

            for (int a = 0; a < data.Length; a++)
            {
                i = (i + 1) % 256;
                j = (j + table[i]) % 256;
                t = table[i];
                table[i] = table[j];
                table[j] = t;

                k = table[(table[i] + table[j]) % 256];

                int c = (char)data.Substring(a, 1).ToCharArray()[0] ^ k;

                if (c <= 0)
                {
                    cipher.Append("00");
                }
                else
                {
                    cipher.Append(di[c >> 4 & 0xf]);
                    cipher.Append(di[c & 0xf]);
                }

            }

            return cipher.ToString();
        }

        public string deciphper(string data)
        {
            StringBuilder cipher = new StringBuilder(data.Length);
            int t = 0;
            int k = 0;
            for (int a = 0; a < data.Length; a += 2)
            {
                i = (i + 1) % 256;
                j = (j + table[i]) % 256;
                t = table[i];
                table[i] = table[j];
                table[j] = t;
                k = table[(table[i] + table[j]) % 256];
                //t = System.Convert.ToInt32( data.Substring(a, a + 2), 16);
                t = System.Convert.ToInt32(JavaSubstring(data, a, a + 2), 16);
                cipher = cipher.Append((char)(t ^ k));
            }
            //this.rePremix();
            return cipher.ToString();
        }

        public string rePremix()
        {
            int tmpCount = 10;
            string tmpString = GenerateRandomString(20);
            this.premixTable(tmpString, tmpCount);
            return "";
        }

        private static string GenerateRandomString(int Length)
        {
            string tmp = "";
            Random Rnd = new Random();
            for (int x = 0; x < Length; x++)
            {
                char tmp2 = (char)Rnd.Next(65, 122);
                tmp += tmp2.ToString();
            }
            return tmp;
        }

        public string JavaSubstring(string dataIn, int start, int end)
        {
            return dataIn.Substring(start, end - start);
        }

        #region Constants

        string[] di = {
        "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", 
        "A", "B", "C", "D", "E", "F"
        };

        //string premixString = "eb11nmhdwbn733c2xjv1qln3ukpe0hvce0ylr02s12sv96rus2ohexr9cp8rufbmb1mdb732j1l3kehc0l0s2v6u2hx9prfmu";
        string premixString = "1wz8rzgiv87708l9oi7ot8l9smdqv5yvzz8tavkyuoi9p3kgrrq7r5p53kchnb5hly8jkfx5hsoo6imx8o5ktczwdst8dooa7r331wkrw8zi8789io89mq5vztvyo93gr755khbhyjf5soixokcws8oar3wr";
        int premixCount = 17;

#endregion


    }
}
