namespace TailBlazer.Domain.FileHandling
{
    public enum ScrollReason
    {
        /// <summary>
        /// Auto scroll to the tail.
        /// </summary>
        Tail,
        /// <summary>
        /// The consumer specifies whic starting index and page size
        /// </summary>
        User
    }
}