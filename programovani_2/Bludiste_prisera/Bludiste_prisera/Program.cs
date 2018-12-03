using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace Bludiste_prisera
{
    enum AbsolutniSmer : byte {Nahoru, Doleva, Dolu, Doprava}
    enum RelativniOrientace : byte {Vpred, Vlevo, Vzad, Vpravo}
    class Prisera
    {
        public AbsolutniSmer Smer;
        public  int poziceX;
        public int poziceY;
        public bool[,] bludiste;
        private bool otocilaJsemSeDoprava;

        public Prisera(int x, int y, AbsolutniSmer smer, bool[,] bludiste)
        {
            poziceX = x;
            poziceY = y;
            this.Smer = smer;
            this.bludiste = bludiste;
        }

        public void PohniSe()
        {
            if (otocilaJsemSeDoprava)
            {
                JdiDopredu();
                otocilaJsemSeDoprava = false;
            }
            else if (!JeTamZed(RelativniOrientace.Vpravo))
            {
                OtocSe(RelativniOrientace.Vpravo);
                otocilaJsemSeDoprava = true;
            }
            else if (JeTamZed(RelativniOrientace.Vpred))
            {
                OtocSe(RelativniOrientace.Vlevo);
            }
            else
            {
                JdiDopredu();
            }
        }

        void OtocSe(RelativniOrientace kam)
        {
            Smer = (AbsolutniSmer) (((int) Smer + (int) kam)%4);
        }
        
        void JdiDopredu()
        {
            switch (Smer)
            {
                case AbsolutniSmer.Nahoru:
                    poziceY--;
                    break;
                case AbsolutniSmer.Doleva:
                    poziceX--;
                    break;
                case AbsolutniSmer.Dolu:
                    poziceY++;
                    break;
                case AbsolutniSmer.Doprava:
                    poziceX++;
                    break;
            }
        }

        bool JeTamZed(RelativniOrientace kde)
        {
            AbsolutniSmer absolutniSmer = (AbsolutniSmer) (((int)Smer + (int)kde) % 4);

            switch (absolutniSmer)
            {
                case AbsolutniSmer.Nahoru:
                    return poziceY != 0 && bludiste[poziceY-1, poziceX];
                case AbsolutniSmer.Dolu:
                    return poziceY != bludiste.GetUpperBound(0) && bludiste[poziceY+1, poziceX];
                case AbsolutniSmer.Doleva:
                    return poziceX != 0 && bludiste[poziceY, poziceX-1];
                case AbsolutniSmer.Doprava:
                    return poziceX != bludiste.GetUpperBound(1) && bludiste[poziceY, poziceX+1];
                default:
                    return false;
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            int sirka = int.Parse(Console.ReadLine());
            int vyska = int.Parse(Console.ReadLine());
            Prisera p = Reader.ReadMaze(sirka, vyska);

            for (int i = 0; i < 20; i++)
            {
                p.PohniSe();
                Writer.PrintMaze(p);
            }

        }
    }

    class Reader
    {
        public static Prisera ReadMaze(int sirka, int vyska)
        {
            bool[,] bludiste = new bool[vyska,sirka];
            int x = 0, y = 0;
            AbsolutniSmer s = AbsolutniSmer.Nahoru;
            for (int i = 0; i < vyska; i++)
            {
                for (int j = 0; j < sirka; j++)
                {
                    char c = (char) Console.Read();
                    switch (c)
                    {
                        case 'X':
                            bludiste[i, j] = true;
                            break;
                        case '>':
                            y = i;
                            x = j;
                            s = AbsolutniSmer.Doprava;
                            break;
                        case '<':
                            y = i;
                            x = j;
                            s = AbsolutniSmer.Doleva;
                            break;
                        case '^':
                            y = i;
                            x = j;
                            s = AbsolutniSmer.Nahoru;
                            break;
                        case 'v':
                            y = i;
                            x = j;
                            s = AbsolutniSmer.Dolu;
                            break;
                    }
                }
                Console.ReadLine();
            }

            return new Prisera(x, y, s, bludiste);
        }
    }

    class Writer
    {
        public static void PrintMaze(Prisera p)
        {
            for (int i = 0; i < p.bludiste.GetUpperBound(0)+1; i++)
            {
                for (int j = 0; j < p.bludiste.GetUpperBound(1)+1; j++)
                {
                    if (p.bludiste[i,j])
                        Console.Write("X");
                    else if (p.poziceX == j && p.poziceY == i)
                        PrintPrisera(p);
                    else
                        Console.Write(".");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        static void PrintPrisera(Prisera p)
        {
            switch (p.Smer)
            {
                case AbsolutniSmer.Nahoru:
                    Console.Write("^");
                    break;
                case AbsolutniSmer.Doleva:
                    Console.Write("<");
                    break;
                case AbsolutniSmer.Dolu:
                    Console.Write("v");
                    break;
                case AbsolutniSmer.Doprava:
                    Console.Write(">");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
