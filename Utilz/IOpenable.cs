using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilz
{
    public enum LifecycleEvents { Disposing, NavigatedTo, NavigatingFrom, Resuming, Suspending }

    public interface IOpenable
    {
        Task<bool> OpenAsync(object args = null);
        Task<bool> CloseAsync(object args = null);
        bool IsOpen { get; }
    }
}
