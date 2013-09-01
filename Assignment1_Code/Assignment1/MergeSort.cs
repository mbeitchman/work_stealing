using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assignment1
{
    /*
    * The code is taken from: http://www.java2s.com/Code/CSharp/Collections-Data-Structure/Implementstherecursivemergesortalgorithmtosortanarray.htm
    */
    public class MergeSort
    {
        public static void Sort(int[] data)
        {
            Sort(data, 0, data.Length - 1);
        }

        public static void Sort(int[] data, int left, int right)
        {
            if (left < right)
            {
                int middle = (left + right) / 2;
                Sort(data, left, middle);
                Sort(data, middle + 1, right);
                Merge(data, left, right);
            }
        }

        public static void Merge(int[] data, int left, int right)
        {
            int oldPosition = left;
            int size = right - left + 1;
            int[] temp = new int[size];
            int i = 0;
            int middle = (left + right) / 2;
            int middle1 = middle + 1;

            while (left <= middle && middle1 <= right)
            {
                if (data[left] <= data[middle1])
                    temp[i++] = data[left++];
                else
                    temp[i++] = data[middle1++];
            }
            if (left > middle)
                for (int j = middle1; j <= right; j++)
                    temp[i++] = data[middle1++];
            else
                for (int j = left; j <= middle; j++)
                    temp[i++] = data[left++];
            Array.Copy(temp, 0, data, oldPosition, size);
        }
    }
}