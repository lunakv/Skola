using System;

namespace TabulkaPismen
{
    class HledacRetezcu
    {
        private char[,] _tabulkaZnaku;   //tabulka, po ktere se pohybujeme
        private int[,] _minimaVzdalenosti;      //tabulka delek minimalni cesty ke znaku
        private int _sirka, _vyska;       //sirka a vyska tabulek
        

        public HledacRetezcu(char[,] tabulka)
        {
            _tabulkaZnaku = tabulka;
            _sirka = _tabulkaZnaku.GetUpperBound(0)+1;
            _vyska = _tabulkaZnaku.GetUpperBound(1)+1;
            _minimaVzdalenosti = new int[_sirka, _vyska];
        }

        /// <summary>
        /// Najde minimalni pocet kroku nutny k zapsani retezce v tabulce znaku
        /// </summary>
        /// <param name="hledanyString">Cilovy retezec</param>
        /// <returns><value>int.MaxValue</value>, pokud retezec nejde zapsat, jinak pocet kroku nutny k zapsani</returns>
        public int NajdiVzdalenostRetezce(string hledanyString)
        {
            int posledni = 0;
            while (posledni < hledanyString.Length && !ExistujeZnak(hledanyString[posledni]))
                posledni++;

            if (posledni < hledanyString.Length)
                NajdiPristiZnaky(0, 0, hledanyString[posledni]);
            int hledany = posledni + 1;
            
            while (hledany < hledanyString.Length)
            {
                char posledniChar = hledanyString[posledni];
                char hledanyChar = hledanyString[hledany];

                if (!ExistujeZnak(hledanyChar) || hledany == posledni)
                {
                    hledany++;
                    continue;
                }

                if (!ExistujeZnak(posledniChar))
                {
                    posledni++;
                    continue;
                }
           
                // z poslednich znaku podretezce najdeme nejkratsi cesty do dalsiho znaku a prodlouzime podretezec o 1
                AktualizujMinima(posledniChar, hledanyChar);

                // je potreba si pamatovat pouze delky cest do posledniho znaku
                ResetujMinima(hledanyChar);

                posledni++;
                hledany++;

            }
            
            // po skonceni najdeme nejmensi delku cesty 
            int minimum = -1;
            foreach (var i in _minimaVzdalenosti)
            {
                // nenulove jsou jen znaky ukoncujici retezec 
                if (i != 0 && (minimum > i || minimum == -1))
                    minimum = i;
            }
            
            return minimum;
            
        }
        
        bool ExistujeZnak(char c)
        {
            foreach (var i in _tabulkaZnaku)
            {
                if (i == c)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Spusti prohledavani ze vsech znaku <paramref name="start"/> do vsech znaku <paramref name="cil"/>
        /// </summary>
        /// <param name="start">znak, ze ktereho vychazi prohledavani</param>
        /// <param name="cil">hledany znak</param>
        void AktualizujMinima(char start, char cil)
        {   
            for (int i = 0; i < _sirka; i++)
            {
                for (int j = 0; j < _vyska; j++)
                {
                   if (_tabulkaZnaku[i, j] == start)
                        NajdiPristiZnaky(i, j, cil);
                }


            }
        }

        /// <summary>
        /// Hleda nejkratsi cestu vsech cilovych prvku tabulky z vychozich souradnic
        /// </summary>
        /// <param name="x">horizontalni souradnice vychoziho prvku</param>
        /// <param name="y">vertikalni souradnice vychoziho prvku</param>
        /// <param name="cil">hledany znak</param>
        void NajdiPristiZnaky(int x, int y, char cil)
        {
            for (int i = 0; i < _sirka; i++)
            {
                for (int j = 0; j < _vyska; j++)
                {
                    if (_tabulkaZnaku[i, j] != cil) continue;

                    if (x == i && y == j)
                    {
                        _minimaVzdalenosti[i, j]++;
                        continue;
                    }

                    int vzdalenost = _minimaVzdalenosti[x,y] + Math.Abs(x - i) + Math.Abs(y - j);
                    if (_minimaVzdalenosti[i, j] >= vzdalenost || _minimaVzdalenosti[i,j] == 0)
                        _minimaVzdalenosti[i, j] = vzdalenost + 1;
                }
            }
            
        }

        /// <summary>
        /// Vynuluje hodnoty vsech znaku krome posledniho znaku podretezce
        /// </summary>
        /// <param name="c">zachovany znak</param>
        void ResetujMinima(char c)
        {
            for (int i = 0; i < _sirka; i++)
            {
                for (int j = 0; j < _vyska; j++)
                {
                    if (_tabulkaZnaku[i, j] != c)
                        _minimaVzdalenosti[i, j] = 0;
                }
            }
        }


    }
    
    class Program
    {
        static void Main()
        {
            int vyska = int.Parse(Console.ReadLine());
            int sirka = int.Parse(Console.ReadLine());
            char[,] tabulka = new char[sirka, vyska];
            Reader.ReadTable(tabulka);
            string retezec = Console.ReadLine();
            HledacRetezcu f = new HledacRetezcu(tabulka);
            Console.WriteLine(f.NajdiVzdalenostRetezce(retezec));
        }
    }

    class Reader
    {
        public static int ReadNumber()
        {
            int vysledek = 0;
            int c = Console.Read();
            bool neg = false;

            while ((c < '0') || (c > '9'))
            {
                neg = (c == '-');

                c = Console.Read();
            }

            while ((c >= '0') && (c <= '9'))
            {
                vysledek = 10 * vysledek + c - '0';
                c = Console.Read();
            }

            if (neg)
            {
                vysledek = -vysledek;
            }
            return vysledek;
        }

        public static void ReadTable(char[,] table)
        {
            string tbDef = Console.ReadLine();
            for (int i = 0; i <= table.GetUpperBound(0) ; i++)
            {
                for (int j = 0; j <= table.GetUpperBound(1); j++)
                {
                    if (string.IsNullOrEmpty(tbDef)) break;

                    table[i, j] = tbDef[0];
                    tbDef = tbDef.Substring(1);
                }
            }
        }
    }
}
