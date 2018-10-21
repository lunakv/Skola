using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NosicABedna
{
    class Vektor
    {
        public int X;
        public int Y;

        public Vektor(int x, int y)
        {
            Y = y;
            X = x;
        }

        public Vektor Posun(Vektor kam)
        {
            return new Vektor(X+kam.X, Y+kam.Y);
        }

        public override bool Equals(object obj)
        {
            if (obj is Vektor pozice)
                return pozice.X == X && pozice.Y == Y;

            return false;
        }
    }

    class Stav
    {
        public Vektor Skladnik;
        public Vektor Bedna;
        public int Vzdalenost;

        public Stav(Vektor skladnik, Vektor bedna, int vzdalenost)
        {
            Skladnik = skladnik;
            Bedna = bedna;
            Vzdalenost = vzdalenost;
        }

        public override string ToString()
        {
            return $"{Skladnik.X}{Skladnik.Y}{Bedna.X}{Bedna.Y}";
        }
    }

    class Prohledavacka
    {
        List<string> seznamProslychStavu = new List<string>();
        private readonly bool[,] bludiste;
        private readonly Vektor cil;
        private Stav aktualniStav;
        private readonly Vektor[] vsechnySmery = {new Vektor(0, 1), new Vektor(0, -1), new Vektor(1, 0), new Vektor(-1, 0)};
                                                    // Dolu               Nahoru             Doprava         Doleva
        Queue<Stav> frontaStavu = new Queue<Stav>();

        public Prohledavacka(Stav pocatecniStav, Vektor cil, bool[,] bludiste)
        {
            this.cil = cil;
            this.bludiste = bludiste;
            aktualniStav = pocatecniStav;
            frontaStavu.Enqueue(aktualniStav);
        }

        bool JeTamZed(Vektor kde)
        {
            return kde.X < 0 || kde.X >= 10 || 
                   kde.Y < 0 || kde.Y >= 10 ||
                   bludiste[kde.X, kde.Y];
        }

        bool JeTamBedna(Vektor kde)
        {
            return kde.Equals(aktualniStav.Bedna);
        }

        bool LzePosunoutBednuSmerem(Vektor smer)
        {
            Vektor posun = aktualniStav.Bedna.Posun(smer);
            return !JeTamZed(posun);
        }
        
        void PohniSeSmerem(Vektor smer)
        {
            Vektor novySkladnik = aktualniStav.Skladnik.Posun(smer);
            Vektor novaBedna = aktualniStav.Bedna;

            if (JeTamZed(novySkladnik) || (JeTamBedna(novySkladnik) && !LzePosunoutBednuSmerem(smer)))
                return;

            if (JeTamBedna(novySkladnik))
                novaBedna = novaBedna.Posun(smer);

            Stav novyStav = new Stav(novySkladnik, novaBedna, aktualniStav.Vzdalenost+1);

            frontaStavu.Enqueue(novyStav);
        }

        public int ProhledejDoSirky()
        {
            while (frontaStavu.Count > 0)
            {
                aktualniStav = frontaStavu.Dequeue();

                if (aktualniStav.Bedna.Equals(cil))
                    return aktualniStav.Vzdalenost;
                
                if (seznamProslychStavu.Contains(aktualniStav.ToString()))
                    continue;
                
                seznamProslychStavu.Add(aktualniStav.ToString());
                foreach (var smer in vsechnySmery)
                {
                    PohniSeSmerem(smer);
                }
            }

            return -1;
        }



    }

    class Program
    {
        static void Main(string[] args)
        {
            Stav start = Reader.PrectiSkladiste(out var bludiste, out var cil);
            Prohledavacka p = new Prohledavacka(start, cil, bludiste);
            Console.WriteLine(p.ProhledejDoSirky());

        }
    }

    

    class Reader
    {
        public static Stav PrectiSkladiste(out bool[,] bludiste, out Vektor cil)
        {
            Vektor skladnik = null, bedna = null;
            
            bludiste = new bool[10,10];
            cil = null;
            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    int c = Console.Read();
                    switch (c)
                    {
                        case 'X':
                            bludiste[x, y] = true;
                            break;
                        case 'C':
                            cil = new Vektor(x, y);
                            break;
                        case 'S':
                            skladnik = new Vektor(x, y);
                            break;
                        case 'B':
                            bedna = new Vektor(x, y);
                            break;
                    }
                }
                Console.ReadLine();
            }
            
            return new Stav(skladnik, bedna, 0);
        }
    }
}
