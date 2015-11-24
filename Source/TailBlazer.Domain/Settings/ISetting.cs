using System;

namespace TailBlazer.Domain.Settings
{
    /*
        The consuming code talks to implementations of this
   */
    public interface ISetting<T>
    {
        //Need to work out the interface for this but must ne observable

        IObservable<T> Watch();
         
        //write item and internally goes to the store.
        //also the setting must know how to interpret from state to T and back.

        void Write(T item);
    }
}