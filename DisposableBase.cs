/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIThttps://github.com/electricessence/Open.Disposable/blob/master/LISCENSE.md
 */

using System;
using System.Diagnostics;

namespace Open.Disposable
{
	/// <summary>
	/// A base class for properly implementing the synchronous dispose pattern.
	/// Implement OnDispose to handle disposal.
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

		/// <summary>
		/// When implemented will be called (only once) when being disposed.
		/// </summary>
		/// <param name="calledExplicitly">True if called through code in the runtime, or false if finalized by the garbage collector.</param>
		protected abstract void OnDispose(bool calledExplicitly);

		internal protected bool StartDispose(bool calledExplicitly)
		{
			try
			{
				if (StartDispose())
					return true;

				// Already called?  No need to GC suppress.
				calledExplicitly = false;
				return false;
			}
			catch (Exception eventBeforeDisposeException) when (!calledExplicitly)
			{
				if (Debugger.IsAttached)
					Debug.Fail(eventBeforeDisposeException.ToString());

				return true; // If we even got this far with !calledExplicitly then it must be the first time and we should proceed.
			}
			finally
			{
				if (calledExplicitly)
					GC.SuppressFinalize(this);
			}
		}

		private void Dispose(bool calledExplicitly)
        {
			if (!StartDispose(calledExplicitly))
				return;
			
            try
            {
				OnDispose(calledExplicitly);
            }
            catch (Exception onDisposeException) when (!calledExplicitly)
            {
                if (Debugger.IsAttached)
                    Debug.Fail(onDisposeException.ToString());
            }             
            finally
            {
                Disposed();
            }
        }
	}

}
