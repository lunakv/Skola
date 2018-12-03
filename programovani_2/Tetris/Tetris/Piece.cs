using System;
using System.Collections.Generic;
using System.Drawing;

namespace Tetris
{
    enum PieceType : byte { I, O, J, L, S, T, Z}
    enum Direction : byte {Left, Right}

    class PieceData
    {
        #region Boxes
        //I
        public static readonly byte[,] IBox =
        {
            {0, 0, 0, 0}, {1, 1, 1, 1}, {0, 0, 0, 0}, {0, 0, 0, 0}
        };
        
        //O
        public static readonly byte[,] OBox =
        {
                {0, 0, 0, 0}, {0, 1, 1, 0}, {0, 1, 1, 0}, {0, 0, 0, 0}
        };
       
        //J
        public static readonly byte[,] JBox =
        {
            {1, 0, 0}, {1, 1, 1}, {0, 0, 0}
        };

        //L
        public static readonly byte[,] LBox =
        {
            {0, 0, 1}, {1, 1, 1}, {0, 0, 0}
        };

        //S
        public static readonly byte[,] SBox =
        {
            {0, 1, 1}, {1, 1, 0}, {0, 0, 0}
        };

        //T
        public static readonly byte[,] TBox =
        {
            {0, 1, 0}, {1, 1, 1}, {0, 0, 0}
        };

        //Z
        public static readonly byte[,] ZBox =
        {
            {1, 1, 0}, {0, 1, 1}, {0, 0, 0}
        };
       
        public static readonly byte[][,] AllBoxes =
        {
            IBox, OBox, JBox, LBox, SBox, TBox, ZBox
        };
        #endregion

        public static readonly int[,,] IWallKicks =
        {
            //0>>1
            {
                {0, 0}, {-2, 0}, {1, 0}, {-2, -1}, {1, 2}
            },
            //1>>2
            {
                {0, 0}, {-1, 0}, {2, 0}, {-1, 2}, {2, -1}
            },
            //2>>3
            {
                {0, 0}, {2, 0}, {-1, 0}, {2, 1}, {-1, -2}
            },
            //3>>0
            {
                {0, 0}, {1, 0}, {-2, 0}, {1, -2}, {-2, 1}
            }
        };

        public static readonly int[,,] OtherWallKicks =
        {
            //0>>1
            {
                {0, 0}, {-1, 0}, {-1, 1}, {0, -2}, {-1, -2}
            },
            //1>>2
            {
                {0, 0}, {1, 0}, {1, -1}, {0, 2}, {1, 2}
            },
            //2>>3
            {
                {0, 0}, {1, 0}, {1, 1}, {0, -2}, {1, -2}
            },
            //3>>0
            {
                {0, 0}, {-1, 0}, {-1, -1}, {0, 2}, {-1, 2}
            }
        };


        public static byte[,] Rotate(byte[,] box)
        {
            int n = box.GetLength(0);
            byte[,] res = new byte[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n ; j++)
                {
                    res[i, j] = box[n - j - 1, i];
                }
            }

