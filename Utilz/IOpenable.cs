using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Utilz
{
    /// <summary>
    /// Many objects need asynchronous operations on initialising and terminating.
    /// However, constructors, Dispose and destructors are synchronous.
    /// Implement this interface to run those operations asynchsonously.
    /// </summary>
    public interface IOpenable
    {
        Task<bool> OpenAsync(object args = null);
        Task<bool> CloseAsync(object args = null);
        bool IsOpen { get; }
    }

    /// <summary>
    /// Useful to tipify parameters for <see cref="IOpenable.OpenAsync"/> and <see cref="IOpenable.CloseAsync"/>.
    /// </summary>
    public enum LifecycleEvents { Disposing, NavigatedToAfterFileActivated, NavigatedToAfterLaunch, NavigatingFrom, Resuming, Suspending }

    /// <summary>
    /// An app can implement this interface, and then send out events when its state changes.
    /// Why not use the standard events <see cref="Application.Resuming"/> and <see cref="Application.Suspending"/>?
    /// Because they are defined in <see cref="Application"/>, so your subclass will have no access to the event subscribers.
    /// To achieve finer control on the event subscribers, implement this interface and use <see cref="SuspenderResumerExtensions"/>.
    /// </summary>
    public interface ISuspenderResumer
    {
        event SuspendingEventHandler SuspendStarted;
        event EventHandler ResumeStarted;
    }

    /// <summary>
    /// Helps deal with <see cref="ISuspenderResumer"/> events with fine-grained control.
    /// </summary>
    public static class SuspenderResumerExtensions
    {
        public const int MSecToWaitToConfirm = 25;
        /// <summary>
        /// Waits for <see cref="ISuspenderResumer"/> event subscribers to be open (if mustBeOpen is true) or closed (otherwise) before proceeding,
        /// if they implement <see cref="IOpenable"/>.
        /// Useful if you have to wait for open/close operations to terminate.
        /// </summary>
        /// <param name="suspenderResumer"></param>
        /// <param name="eventSubscribers"></param>
        /// <param name="mustBeOpen"></param>
        /// <returns></returns>
        public static async Task WaitForIOpenableSubscribers(this ISuspenderResumer suspenderResumer, Delegate[] eventSubscribers, bool mustBeOpen)
        {
            if (suspenderResumer == null || eventSubscribers == null) return;

            bool exitLoop = false;
            while (!exitLoop)
            {
                exitLoop = true;
                foreach (var subscriber in eventSubscribers)
                {
                    var iOpenable = subscriber.Target as IOpenable;
                    if (iOpenable != null && iOpenable.IsOpen != mustBeOpen)
                    {
                        exitLoop = false;

                        if (mustBeOpen) Debug.WriteLine("Waiting for an ISuspenderResumer to be open");
                        else Debug.WriteLine("Waiting for an ISuspenderResumer to be closed");

                        await Task.Delay(MSecToWaitToConfirm).ConfigureAwait(false);
                        break;
                    }
                }
            }
        }
    }
}
