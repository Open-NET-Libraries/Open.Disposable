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
        private int _disposeCalled = ALIVE;
        private int _disposeState = ALIVE;
        public DisposeState DisposeState => (DisposeState)_disposeState;

        /* This is important because some classes might react to disposal
         * and still need access to the live class before it's disposed. */
        #region Before Disposal
		protected virtual void OnBeforeDispose() { }

		/// <summary>
		/// This event is triggered by the DisposeHelper immediately before 
		/// </summary>
		public event EventHandler BeforeDispose;
		internal void FireBeforeDispose()
		{
			// Events should only fire if there are listeners...
			if (BeforeDispose != null)
			{
				BeforeDispose(this, EventArgs.Empty);
				BeforeDispose = null;
			}
		}
        #endregion


        protected bool StartDispose()
        {
            if(_disposeCalled == ALIVE
            && Interlocked.CompareExchange(ref _disposeCalled, DISPOSING, ALIVE) == ALIVE)
            {
                try {

                }
                finally {
                    if(_disposeState == ALIVE)
                        Interlocked.CompareExchange(ref _disposeState, DISPOSING, ALIVE);
                }
                return true;
            }

            return false;
        }

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
