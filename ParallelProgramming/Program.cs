using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using System.Diagnostics;

namespace ParallelProgramming
{
    class Program
    {
        static void Main(string[] args)
        {
            // parallel foreach example
            //string[] files = Directory.GetFiles(@"C:\Photos", "*.jpg");
            //string newDir = @"C:\Temp\ModifiedPhotos";
            //Directory.CreateDirectory(newDir);

            //// flip all files upside down
            //Parallel.ForEach(files, (currentFile) =>
            //        {
            //            string filename = Path.GetFileName(currentFile);
            //            var bitmap = new Bitmap(currentFile);

            //            bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
            //            bitmap.Save(Path.Combine(newDir, filename));

            //        });

            //Console.WriteLine("Processing Complete...");


            int cols = 200;
            int rows = 2000;
            int cols2 = 250;

            double[,] matrix1 = InitializeRandomMatrix(rows, cols);
            double[,] matrix2 = InitializeRandomMatrix(cols, cols2);
            double[,] resultMatrix = new double[rows, cols2];

            Stopwatch stopwatch = new Stopwatch();




            Console.WriteLine("Timing sequential execution...");
            stopwatch.Start();
            SequentialMatMul(matrix1, matrix2, resultMatrix);
            stopwatch.Stop();

            Console.Error.WriteLine("Seq matMul time in milliseconds: {0}", stopwatch.ElapsedMilliseconds);





            // reset sw and results
            stopwatch.Reset();
            resultMatrix = new double[rows, cols2];



            Console.WriteLine("Timing Parallel execution...");

            stopwatch.Start();
            ParallelForMatMul(matrix1, matrix2, resultMatrix);
            stopwatch.Stop();

            Console.Error.WriteLine("Parallel matMul time in milliseconds: {0}", stopwatch.ElapsedMilliseconds);

        }

        static void SequentialMatMul(double[,] A, double[,] B, double[,] C)
        {
            int aCols = A.GetLength(1);
            int bCols = B.GetLength(1);
            int aRows = A.GetLength(0);

            for (int i = 0; i < aRows; i++)
            {
                for (int j = 0; j < bCols; j++)
                {
                    double temp = 0;
                    for (int k = 0; k < aCols; k++)
                    {
                        temp += A[i, k] * B[k, j];
                    }
                    C[i, j] += temp;
                }
            }
        }

        static void ParallelForMatMul(double[,] A, double[,] B, double[,] C)
        {
            int aCols = A.GetLength(1);
            int bCols = B.GetLength(1);
            int aRows = A.GetLength(0);

            // outer loop parallel, each row done in parallel
            Parallel.For(0, aRows, i =>
            {
                for (int j = 0; j < bCols; j++)
                {
                    double temp = 0;
                    for (int k = 0; k < aCols; k++)
                    {
                        temp += A[i, k] * B[k, j];
                    }
                    C[i, j] += temp;
                }
            });
        }


        static double[,] InitializeRandomMatrix(int rows, int cols)
        {
            double[,] matrix = new double[rows, cols];

            Random rand = new Random();

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    matrix[i, j] = rand.Next(100);
                }
            }
            return matrix;
        }

        static void PrintMatrix(int rows, int cols, double[,] matrix)
        {
            for (int x = 0; x < rows; x++)
            {
                Console.WriteLine("ROW {0}: ", x);
                for (int y = 0; y < cols; y++)
                {
                    Console.Write("{0:#.##} ", matrix[x, y]);
                }
                Console.WriteLine();
            }
        }

    }
}
