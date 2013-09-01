using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using Tasks;

namespace Assignment1
{
    public class AssignmentMain
    {
        private static int m_noProcs = 0;
        private static volatile bool m_done = false;
        private static Processor[] m_processors = null;
        public static bool IsDone()
        {
            return m_done;
        }

        public static void WorkCompleted()
        {
            m_done = true;
        }

        public static int TotalNumberOfProcessors()
        {
            return m_noProcs;
        }

        public static Processor GetProcessor(int id)
        {
            return m_processors[id];
        }

        public static void Main(string[] args)
        {
            /*
             * CSEP 506 Assignment 1
             * This assignment sets the number of processors that will be
             * used during the simulation. 
             * Feel free to change the number to experiment the performance
             * with different setups.
             */
            m_noProcs = 8;
         
            Random random = new Random();
            TimeSpan totalSequentialTime = new TimeSpan();
            TimeSpan totalParallelTime = new TimeSpan();
            int iterations = 20;
            int million = 1000000;
            int min = 1 * million;
            int max = 1 * million;

            for (int a = 0; a < iterations; a++)
            {
                Console.WriteLine("Iteration " + (a + 1) + " is started.");
                // Generate a random sorting task.
                int[] array = new int[million];
                int [] arrayDuplicate =  new int[array.Length];
                for (int b = 0; b < array.Length; b++)
                {
                    int value = random.Next(max);
                    array[b] = value;
                    arrayDuplicate[b] = value;
                }
                TimeSpan sequentialTime = RunSequential(array);
                Console.WriteLine("Sequential sort lasted = " + sequentialTime);
                totalSequentialTime += sequentialTime;
                
                TimeSpan parallelTime = RunParallel(arrayDuplicate);
                totalParallelTime += parallelTime;
                Console.WriteLine("Parallel sort lasted = " + parallelTime);
                
                Console.WriteLine("Iteration " + (a + 1) + " is ended.\n");
            }

            Console.WriteLine("Sequential total time = " + totalSequentialTime);
            Console.WriteLine("Parallel total time = " + totalParallelTime);
            Console.WriteLine("Speed up = " + totalSequentialTime.TotalMilliseconds / totalParallelTime.TotalMilliseconds);

            Console.ReadLine();
        }

        private static TimeSpan RunParallel(int[] array)
        {
            DateTime start = DateTime.Now;
            m_done = false;
            
            // create the Queues for all processors
            Queues globalQueues = new Queues();

            // create and allocate N processor instances
            m_processors = new Processor[m_noProcs];
            for (int a = 0; a < m_noProcs; a++)
                m_processors[a] = new Processor(a, globalQueues);

            // kick off first task on first processor 
            m_processors[0].EnqueueTask(new MergeSortTask(array));
            
            // create threads which run the tasks
            Thread[] threads = new Thread[m_noProcs];
           
            for (int a = 0; a < m_noProcs; a++)
            {
                Processor processor = m_processors[a];
                Thread thread = new Thread(new ThreadStart(processor.Run));
                threads[a] = thread;
                thread.Start();
            }

            // wait for threads to finish
            foreach (Thread thread in threads)
                thread.Join();

            DateTime end = DateTime.Now;
            return end - start;
        }

        private static TimeSpan RunSequential(int [] array)
        {
            DateTime start = DateTime.Now;
            MergeSort.Sort(array);

            DateTime end = DateTime.Now;
            return end - start;
        }
    }
}
