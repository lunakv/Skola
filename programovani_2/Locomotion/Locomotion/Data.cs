using System;
using System.Drawing;

namespace Locomotion
{
    //Typy kolejí
    // H - horizontal, V - vertical, U - up, D - down, L - left, R - right
    public enum Type {H, V, UL, DL, UR, DR, All, Null}

    public static class Maps
    {
        //Mapy všech úrovní
        public static readonly Type[,] London =
        {
            {Type.Null, Type.H, Type.H, Type.H, Type.H, Type.H, Type.H, Type.DL},
            {Type.V, Type.DR, Type.H, Type.H, Type.H, Type.H, Type.DL, Type.V},
            {Type.V, Type.UR, Type.H, Type.H, Type.H, Type.H, Type.UL, Type.V},
            {Type.UR, Type.H, Type.H, Type.H, Type.H, Type.H, Type.H, Type.UL},
            {Type.DR, Type.H, Type.H, Type.DL, Type.DR, Type.H, Type.H, Type.DL},
            {Type.V, Type.All, Type.All, Type.V, Type.V, Type.All, Type.All, Type.V},
            {Type.UR, Type.H, Type.H, Type.UL, Type.UR, Type.H, Type.H, Type.UL}
        };

        public static readonly Type[,] Paris =
        {
            {Type.Null, Type.DL, Type.UL, Type.DR, Type.All, Type.All, Type.DR, Type.V},
            {Type.V, Type.V, Type.DL, Type.UR, Type.DL, Type.V, Type.V, Type.V},
            {Type.V, Type.DR, Type.V, Type.V, Type.V, Type.UL, Type.H, Type.UL},
            {Type.DL, Type.H, Type.H, Type.UL, Type.V, Type.V, Type.UR, Type.UL},
            {Type.UL, Type.DL, Type.H, Type.DL, Type.H, Type.H, Type.V, Type.UL},
            {Type.V, Type.UR, Type.DL, Type.V, Type.DL, Type.DR, Type.DR, Type.All},
            {Type.V, Type.DR, Type.UL, Type.UL, Type.DR, Type.All, Type.UL, Type.H}
        };

        public static readonly Type[,] Brussels =
        {
            {Type.Null, Type.V, Type.DR, Type.UR, Type.H, Type.DR, Type.All, Type.DR},
            {Type.V, Type.UL, Type.UL, Type.V, Type.V, Type.H, Type.UL, Type.V},
            {Type.H, Type.H, Type.DL, Type.DL, Type.UL, Type.All, Type.DL, Type.V},
            {Type.DL, Type.All, Type.UL, Type.DR, Type.DL, Type.V, Type.DR, Type.H},
            {Type.V, Type.H, Type.H, Type.H, Type.DL, Type.UL, Type.DL, Type.UL},
            {Type.DL, Type.UL, Type.DL, Type.All, Type.UR, Type.DR, Type.UR, Type.H},
            {Type.UR, Type.UR, Type.UL, Type.DL, Type.V, Type.All, Type.H, Type.All}
        };

        public static readonly Type[,] Amsterdam =
        {
            {Type.Null, Type.UL, Type.All, Type.All, Type.DL, Type.H, Type.UR, Type.UR},
            {Type.UR, Type.UR, Type.H, Type.UR, Type.V, Type.UR, Type.UL, Type.DR},
            {Type.DR, Type.DL, Type.DR, Type.UL, Type.UR, Type.V, Type.UR, Type.H},
            {Type.V, Type.UR, Type.DR, Type.V, Type.All, Type.UR, Type.UR, Type.H},
            {Type.DR, Type.All, Type.All, Type.All, Type.H, Type.DL, Type.DR, Type.DR},
            {Type.All, Type.H, Type.H, Type.UR, Type.UR, Type.UR, Type.UL, Type.UR},
            {Type.V, Type.H, Type.UR, Type.DL, Type.UR, Type.UR, Type.V, Type.All}
        };

