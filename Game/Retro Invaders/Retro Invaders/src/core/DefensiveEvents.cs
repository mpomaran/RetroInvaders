using System;

namespace MPP.core
{
    /* http://www.c-sharpcorner.com/uploadfile/rmcochran/defensiveevents04022006105141am/defensiveevents.aspx
 */

    public interface IEventListener
    {
        void OnNotification(IEventPublisher publisher, EventArgs eventArgs);
    }

    public interface IEventPublisher
    {
        void RegisterListener<T>(T listener) where T : IEventListener;
        void UnregisterListener<T>(T listener) where T : IEventListener;
        void NotifyListeners(EventArgs args);
    }

}
