using System.Collections.Generic;

namespace Nop.Plugin.Misc.ErpWebhook.Extensions;

public static class IEnumerableExtensions
{
    public static IEnumerable<IEnumerable<T>> Batched<T>(this IEnumerable<T> records, int batchSize)
    {
        var buffer = new List<T>(batchSize);
        var cursor = records.GetEnumerator();
        while (true)
        {
            while (buffer.Count < batchSize && cursor.MoveNext())
            {
                buffer.Add(cursor.Current);
            }
            if (buffer.Count < 1)
            {
                yield break;
            }

            yield return buffer;

            if (buffer.Count < batchSize)
            {
                yield break;
            }
            buffer.Clear();
        }
    }
}
