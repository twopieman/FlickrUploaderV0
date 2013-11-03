using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlickrUploaderV0
{
    public static class MyExtensions
    {
        public static void Batch<T>(this IEnumerable<T> items, int batchSize, Action<IEnumerable<T>> actionOnBatch)
        {
            Contract.Assert(batchSize > 1, "Batchsize cannot be less than 1");

            List<T> buffer = new List<T>();

            using (var enumerator = (items ?? Enumerable.Empty<T>()).GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    buffer.Add(enumerator.Current);

                    if (buffer.Count == batchSize)
                    {
                        actionOnBatch(buffer);
                        buffer.Clear();
                        Console.WriteLine("Done with a batch");
                    }
                }

                if (buffer.Count() > 0)
                {
                    actionOnBatch(buffer);
                }
            }
        }
    }
}
