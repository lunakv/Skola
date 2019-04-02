using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Cuni.Arithmetics.FixedPoint;

namespace FixedPointBenchmark
{
    public class Benchmarks<T> where T : Q, new()
    {
        private const int size = 20;

        public Fixed<T>[,] FillMatrix(bool fractionized)
        {
            var matrix = new Fixed<T>[size, size];
            var r = new Random(31415);
            
            /* Fill top diagonal */
            for (int i = 0; i < size; i++)
            {
                for (int j = i; j < size; j++)
                {
                    matrix[i, j] = r.Next(100);
                }    
            }
            
            /* Combine rows to fill rest of the matrix */
            for (int i = 0; i < size - 1; i++)
            {
                for (int j = i + 1; j < size; j++)
                {
                    var coeff = r.Next(1, 4);
                    for (int k = 0; k < size; k++)
                    {
                        matrix[j, k] += coeff * matrix[i, k];
                    }
                }
            }

            if (!fractionized) return matrix;
            
            /* Make the array non-integer for good measure */
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    matrix[i, j] *= new Fixed<T>(100)/new Fixed<T>(99);
                }
            }

            return matrix;
        }

        public void PrintMatrix(Fixed<T>[,] matrix)
        {
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    Console.Write(matrix[i,j] + "\t");
                }

                Console.WriteLine();
            }
        }


        [Benchmark]
        [ArgumentsSource(nameof(GaussData))]
        public Fixed<T>[,] GaussElim(Fixed<T>[,] matrix, Fixed<T>[,] result)
        {
            // result is put to a separate array so that each run has the same input
            for (int p = 0; p < size; p++)
            {
                for (int i = p+1; i < size; i++)
                {
                    var coeff = matrix[i, p] / matrix[p, p];
                    for (int j = p; j < size; j++)
                    {
                        result[i, j] = matrix[i,j] - coeff * matrix[p, j];
                    }
                }
            }
	    return result;
        }

        public Fixed<T>[,] GetRandMatrix()
        {
            var r = new Random(2174);
            var res = new Fixed<T>[size, size];
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    res[i, j] = r.Next(100) * 14 / new Fixed<T>(13);
                }
            }

            return res;
        }

        [Benchmark]
        [ArgumentsSource(nameof(MultData))]
        public void MatrixMult(Fixed<T>[,] a, Fixed<T>[,] b, Fixed<T>[,] res)
        {
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    for (int k = 0; k < size; k++)
                    {
                        res[i, j] += a[i, k] * b[k, j];
                    }
                }
            }
        }

        [Benchmark]
        public Fixed<T> RepeatedAddition()
        {
            var sum = new Fixed<T>(0);
            var inc = new Fixed<T>(7)/new Fixed<T>(5);
            for (int i = 0; i < 10000; i++)
            {
                sum += inc;
            }

            return sum;
        }
        
        [Benchmark]
        public Fixed<T> RepeatedSubtraction()
        {
            var sum = new Fixed<T>(0);
            var inc = new Fixed<T>(7)/new Fixed<T>(5);
            for (int i = 0; i < 10000; i++)
            {
                sum -= inc;
            }

            return sum;
        }
        
        [Benchmark]
        public Fixed<T> RepeatedMultiplication()
        {
            var sum = new Fixed<T>(1);
            var inc = new Fixed<T>(7)/new Fixed<T>(5);
            for (int i = 0; i < 10000; i++)
            {
                sum *= inc;
            }

            return sum;
        }
        
        [Benchmark]
        public Fixed<T> RepeatedDivision()
        {
            var sum = new Fixed<T>(1);
            var inc = new Fixed<T>(5)/new Fixed<T>(7);
            for (int i = 0; i < 10000; i++)
            {
                sum /= inc;
            }

            return sum;
        }

        // Matrix allocation is not included in the measurements.
        public IEnumerable<Fixed<T>[][,]> GaussData()
        {
            yield return new[] {FillMatrix(false), new Fixed<T>[size, size]};
            yield return new[] {FillMatrix(true), new Fixed<T>[size,size]};
        }

        public IEnumerable<Fixed<T>[][,]> MultData()
        {
            yield return new []{GetRandMatrix(), GetRandMatrix(), new Fixed<T>[size, size]};   
        }
    }

    public class BenchmarksDouble
    {
        private const int size = 20;
        
        public double[,] FillMatrix(bool fractionized)
        {
            var matrix = new double[size, size];
            var r = new Random(31415);
            /* Fill top diagonal */
            for (int i = 0; i < size; i++)
            {
                for (int j = i; j < size; j++)
                {
                    matrix[i, j] = r.Next(100);
                }    
            }

            /* Combine rows to fill rest of the matrix */
            for (int i = 0; i < size - 1; i++)
            {
                for (int j = i + 1; j < size; j++)
                {
                    var coeff = r.Next(1, 4);
                    for (int k = 0; k < size; k++)
                    {
                        matrix[j, k] += coeff * matrix[i, k];
                    }
                }
            }

            if (!fractionized) return matrix;
            
            /* Make the array non-integer for good measure */
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    matrix[i, j] *= 100.0/99;
                }
            }

            return matrix;
        }

        public void PrintMatrix(double[,] matrix)
        {
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    Console.Write(matrix[i,j] + "\t");
                }

                Console.WriteLine();
            }
        }


        [Benchmark]
        [ArgumentsSource(nameof(GaussData))]
        public double[,] GaussElim(double[,] matrix, double[,] result)
        {
            for (int p = 0; p < size; p++)
            {
                for (int i = p+1; i < size; i++)
                {
                    var coeff = matrix[i, p] / matrix[p, p];
                    for (int j = p; j < size; j++)
                    {
                        result[i,j] = matrix[i, j] - coeff * matrix[p, j];
                    }
                }
            }
	    return result;
        }

        public double[,] GetRandMatrix()
        {
            var r = new Random(2174);
            var res = new double[size, size];
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    res[i, j] = r.Next(100) * 14.0 / 13;
                }
            }

            return res;
        }
        
        [Benchmark]
        [ArgumentsSource(nameof(MultData))]
        public void MatrixMult(double[,] a, double[,] b, double[,] res)
        {
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    for (int k = 0; k < size; k++)
                    {
                        res[i, j] += a[i, k] * b[k, j];
                    }
                }
            }
        }

        [Benchmark]
        public double RepeatedAddition()
        {
            var sum = 0.0;
            var inc = 7.0 / 5;
            for (int i = 0; i < 10000; i++)
            {
                sum += inc;
            }

            return sum;
        }
        
        [Benchmark]
        public double RepeatedSubtraction()
        {
            var sum = 0.0;
            var inc = 7.0 / 5;
            for (int i = 0; i < 10000; i++)
            {
                sum -= inc;
            }

            return sum;
        }
        
        [Benchmark]
        public double RepeatedMultiplication()
        {
            var sum = 1.0;
            var inc = 7.0 / 5;
            for (int i = 0; i < 10000; i++)
            {
                sum *= inc;
            }

            return sum;
        }
        
        [Benchmark]
        public double RepeatedDivision()
        {
            var sum = 1.0;
            var inc = 5.0 / 7;
            for (int i = 0; i < 10000; i++)
            {
                sum /= inc;
            }

            return sum;
        }

        public IEnumerable<double[][,]> GaussData()
        {
            yield return new []{ FillMatrix(false), new double[size, size] };
            yield return new []{ FillMatrix(true), new double[size, size] };
        }

        public IEnumerable<double[][,]> MultData()
        {
            yield return new[] {GetRandMatrix(), GetRandMatrix(), new double[size, size] };
        }
    }

    
    class Program
    {
        static void Main(string[] args)
        {
            
            BenchmarkRunner.Run<Benchmarks<Q8_24>>();
            BenchmarkRunner.Run<Benchmarks<Q16_16>>();
            BenchmarkRunner.Run<Benchmarks<Q24_8>>();
            BenchmarkRunner.Run<BenchmarksDouble>();
        }
    }
}
