using Mono.Collections.Generic;


namespace Shaspect.Builder.Tools
{
    internal static class CollectionExtensions
    {
        public static int LastIndexOf<T> (this Collection<T> coll, T item)
        {
            for (int i = coll.Count - 1; i >= 0; --i)
            {
                if (coll[i].Equals(item))
                    return i;
            }
            return -1;
        }

    }
}