        public static readonly Type[,] Bonn =
        {
            {Type.Null, Type.UR, Type.DR, Type.UL, Type.V, Type.All, Type.DL, Type.UL},
            {Type.UL, Type.UR, Type.V, Type.DR, Type.UL, Type.H, Type.UR, Type.H},
            {Type.DL, Type.UL, Type.UR, Type.DL, Type.H, Type.UL, Type.UR, Type.All},
            {Type.V, Type.H, Type.UL, Type.UR, Type.DR, Type.V, Type.UL, Type.DL},
            {Type.UL, Type.V, Type.DL, Type.H, Type.UR, Type.All, Type.DR, Type.V},
            {Type.UR, Type.DR, Type.H, Type.DR, Type.V, Type.H, Type.H, Type.UR},
            {Type.All, Type.UL, Type.V, Type.All, Type.DL, Type.H, Type.H, Type.UR}
        };

        public static readonly Type[,] Vienna =
        {
            {Type.Null, Type.H, Type.UL, Type.DR, Type.H, Type.DL, Type.UR, Type.UL},
            {Type.UR, Type.UR, Type.UR, Type.V, Type.DL, Type.DL, Type.UL, Type.UL},
            {Type.UR, Type.V, Type.DR, Type.V, Type.V, Type.All, Type.UR, Type.DL},
            {Type.UL, Type.UR, Type.UR, Type.DR, Type.V, Type.H, Type.UR, Type.UL},
            {Type.V, Type.DR, Type.DL, Type.DL, Type.V, Type.V, Type.DL, Type.UR},
            {Type.UR, Type.H, Type.UL, Type.V, Type.H, Type.DL, Type.H, Type.H},
            {Type.DR, Type.DL, Type.UR, Type.DL, Type.UL, Type.DL, Type.DR, Type.DR}
        };

        public static readonly Type[,] Bern =
        {
            {Type.Null, Type.H, Type.H, Type.V, Type.UR, Type.UL, Type.V, Type.DL},
            {Type.UL, Type.All, Type.UR, Type.UR, Type.DR, Type.DR, Type.UR, Type.V},
            {Type.All, Type.DR, Type.V, Type.UR, Type.V, Type.All, Type.UR, Type.V},
            {Type.All, Type.DR, Type.All, Type.V, Type.DR, Type.DL, Type.V, Type.V},
            {Type.H, Type.DL, Type.All, Type.All, Type.UL, Type.DL, Type.H, Type.UR},
            {Type.UR, Type.DL, Type.DR, Type.UR, Type.H, Type.All, Type.H, Type.H},
            {Type.UL, Type.DR, Type.All, Type.V, Type.All, Type.All, Type.DL, Type.UR}
        };

        public static readonly Type[,] Roma =
        {
            {Type.Null, Type.H, Type.V, Type.UL, Type.All, Type.V, Type.UL, Type.UL},
            {Type.DR, Type.DL, Type.DR, Type.All, Type.V, Type.DR, Type.UL, Type.UL},
            {Type.V, Type.H, Type.DR, Type.H, Type.UL, Type.DL, Type.H, Type.UL},
            {Type.DR, Type.H, Type.DL, Type.DL, Type.H, Type.UR, Type.UL, Type.DL},
            {Type.UL, Type.DL, Type.DL, Type.DL, Type.DR, Type.H, Type.DR, Type.UR},
            {Type.DL, Type.All, Type.All, Type.All, Type.All, Type.UL, Type.DL, Type.V},
            {Type.DL, Type.H, Type.UR, Type.DR, Type.V, Type.UL, Type.V, Type.DR}
        };

        public static readonly Type[,] Madrid =
        {
            {Type.Null, Type.DR, Type.V, Type.H, Type.UR, Type.H, Type.UL, Type.UR},
            {Type.V, Type.V, Type.V, Type.UR, Type.DL, Type.UR, Type.UL, Type.V},
            {Type.UL, Type.DR, Type.All, Type.DR, Type.UR, Type.UL, Type.H, Type.DR},
            {Type.V, Type.V, Type.DR, Type.V, Type.All, Type.V, Type.UL, Type.DR},
            {Type.UR, Type.DL, Type.DL, Type.UL, Type.UR, Type.DL, Type.V, Type.DL},
            {Type.DL, Type.H, Type.All, Type.UR, Type.UL, Type.DL, Type.UL, Type.UL},
            {Type.DL, Type.H, Type.V, Type.All, Type.V, Type.V, Type.DR, Type.DL}
        };

