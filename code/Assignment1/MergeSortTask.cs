using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assignment1;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Tasks
{
    public class MergeSortTask: Task
    {
        private int [] m_array;
        private readonly int m_start;
        private readonly int m_end;
        private int m_counter;
        private readonly MergeSortTask m_parent;

        public MergeSortTask(int[] array): this(array, 0, array.Length - 1, null) {}

        public MergeSortTask(int[] array, int start, int end, MergeSortTask parent)
        {
            m_array = array;
            m_start = start;
            m_end = end;
            m_parent = parent;
            m_counter = 2;
        }

        public void Execute(Processor processor)
        {
            if (m_end - m_start > 128)
            {
                int middle = (m_start + m_end) / 2;
                MergeSortTask left = new MergeSortTask(m_array, m_start, middle, this);
                MergeSortTask right = new MergeSortTask(m_array, middle + 1, m_end, this);
                processor.EnqueueTask(left);
                processor.EnqueueTask(right);
            }
            else
            {
                MergeSort.Sort(m_array, m_start, m_end);
                SignalTaskDone(processor);
            }
        }

        private void SignalTaskDone(Processor processor)
        {
            if (m_parent != null)
                m_parent.ChildTaskDone(processor);
            else
                AssignmentMain.WorkCompleted();
        }

        private void ChildTaskDone(Processor processor)
        {
            int decrementedValue = Interlocked.Decrement(ref m_counter);
            if (decrementedValue == 0)
            {
                MergeSort.Merge(m_array, m_start, m_end);
                SignalTaskDone(processor);
            }
        }
    }
}
