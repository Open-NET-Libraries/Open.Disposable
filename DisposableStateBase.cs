/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIThttps://github.com/electricessence/Open.Disposable/blob/master/LISCENSE.md
 */
 
using System;
using System.Threading;

namespace Open.Disposable
{

    public abstract class DisposableStateBase : IDisposalState
    {
        protected const int ALIVE = 0;
        protected const int DISPOSING = 1;
        protected const int DISPOSED = 2;

        // Since all write operations are done through Interlocked, no need for volatile.
        private int _disposeState;
        public DisposeState DisposeState => (DisposeState)_disposeState;
        protected bool StartDispose()
            => _disposeState == ALIVE
            && Interlocked.CompareExchange(ref _disposeState, DISPOSING, ALIVE) == ALIVE;

        protected void Disposed()
            => Interlocked.Exchange(ref _disposeState, DISPOSED); // State.Disposed

        protected static TNullable Nullify<TNullable>(ref TNullable x)
            where TNullable : class
        {
            var y = x;
            x = null;
            return y;
        }

        protected static void DisposeOf<T>(ref T x)
            where T : class, IDisposable
        {
            var y = x;
            x = null;
            y?.Dispose();
        }

    }
}
