using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;

namespace ParallelProgramming
{
    class Program
    {
        private static int _tpSize = 8;
        private static Dictionary<int, int> _tpCalculated = new Dictionary<int, int>();

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


            // Schedule Tasks
            #region UsingTasksAndTaskScheduler
            LCTaskScheduler ts = new LCTaskScheduler(8);
            List<Task> tasks = new List<Task>();

            TaskFactory factory = new TaskFactory(ts);
            CancellationTokenSource cts = new CancellationTokenSource();

            Object lockObj = new Object();
            int outItem = 0;


            for (int tCtr = 0; tCtr <= 8; tCtr++)
            {
                int iteration = tCtr;
                Task t = factory.StartNew(() => {
                    for (int i = 0; i < 8; i++)
                    {
                        lock (lockObj)
                        {
                            Console.Write("{0} in task t-{1} on thread {2}   ",
                                          i, iteration, Thread.CurrentThread.ManagedThreadId);
                            outItem++;
                            if (outItem % 3 == 0)
                                Console.WriteLine();
                        }
                    }
                }, cts.Token);
                tasks.Add(t);
            }

            // like a barrier
            Task.WaitAll(tasks.ToArray());
            cts.Dispose(); // safely delete token
            Console.WriteLine("\n\nTask scheduler and work complete...");
            #endregion


            #region UsingTheThreadPool

            // this is like Assignment: T1 from class
            Console.WriteLine("Doing factorial and summation work with the ThreadPool...\n");


            for (var i = 1; i <= _tpSize; i++)
            {
                ThreadPool.QueueUserWorkItem(DoT1Work, i);
            }

            Thread.Sleep(2000);

            var sortedCalculated = new SortedDictionary<int, int>(_tpCalculated);

            foreach (var calculated in sortedCalculated)
            {
                Console.WriteLine($"In Main: Thread {calculated.Key}'s calculated value: {calculated.Value}");
            }

            // line break
            Console.WriteLine();    
            #endregion


            #region MATMUL
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
            #endregion
        }

        static void DoT1Work(Object stateInfo)
        {
            var threadIndexToUse = (int) stateInfo;
            Console.WriteLine($"Hello from ThreadPool task with managed tid: {Thread.CurrentThread.ManagedThreadId} and user id: {threadIndexToUse}");

            // even
            if (threadIndexToUse % 2 == 0)
            {
                _tpCalculated.Add(threadIndexToUse, CalculateSummationTo(threadIndexToUse));
            }
            // odd
            else
            {
                _tpCalculated.Add(threadIndexToUse, CalculateFactorial(threadIndexToUse));
            }
        }

        static int CalculateSummationTo(int val)
        {
            var retValue = 0;
            for (var i = 1; i <= val; i++)
            {
                retValue += i;
            }

            return retValue;
        }

        static int CalculateFactorial(int val)
        {
            var retValue = 1;
            for (var i = 1; i <= val; i++)
            {
                retValue *= i;
            }

            return retValue;
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
