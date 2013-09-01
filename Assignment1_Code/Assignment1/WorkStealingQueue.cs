using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

/*
 * This code is taken from: http://www.bluebytesoftware.com/blog/2008/08/12/BuildingACustomThreadPoolSeriesPart2AWorkStealingQueue.aspx
 * Only field names are modified for external consistency.
 * 
 * This datastructure is thread safe. Clients can concurrently call 
 * the member methods without holding locks or other synchronization
 * 
 */
namespace Assignment1
{
    public class WorkStealingQueue<T>
    {
        private const int INITIAL_SIZE = 32;
        private T[] m_array = new T[INITIAL_SIZE];
        private int m_mask = INITIAL_SIZE - 1;
        private volatile int m_headIndex = 0;
        private volatile int m_tailIndex = 0;
        private object m_foreignLock = new object();

        public bool IsEmpty
        {
            get { return m_headIndex >= m_tailIndex; }
        }

        public int Count
        {
            get { return m_tailIndex - m_headIndex; }
        }

        public void LocalPush(T obj)
        {
            int tail = m_tailIndex;
            if (tail < m_headIndex + m_mask)
            {
                m_array[tail & m_mask] = obj;
                m_tailIndex = tail + 1;
            }
            else
            {
                lock (m_foreignLock)
                {
                    int head = m_headIndex;
                    int count = m_tailIndex - m_headIndex;
                    if (count >= m_mask)
                    {
                        T[] newArray = new T[m_array.Length << 1];
                        for (int i = 0; i < m_array.Length; i++)
                            newArray[i] = m_array[(i + head) & m_mask];
                        // Reset the field values, incl. the mask.
                        m_array = newArray;
                        m_headIndex = 0;
                        m_tailIndex = tail = count;
                        m_mask = (m_mask << 1) | 1;
                    }
                    m_array[tail & m_mask] = obj;
                    m_tailIndex = tail + 1;
                }
            }
        }

        public bool LocalPop(ref T obj)
        {
            int tail = m_tailIndex;
            if (m_headIndex >= tail)
                return false;
            #pragma warning disable 0420
            tail -= 1;
            Interlocked.Exchange(ref m_tailIndex, tail);
            if (m_headIndex <= tail)
            {
                obj = m_array[tail & m_mask];
                return true;
            }
            else
            {
                lock (m_foreignLock)
                {
                    if (m_headIndex <= tail)
                    {
                        // Element still available. Take it.
                        obj = m_array[tail & m_mask];
                        return true;
                    }
                    else
                    {
                        // We lost the race, element was stolen, restore the tail.
                        m_tailIndex = tail + 1;
                        return false;
                    }
                }
            }
        }

        public bool TrySteal(ref T obj, int millisecondsTimeout)
        {
            bool taken = false;
            try
            {
                taken = Monitor.TryEnter(m_foreignLock, millisecondsTimeout);
                if (taken)
                {
                    int head = m_headIndex;
                    Interlocked.Exchange(ref m_headIndex, head + 1);
                    if (head < m_tailIndex)
                    {
                        obj = m_array[head & m_mask];
                        return true;
                    }
                    else
                    {
                        m_headIndex = head;
                        return false;
                    }
                }
            }
            finally
            {
                if (taken)
                    Monitor.Exit(m_foreignLock);
            }
            return false;
        }
    }
}
