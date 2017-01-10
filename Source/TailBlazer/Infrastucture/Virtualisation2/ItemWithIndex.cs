namespace TailBlazer.Infrastucture
{
    public class ItemWithIndex<T>
    {
        public T Item { get; }
        public int Index { get; }

        public ItemWithIndex(T item, int index)
        {
            Item = item;
            Index = index;
        }
    }
}