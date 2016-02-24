using System;
using TailBlazer.Domain.Settings;

namespace TailBlazer.Views.Tail
{
    [Obsolete("Need to have a think about this")]
    public class TailViewPersister: IPersistentStateProvider
    {
        private readonly TailViewModel _tailView;
        

        public TailViewPersister(TailViewModel tailView)
        {
            _tailView = tailView;
        }


        State IPersistentStateProvider.CaptureState()
        {
            return new State(1,"");
        }

        private TailViewState RestoreState()
        {
            return null;
        }

        public void Restore(State state)
        {
            
        }
    }
}
