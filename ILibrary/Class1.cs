using System;
using System.Collections.Generic;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;

namespace ILibrary
{
    namespace GameTools
    {
        public class IDController
        {
            private List<int> _idList;
            private int _capacity;
            public int CapacityFactor;

            public int Capacity { get => _capacity; set => _capacity = value; }
            public int Count => _idList.Count;

            public IDController() : this(1) { }
            public IDController(int capacity) { _idList = new List<int>(Capacity = capacity); CreateIdList(); }

            private void CreateIdList()
            {
                CapacityFactor = _capacity;
                for (int i = 0; i < _capacity; i++)
                {
                    _idList.Add(i);
                }
            }

            private void ExpandCapacity(int count)
            {
                for (int i = 0; i < count; i++)
                {
                    _idList.Add(_capacity + i);
                }
                _capacity += count;
            }

            public int GetID()
            {
                if (_idList.Count == 1) ExpandCapacity(CapacityFactor);
                int id = _idList[0];
                _idList.RemoveAt(0);
                return id;
            }

            public void ReturnID(int value) => _idList.Add(value);
        }
    }
    namespace List
    {
        public class AList<T> : IEnumerable<T>
        {
            private int _count;
            public int Count => _count;
            public int Capacity => _array.Length;
            private T[] _array;

            public AList() : this(0) { }

            public AList(int capacity) => _array = new T[capacity];

            public void Add(T addItem)
            {
                T[] newArray = new T[++_count];
                for (int i = 0; i < _array.Length; i++)
                {
                    newArray[i] = _array[i];
                }
                newArray[_array.Length] = addItem;
                _array = newArray;
            }

            public void Sort()
            {
                bool c;
                do
                {
                    c = false;
                    for (int i = 0; i < _count - 1; i++)
                    {
                        if (((IComparable<T>)_array[i]).CompareTo(_array[i + 1]) > 0)
                        {
                            T a = _array[i];
                            _array[i] = _array[i + 1];
                            _array[i + 1] = a;
                            c = true;
                        }
                    }
                }
                while (c);
            }

            public void Reverce()
            {
                for (int i = 0; i < (_array.Length / 2); i++)
                {
                    T a = _array[i];
                    _array[i] = _array[(_array.Length) - 1 - i];
                    _array[(_array.Length) - 1 - i] = a;
                }
            }

            public void Clear()
            {
                _count = 0;
                _array = new T[_count];
            }

            public bool Contains(T item)
            {
                for (int i = 0; i < _array.Length; i++)
                    if (_array[i].Equals(item))
                        return true;
                return false;
            }

            public void AddCopyTo(ref T[] items)
            {
                T[] newItemsArray = new T[items.Length + _array.Length];
                for (int i = 0; i < items.Length; i++)
                {
                    newItemsArray[i] = items[i];
                }
                for (int i = 0; i < _array.Length; i++)
                {
                    newItemsArray[i + items.Length] = _array[i];
                }
                items = newItemsArray;
            }

            public void CopyTo(T[] items)
            {
                items = new T[_array.Length];

                for (int i = 0; i < _array.Length; i++)
                {
                    items[i] = _array[i];
                }
            }

            public void Insert(int index, T item)
            {
                T[] newArray = new T[++_count];
                for (int i = 0; i < index; i++)
                    newArray[i] = _array[i];
                newArray[index] = item;
                for (int i = index + 1; i < _array.Length + 1; i++)
                    newArray[i] = _array[i - 1];
                _array = newArray;
            }

            public void Remove(T item)
            {
                int i = 0;
                for (; i < _array.Length; i++)
                {
                    if (_array[i].Equals(item))
                        break;
                }
                for (; i < _array.Length - 1; i++)
                {
                    _array[i] = _array[i + 1];
                }
            }