        //Vrátí úroveň odpovídající dané úrovni
        public static Type[,] GetMap(int level)
        {
            switch (level)
            {
                case 1:
                    return London;
                case 2:
                    return Paris;
                case 3:
                    return Brussels;
                case 4:
                    return Amsterdam;
                case 5:
                    return Bonn;
                case 6:
                    return Vienna;
                case 7:
                    return Bern;
                case 8:
                    return Roma;
                case 9:
                    return Madrid;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        //Vrátí jméno mapy odpovídající úrovně
        public static string GetMapName(int level)
        {
            switch (level)
            {
                case 1:
                    return "London";
                case 2:
                    return "Paris";
                case 3:
                    return "Brussels";
                case 4:
                    return "Amsterdam";
                case 5:
                    return "Bonn";
                case 6:
                    return "Vienna";
                case 7:
                    return "Bern";
                case 8:
                    return "Roma";
                case 9:
                    return "Madrid";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
    }

    public static class Directions
    {
        //Konverze ze směru do souřadnic a zpět
        //0>, 1^, 2<, 3v; x zleva doprava, y shora dolů
        public static int DirToX(int direction)
        {
            return (1 - direction) * ((direction + 1) % 2);
        }

        public static int DirToY(int direction)
        {
            return (direction - 2) * (direction % 2);
        }

        public static int CoordToDir(int x, int y)
        {
            return (1 - x) * ((x + 2) % 2) + (y + 2) * ((y + 2) % 2);
        }

        //Příští směr vlaku přijíždějícího z daného směru na danou kolej
        public static int NextTrainDir(int direction, Track t)
        {
            switch (t.MyType)
            {
                case Type.H:
                case Type.V:
                case Type.All:
                    return direction;
                case Type.UL:
                    return direction == 0 ? 1 : 2;
                case Type.DL:
                    return direction == 0 ? 3 : 2;
                case Type.UR:
                    return direction == 2 ? 1 : 0;
                case Type.DR:
                    return direction == 2 ? 3 : 0;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        //Vrátí, zda se dá daná kolej navštívit z daného směru
        public static bool CanBeAccessed(int direction, Track t)
        {
            if (t == null) return false;

            switch (t.MyType)
            {
                case Type.H:
                    return direction == 0 || direction == 2;
                case Type.V:
                    return direction == 1 || direction == 3;
                case Type.UL:
                    return direction == 0 || direction == 3;
                case Type.DL:
                    return direction == 0 || direction == 1;
                case Type.UR:
                    return direction == 3 || direction == 2;
                case Type.DR:
                    return direction == 1 || direction == 2;
                case Type.All:
                    return true;
                case Type.Null:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public static class Images
    {
        //Vrátí obrázek daného typu koleje
        public static Image GetTrackImage(Type type)
        {
            Image result = Properties.Resources.UL;
            switch (type)
            {  
                case Type.All:
                    return Properties.Resources.All;
                case Type.Null:
                    return Properties.Resources.Danger;
                case Type.H:
                    result = Properties.Resources.H;
                    break;
                case Type.V:
                    result = Properties.Resources.H;
                    result.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    break;
                case Type.UL:
                    break;
                case Type.DL:
                    result.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    break;
                case Type.UR:
                    result.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    break;
                case Type.DR:
                    result.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            return result;
        }

        //Vrátí obrázek správně natočeného vlaku
        public static Image GetTrainImage(int direction, Type type)
        {
            Image result = Properties.Resources.VlakLU; // Vlak jede na UL koleji zleva nahoru

            if (type == Type.H || type == Type.V || type == Type.All)
            {
                result = Properties.Resources.VlakU;    // Vlak jede na V koleji zdola nahoru
            }
            
            switch (direction)
            {
                case 0:
                    result.RotateFlip(type == Type.DR ? RotateFlipType.Rotate90FlipY : RotateFlipType.Rotate90FlipNone);
                    break;
                case 1:
                    result.RotateFlip(type == Type.UR ? RotateFlipType.RotateNoneFlipX : RotateFlipType.RotateNoneFlipNone);
                    break;
                case 2:
                    result.RotateFlip(type == Type.UL ? RotateFlipType.Rotate270FlipY : RotateFlipType.Rotate270FlipNone);
                    break;
                case 3:
                    result.RotateFlip(type == Type.DL ? RotateFlipType.Rotate180FlipX : RotateFlipType.Rotate180FlipNone);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return result;
        }
    }
}