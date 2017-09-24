/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Open/blob/dotnet-core/LICENSE.md
 */

using System;
using System.Threading;

namespace Open.Disposable
{
    /// <summary>
    /// This class allows for supplying a delegate to be executed when the .Dispose() method is called.
    /// 
    /// Typically overriding DisposableBase would be the normal procedure, but with this class you could pass a finalizer function into the constructor from a sub class instead.
    /// </summary>
    public class DisposeHandler : DisposableBase
    {
        Action _action;
        public DisposeHandler(Action action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            _action = action;
        }

        protected override void OnDispose(bool calledExplicitly)
        {
            Interlocked.Exchange(ref _action, null)?.Invoke();
        }
    }
}
