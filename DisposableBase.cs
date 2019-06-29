/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIThttps://github.com/electricessence/Open.Disposable/blob/master/LISCENSE.md
 */

using System;

namespace Open.Disposable
{

    /// <summary>
    /// A base class for implementing other disposables.  Properly implements the (thread-safe) dispose pattern using DisposeHelper.
    /// 
    /// Provides useful properties and methods to allow for checking if this instance has already been disposed and provides a "BeforeDispose" event for other classes to react to.
    /// </summary>
    public abstract class DisposableBase : DisposableStateBase, IDisposable
	{
        /// <inheritdoc />
		public void Dispose()
		{
			Dispose(true);
		}

		// Being called by the GC...
		~DisposableBase()
		{
			Dispose(false);
		}

		protected abstract void OnDispose(bool calledExplicitly);

        private void Dispose(bool calledExplicitly)
        {
            if (!StartDispose())
                return;
			
			GC.SuppressFinalize(this);

            try
            {
				OnDispose(calledExplicitly);
            }
            finally
            {
                Disposed();
            }
        }
	}

}
