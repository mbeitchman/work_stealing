using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Tasks;
using System.Diagnostics;

namespace Assignment1
{
    // This class encapsulates all oof the processors queues
    // so each processor can be initialized with a pointer
    // to all of the queues so they can access them in order
    // to steal tasks.
    public class Queues
    {
        public WorkStealingQueue<Task>[] localQueues;

        public Queues()
        {
            localQueues = new WorkStealingQueue<Task>[AssignmentMain.TotalNumberOfProcessors()];
            for (int i = 0; i < AssignmentMain.TotalNumberOfProcessors(); i++)
            {
                localQueues[i] = new WorkStealingQueue<Task>();
            }
        }
    }

    public class Processor
    {
        public int id_;
        public WorkStealingQueue<Task> localQueue; // local queue
        public int iteration;
        public Queues allProcQueues; // pointer to all queues
        private int totalProcs;
        private Random rand;

        public Processor(int id, Queues queue)
        {
            id_ = id;
            iteration = 0;
            localQueue = queue.localQueues[id_];
            allProcQueues = queue;
            totalProcs = AssignmentMain.TotalNumberOfProcessors();
            rand = new Random();
        }

        public WorkStealingQueue<Task> GetQueue(int procid)
        {
            return this.localQueue;
        }

        public void EnqueueTask(Task task)
        {
            localQueue.LocalPush(task);
        }

        // main thread function
        // initially processor 0 has 1 task
        public void Run()
        {
            int procToStealFrom = 0;
            int MaxTasks = 0;
            Task localTask = null;

            while (!AssignmentMain.IsDone())
            {
                if (localQueue.Count > 0)
                {
                    bool Success = localQueue.LocalPop(ref localTask);
                    if (Success)
                    {
                        localTask.Execute(this);
                        //iteration++;
                    }
                }
                else
                {
                   // Find the processor with the most tasks and try to steal from them
                   for (int i = 0; i < totalProcs; i++)
                     {
                         if (i != id_)
                         {
                             if (allProcQueues.localQueues[i].Count > MaxTasks)
                             {
                                 MaxTasks = allProcQueues.localQueues[i].Count;
                                 procToStealFrom = i;
                             }
                         }
                     }
                    
                    
                     // Try to steal from a random processor.
                     /*for (int i = 0; i < totalProcs; i++)
                       {
                            procToStealFrom = rand.Next(totalProcs);

                            if (totalProcs != id_)
                            {
                                break;
                            }
                        }*/

                    // Execute the task immediately after a sucessful steal
                    bool Success = allProcQueues.localQueues[procToStealFrom].TrySteal(ref localTask, 1);
                    if (Success)
                    {
                        localTask.Execute(this);
                    }
                }
            }
        }
    }
}
