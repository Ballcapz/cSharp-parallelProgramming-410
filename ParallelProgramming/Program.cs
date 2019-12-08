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
        private static int _arrSize = _tpSize + 1;
        //private static Dictionary<int, int> _tpCalculated = new Dictionary<int, int>();
        private  static int[] _tpCalculated = new int[_arrSize];

        static void Main(string[] args)
        {
            #region Parallel For Example

            // parallel foreach example, flips images in \|/ directory upside down
            string[] files = Directory.GetFiles(@"C:\Photos", "*.jpg");
            string newDir = @"C:\Temp\ModifiedPhotos";
            Directory.CreateDirectory(newDir);

            // flip all files upside down
            Parallel.ForEach(files, (currentFile) =>
            {
                string filename = Path.GetFileName(currentFile);
                var bitmap = new Bitmap(currentFile);

                bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                bitmap.Save(Path.Combine(newDir, filename));

            });

            Console.WriteLine("Photo processing Complete...");

            #endregion


            #region Using Tasks And A TaskScheduler
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
            Console.WriteLine("\nTask scheduler and work complete...\n\n");
            #endregion

            #region T1 with tasks
            // like assignment T1 from class
            // Tasks include more methods, such as WaitAll, which is like a barrier
            
            Task[] taskArray = new Task[_tpSize];

            for (var i = 0; i < taskArray.Length; i++)
            {
                taskArray[i] = Task.Factory.StartNew((Object obj) => { DoT1Work(obj); }, i);
            }

            Task.WaitAll(taskArray);

            int sum = 0;
            for (int i = 0; i < taskArray.Length; i++)
            {
                Console.WriteLine($"In Main: Thread {i + 1}'s calculated value: {_tpCalculated[i]}");
                sum += _tpCalculated[i];
            }
            Console.WriteLine($"In Main: Total Summation from threads: {sum}");
            Console.WriteLine("\n");
            #endregion


            #region MATMUL
            // timed sequentially and in parallel, and we see a nice speedup going from 
            // sequential to parallel, from ~ 750ms down to ~200ms
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
            var threadIndexToUse = (int) stateInfo + 1;
            //Console.WriteLine($"Hello from thread with managed tid: {Thread.CurrentThread.ManagedThreadId} and user id: {threadIndexToUse}");
            Console.WriteLine($"Hello from tid: {threadIndexToUse}");

            // even
            if (threadIndexToUse % 2 == 0)
            {
                _tpCalculated[threadIndexToUse - 1] = CalculateSummationTo(threadIndexToUse);
            }
            // odd
            else
            {
                _tpCalculated[threadIndexToUse - 1] = CalculateFactorial(threadIndexToUse);
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
