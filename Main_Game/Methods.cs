
using System;
using System.Collections.Generic;
using System.Linq;
namespace Methods
{
    #pragma warning disable CS8603
    public static class Method
    {
        /// <summary>
        /// Megadja, hogy hány termet tartamaz a térkép
        /// </summary>
        /// <param name="map">Labirintus mátrixa</param>
        /// <returns>Termek száma</returns>
        public static int GetRoomNumber(char[,] map)
        {
            int noOfRooms= 0;
            foreach(char items in map) 
            {
                if(items.Equals('█')) noOfRooms++;
            }

            return noOfRooms;
        }
        /// <summary>
        /// A kapott térkép széleit végignézve megállapítja, hogy hány kijárat van.
        /// </summary>
        /// <param name="map">Labirintus mátrixa</param>
        /// <returns>Az alkalmas kijáratok száma</returns>
        public static int GetSuitableEntrance(char[,] map)
        {
            if(map is null) return -1;
            int NumberOfExits = default;
            for(int row = 0; row < map.GetLength(0);row++) 
            {
                for (int col = 0; col < map.GetLength(1); col++)
                {
                    if (col == 0)
                    {
                        if (map[row, col] == '═') NumberOfExits++;
                    }
                    if (row == 0)
                    {
                        if (map[row, col] == '║') NumberOfExits++;
                    }
                    if (col == map.GetLength(1)-1)
                    {
                        if (map[row, col] == '═') NumberOfExits++;
                    }

                    if (row == map.GetLength(0)-1)
                    {
                        if (map[row, col] == '║') NumberOfExits++;
                    }

                }
            }
            return NumberOfExits;
        }
        /// <summary>
        /// Megnézi, hogy van-e a térképen meg nem engedett karakter?
        /// </summary>
        /// <param name="map">Labirintus mátrixa</param>
        /// <returns>true - A térkép tartalmaz szabálytalan karaktert, false - nincs benne ilyen</returns>
        static bool IsInvalidElement(char[,] map)
        {
            char[] ValidElements = new char[]{'╬','═','╦','╩','║','╣','╠','╗','╝','╚', '╔','.','█'};
            foreach(char item in map) 
            {
                if(!(Array.Exists(ValidElements,elem => elem == item)))
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// Visszaadja azoknak a járatkaraktereknek a pozícióját, amelyekhez egyetlen szomszéd pozícióból sem lehet eljutni.
        /// </summary>
        /// <param name="map">Labirintus mátrixa</param>
        /// <returns>A pozíciók "sor_index:oszlop_index" formátumban szerepelnek a lista elemeiként
        static List<string> GetUnavailableElements(char[,] map)
        {
            List<string> unavailables = new List<string>();
            // ?
            // pld: string poz = "4:12"; 
            return unavailables;
        }
        /// <summary>
        /// Labiritust generál a kapott pozíciókat tartalmazó lista alapján. A lista elemei egymáshoz kapcsolódó járatok pozíciói.
        /// </summary>
        /// <param name="positionsList">"sor_index:oszlop_index" formátumban az egymáshoz kapcsolódó járatok pozícióit tartalmazó lista </param>
        /// <returns>A létrehozott labirintus térképe</returns>
        static char[,] GenerateLabyrinth(List<string> positionsList)
        {
            return null;    
        }


        
    }
}
