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
}