            public IEnumerator<T> GetEnumerator()
            {
                for (int i = 0; i < _count; i++)
                {
                    yield return _array[i];
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public override string ToString()
            {
                string s = "";
                if (_count == 0)
                { return "Список пуст"; }
                for (int i = 0; i < _count; i++)
                {
                    s += _array[i] + " ";
                }
                return s;
            }

        }
    }
    namespace IMath
    {
        public static class IMath
        {
            /// <summary>
            /// Returns a list of Prime numbers
            /// </summary>
            /// <param name="maxValue">To what values to return the number</param>
            /// <returns></returns>
            public static List<int> GetSimpleNumber(int maxValue)
            {
                List<int> listSimpleNumber = new List<int>();
                int[] a = new int[maxValue];
                for (int j = 0; j < maxValue; j++)
                {
                    a[j] = j;
                }
                a[1] = 0;
                int i = 2;
                while (i < maxValue)
                {
                    if (a[i] != 0)
                        listSimpleNumber.Add(a[i]);
                    for (int j = i; j <= a.Length + 1;)
                    {
                        if ((j += i) >= a.Length) break;
                        a[j] = 0;
                    }
                    i++;
                }
                return listSimpleNumber;
            }
            public static List<uint> GetSimpleNumber(uint maxValue)
            {
                List<uint> listSimpleNumber = new List<uint>();
                uint[] a = new uint[maxValue];
                for (uint j = 0; j < maxValue; j++)
                {
                    a[j] = j;
                }
                a[1] = 0;
                uint i = 2;
                while (i < maxValue)
                {
                    if (a[i] != 0)
                        listSimpleNumber.Add(a[i]);
                    for (uint j = i; j <= a.Length + 1;)
                    {
                        if ((j += i) >= a.Length) break;
                        a[j] = 0;
                    }
                    i++;
                }
                return listSimpleNumber;
            }
            public static List<int> GetSimpleNumber(int minValue, int maxValue)
            {
                List<int> listSimpleNumber = new List<int>();
                int[] a = new int[maxValue];
                for (int j = 0; j < maxValue; j++)
                {
                    a[j] = j;
                }
                a[1] = 0;
                int i = 2;
                while (i < maxValue)
                {
                    if (a[i] != 0)
                        listSimpleNumber.Add(a[i]);
                    for (int j = i; j <= a.Length + 1;)
                    {
                        if ((j += i) >= a.Length) break;
                        a[j] = 0;
                    }
                    i++;
                }
                listSimpleNumber.RemoveRange(0, listSimpleNumber.IndexOf(listSimpleNumber.Find(x => x >= minValue)));
                return listSimpleNumber;
            }
            public static int GetRandomSimpleNumber(int minValue, int maxValue)
            {
                List<int> listSimpleNumber = new List<int>();
                int[] a = new int[maxValue];
                for (int j = 0; j < maxValue; j++)
                {
                    a[j] = j;
                }
                a[1] = 0;
                int i = 2;
                while (i < maxValue)
                {
                    if (a[i] != 0)
                        listSimpleNumber.Add(a[i]);
                    for (int j = i; j <= a.Length + 1;)
                    {
                        if ((j += i) >= a.Length) break;
                        a[j] = 0;
                    }
                    i++;
                }
                listSimpleNumber.RemoveRange(0, listSimpleNumber.IndexOf(listSimpleNumber.Find(x => x >= minValue)));
                Random random = new Random();
                return listSimpleNumber[random.Next(0, listSimpleNumber.Count)];
            }
        }
    }
    namespace Images
    {
        class ImageChecker
        {
            private readonly string _path1 = @"C:\Users\Alexey\Desktop\1.jpg";
            private readonly string _path2 = @"C:\Users\Alexey\Desktop\2.jpg";
            private readonly string _saveFiles = @"C:\Users\Alexey\Desktop\saves.jpg";
            public ImageChecker() => Console.Write(SelectionCompareImages(1, 50));
            private bool CompareImages()
            {
                Bitmap bitmap1 = new Bitmap(_path1);
                Bitmap bitmap2 = new Bitmap(_path2);
                Size size1 = bitmap1.Size;
                Size size2 = bitmap2.Size;

                if (size1 == size2)
                {
                    for (int i = 0; i < size1.Width; i += 2)
                        for (int j = 0; j < size1.Height; j += 2)
                        {
                            if (bitmap1.GetPixel(i, j) == bitmap2.GetPixel(i, j))
                                continue;
                            return false;
                        }
                }
                else { return false; }
                return true;
            }
            private bool SelectionCompareImages(int minInaccuracy, int maxInaccuracy)
            {
                Bitmap bitmap1 = new Bitmap(_path1);
                Bitmap bitmap2 = new Bitmap(_path2);
                Size size1 = bitmap1.Size;
                Size size2 = bitmap2.Size;
                int inaccuracy = minInaccuracy;
                do
                {
                    for (int i = 0; i < size1.Width; i++)
                        for (int j = 0; j < size1.Height; j++)
                        {
                            if (i + size2.Width > size1.Width || j + size2.Height > size1.Height)
                                continue;
                            if (!ColorCompare(bitmap1.GetPixel(i, j), bitmap2.GetPixel(0, 0), inaccuracy))
                                continue;
                            if (!ColorCompare(bitmap1.GetPixel(i, j + size2.Height - 1), bitmap2.GetPixel(0, size2.Height - 1), inaccuracy))
                                continue;
                            if (!ColorCompare(bitmap1.GetPixel(i + size2.Width - 1, j + size2.Height - 1), bitmap2.GetPixel(size2.Width - 1, size2.Height - 1), inaccuracy))
                                continue;
                            if (!ColorCompare(bitmap1.GetPixel(i + size2.Width - 1, j), bitmap2.GetPixel(size2.Width - 1, 0), inaccuracy))
                                continue;
                            if (!ColorCompare(bitmap1.GetPixel(i + (size2.Width - 1) / 2, j + (size2.Height - 1) / 2), bitmap2.GetPixel((size2.Width - 1) / 2, (size2.Height - 1) / 2), inaccuracy))
                                continue;

                            Bitmap bitmap3 = new Bitmap(_path1);
                            for (int k = 0; k < size2.Width; k += 1)
                                for (int l = 0; l < size2.Height; l += 1)
                                {
                                    if (!ColorCompare(bitmap1.GetPixel(i + k, j + l), bitmap2.GetPixel(k, l), 50))
                                        goto exit;
                                    if (k == 0 || k == size2.Width - 1 || l == 0 || l == size2.Height - 1)
                                        bitmap3.SetPixel(i + k, j + l, Color.Red);
                                    continue;
                                }
                            bitmap3.Save(_saveFiles, ImageFormat.Jpeg);
                            return true;
                            exit: continue;
                        }
                    inaccuracy++;
                } while (inaccuracy < maxInaccuracy);
                return false;
            }
            private bool ColorCompare(Color First, Color Second, int delta) => Math.Abs(First.R - Second.R) <= delta && Math.Abs(First.G - Second.G) <= delta && Math.Abs(First.B - Second.B) <= delta;
        }
    }
}
