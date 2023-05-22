using System;
using System.Collections;
using System.Collections.Generic;

namespace Proto.Util
{
    [Serializable]
    public class WrapperList<T> : IEnumerable<T>
    {
        public WrapperList() { list = new List<T>(); }
        public WrapperList(List<T> list) { this.list = list; }
        public List<T> list;
        public void Add(T item) { list.Add(item); }
        public T this[int i] => list[i];
        public int Count => list.Count;
        public void Clear() { list.Clear(); }
        public IEnumerator<T> GetEnumerator() { return list.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return list.GetEnumerator(); }
    }
}