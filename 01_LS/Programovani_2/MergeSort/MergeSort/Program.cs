using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MergeSort
{
    class Sorter
    {
        public static void MergeSort(ref int[] toSort)
        {
            int[] moreSorted = new int[toSort.Length];
            int leftPartIndex, rightPartIndex, moreIndex;
            int rightStart, rightEnd;

            /* Iterativně slévá čím dál delší posloupnosti */
            for (int partSize = 1; partSize < toSort.Length; partSize *= 2)
            {
                
                /* Posloupnosti dané velikosti se postupně slijí v celém poli */
                for (int leftStart = 0; leftStart + partSize < toSort.Length; leftStart += 2*partSize)
                {
                    rightStart = leftStart + partSize;
                    rightEnd = rightStart + partSize;
                    if (rightEnd > toSort.Length)
                        rightEnd = toSort.Length;
                    
                    leftPartIndex = leftStart;
                    rightPartIndex = rightStart;
                    moreIndex = leftStart;

                    /* Slévání */
                    while (leftPartIndex < rightStart && rightPartIndex < rightEnd)
                    {
                        if (toSort[leftPartIndex] < toSort[rightPartIndex])
                            moreSorted[moreIndex++] = toSort[leftPartIndex++];
                        else
                            moreSorted[moreIndex++] = toSort[rightPartIndex++];
                    }

                    while (leftPartIndex < rightStart)
                        moreSorted[moreIndex++] = toSort[leftPartIndex++];
                    while (rightPartIndex < rightEnd)
                        moreSorted[moreIndex++] = toSort[rightPartIndex++];
                }

                toSort = (int[]) moreSorted.Clone();
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            int[] toSort = new int[10000];

            Random r = new Random();
            for (int i = 0; i < toSort.Length; i++)
            {
                toSort[i] = r.Next(10000);
            }

            Sorter.MergeSort(ref toSort);
            foreach (var i in toSort)
            {
                Console.WriteLine(i);
            }
        }
    }
}
