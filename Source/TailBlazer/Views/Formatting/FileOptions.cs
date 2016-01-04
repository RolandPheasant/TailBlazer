using System;
using TailBlazer.Domain.FileHandling.Search;

namespace TailBlazer.Views.Formatting
{
    /*
        1. This is for options for individual files
        2. Highlight, filter, colours et
        3. Add ability to apply to all files.
    */



    [Flags]
    public enum SearchOption
    {
        //Filter = 
    }
    public class SearchOptions
    {
     //   = 
    }

    public class FileOption
    {

    }



    public class FileConverter
    {

    }

    public interface IFileOptionCollection
    {
    }

    public class FileOptionCollection : IFileOptionCollection
    {
        public FileOptionCollection(ISearchInfoCollection searchInfoCollection)
        {
            //file options = SearchInfoCollection + HighlightCollection
       

        }
    }
}