            return res;
        }
    }

    class Piece
    {
        
        /*
        public static readonly byte[][][][] AllBoxes =
        {
            //I
            new []{
                new []{
                    new byte[]{0, 0, 0, 0}, new byte[]{0, 0, 0, 0}, new byte[]{1, 1, 1, 1}, new byte[]{0, 0, 0, 0}
                },
                new []{
                    new byte[]{0, 0, 1, 0}, new byte[]{0, 0, 1, 0}, new byte[]{0, 0, 1, 0}, new byte[]{0, 0, 1, 0}
                },
                new []{
                    new byte[]{0, 0, 0, 0}, new byte[]{0, 0, 0, 0}, new byte[]{1, 1, 1, 1}, new byte[]{0, 0, 0, 0}
                },
                new []{
                    new byte[]{0, 0, 1, 0}, new byte[]{0, 0, 1, 0}, new byte[]{0, 0, 1, 0}, new byte[]{0, 0, 1, 0}
                }
            },

            //O
           new []{
               new []{
                    new byte[]{0, 0, 0, 0}, new byte[]{0, 1, 1, 0}, new byte[]{0, 1, 1, 0}, new byte[]{0, 0, 0, 0}
                },
               new []{
                    new byte[]{0, 0, 0, 0}, new byte[]{0, 1, 1, 0}, new byte[]{0, 1, 1, 0}, new byte[]{0, 0, 0, 0}
                },
               new []{
                    new byte[]{0, 0, 0, 0}, new byte[]{0, 1, 1, 0}, new byte[]{0, 1, 1, 0}, new byte[]{0, 0, 0, 0}
                },
               new []{
                    new byte[]{0, 0, 0, 0}, new byte[]{0, 1, 1, 0}, new byte[]{0, 1, 1, 0}, new byte[]{0, 0, 0, 0}
                }
            },

            //J
           new []{
               new []{
                    new byte[]{0, 0, 0, 0}, new byte[]{0, 0, 0, 0}, new byte[]{0, 1, 1, 1}, new byte[]{0, 0, 0, 1}
                },
               new []{
                    new byte[]{0, 0, 0, 0}, new byte[]{0, 0, 1, 0}, new byte[]{0, 0, 1, 0}, new byte[]{0, 1, 1, 0}
                },
               new []{
                    new byte[]{0, 0, 0, 0}, new byte[]{0, 1, 0, 0}, new byte[]{0, 1, 1, 1}, new byte[]{0, 0, 0, 0}
                },
               new []{
                    new byte[]{0, 0, 0, 0}, new byte[]{0, 0, 1, 1}, new byte[]{0, 0, 1, 0}, new byte[]{0, 0, 1, 0}
                }
            },

            //L
           new []{
               new []{
                    new byte[]{0, 0, 0, 0}, new byte[]{0, 0, 0, 0}, new byte[]{0, 1, 1, 1}, new byte[]{0, 1, 0, 0}
                },
               new []{
                    new byte[]{0, 0, 0, 0}, new byte[]{0, 1, 1, 0}, new byte[]{0, 0, 1, 0}, new byte[]{0, 0, 1, 0}
                },
               new []{
                    new byte[]{0, 0, 0, 0}, new byte[]{0, 0, 0, 1}, new byte[]{0, 1, 1, 1}, new byte[]{0, 0, 0, 0}
                },
               new []{
                    new byte[]{0, 0, 0, 0}, new byte[]{0, 0, 1, 0}, new byte[]{0, 0, 1, 0}, new byte[]{0, 0, 1, 1}
                }
            },

            //S
           new []{
               new []{
                    new byte[]{0, 0, 0, 0}, new byte[]{0, 0, 0, 0}, new byte[]{0, 0, 1, 1}, new byte[]{0, 1, 1, 0}
                },
               new []{
                    new byte[]{0, 0, 0, 0}, new byte[]{0, 0, 1, 0}, new byte[]{0, 0, 1, 1}, new byte[]{0, 0, 0, 1}
                },
               new []{
                    new byte[]{0, 0, 0, 0}, new byte[]{0, 0, 0, 0}, new byte[]{0, 0, 1, 1}, new byte[]{0, 1, 1, 0}
                },
               new []{
                    new byte[]{0, 0, 0, 0}, new byte[]{0, 0, 1, 0}, new byte[]{0, 0, 1, 1}, new byte[]{0, 0, 0, 1}
                }
            },

            //T
           new []{
               new []{
                    new byte[]{0, 0, 0, 0}, new byte[]{0, 0, 0, 0}, new byte[]{0, 1, 1, 1}, new byte[]{0, 0, 1, 0}
                },
               new []{
                    new byte[]{0, 0, 0, 0}, new byte[]{0, 0, 1, 0}, new byte[]{0, 1, 1, 0}, new byte[]{0, 0, 1, 0}
                },
               new []{
                    new byte[]{0, 0, 0, 0}, new byte[]{0, 0, 1, 0}, new byte[]{0, 1, 1, 1}, new byte[]{0, 0, 0, 0}
                },
               new []{
                    new byte[]{0, 0, 0, 0}, new byte[]{0, 0, 1, 0}, new byte[]{0, 0, 1, 1}, new byte[]{0, 0, 1, 0}
                }
            },

            //Z
           new []{
               new []{
                    new byte[]{0, 0, 0, 0}, new byte[]{0, 0, 0, 0}, new byte[]{0, 1, 1, 0}, new byte[]{0, 0, 1, 1}
                },
               new []{
                    new byte[]{0, 0, 0, 0}, new byte[]{0, 0, 0, 1}, new byte[]{0, 0, 1, 1}, new byte[]{0, 0, 1, 0}
                },
               new []{
                    new byte[]{0, 0, 0, 0}, new byte[]{0, 0, 0, 0}, new byte[]{0, 1, 1, 0}, new byte[]{0, 0, 1, 1}
                },
               new []{
                    new byte[]{0, 0, 0, 0}, new byte[]{0, 0, 0, 1}, new byte[]{0, 0, 1, 1}, new byte[]{0, 0, 1, 0}
                }
            }
        };
        */

        public PieceType MyType;
        public Color MyColor;
        private GameBoard _myBoard;
        public int Orientation;
        public int ulX, ulY;
        public byte[,] myBox;
        public int boxSize;

        public Piece(PieceType type, GameBoard myBoard)
        {
            MyType = type;
            _myBoard = myBoard;
            Orientation = 0;
            myBox = PieceData.AllBoxes[(int) MyType];
            boxSize = MyType == PieceType.I || MyType == PieceType.O ? 4 : 3;

            ulX = 3;
            ulY = 0;

            SetColor();
        }

        void SetColor()
        {
            switch (MyType)
            {
                case PieceType.I:
                    MyColor = Color.Aqua;
                    break;
                case PieceType.O:
                    MyColor = Color.Yellow;
                    break;
                case PieceType.J:
                    MyColor = Color.Blue;
                    break;
                case PieceType.L:
                    MyColor = Color.Orange;
                    break;
                case PieceType.S:
                    MyColor = Color.Green;
                    break;
                case PieceType.T:
                    MyColor = Color.Purple;
                    break;
                case PieceType.Z:
                    MyColor = Color.Red;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Fall()
        {
            if (_myBoard.CanFall())
                ulY++;
        }

        public void Rotate()
        {
            if (_myBoard.CanRotate())
            {
                Orientation = (Orientation + 1) % 4;
                myBox = PieceData.Rotate(myBox);
            }

        }

        public void Shift(Direction d)
        {
            if (_myBoard.CanShift(d))
            {
                ulX += (byte) d * 2 - 1;
            }
        }
    }


}
