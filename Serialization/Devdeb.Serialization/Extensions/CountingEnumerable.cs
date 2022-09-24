using System;
using System.Collections;
using System.Collections.Generic;

namespace Devdeb.Serialization.Extensions
{
    internal class CountingEnumerable<T> : IEnumerable<T>
    {
        private readonly IEnumerable<T> _instances;

        public CountingEnumerable(IEnumerable<T> instances)
        {
            _instances = instances ?? throw new ArgumentNullException(nameof(instances));
        }

        public int Count { get; private set; }

        public IEnumerator<T> GetEnumerator()
        {
            Count = 0;
            foreach (var instance in _instances)
            {
                yield return instance;
                Count++;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public static class SerializerExtensions
    {
        public static void Serializer<T>(T instance, Span<byte> buffer, ref int offset)
        { 
            
        }
    }
}
