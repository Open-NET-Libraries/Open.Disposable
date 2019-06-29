/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIThttps://github.com/electricessence/Open.Disposable/blob/master/LISCENSE.md
 */

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Open.Disposable
{
	/// <summary>
	/// This class is used to facilitate the proper thread-safe dispose pattern even if a class is not DisposableBase.
	/// See DisposableBase for implementation.
	/// </summary>
	public sealed class DisposeHelper
	{
		const int ALIVE = 0;
		const int DISPOSING = 1;
		const int DISPOSED = 2;

		// Since all write operations are done through Interlocked, no need for volatile.
		private int _disposeState;

		/// <summary>
		/// Returns true if the container instance has not yet been disposed nor is in the process of disposing.
		/// </summary>
		public bool IsAlive => _disposeState == ALIVE;

		/// <summary>
		/// Returns true if the container instance is no longer alive (in the proccess of disposing or already disposed).
		/// </summary>
		public bool IsDisposed => _disposeState != ALIVE;

		/// <summary>
		/// Will throw an ObjectDisposedException if the container instance is no longer alive (in the proccess of disposing or already disposed).
		/// </summary>
		/// <exception cref="ObjectDisposedException">{Object}</exception>
		public void AssertIsAlive()
		{
			if (!IsAlive)
				throw new ObjectDisposedException(ToString());
		}

		public event EventHandler BeforeDispose;

		// Dispose(bool calledExplicitly) executes in two distinct scenarios.
		// If calledExplicitly equals true, the method has been called directly
		// or indirectly by a user's code. Managed and unmanaged resources
		// can be disposed.
		// If calledExplicitly equals false, the method has been called by the
		// runtime from inside the finalizer and you should not reference
		// other objects. Only unmanaged resources can be disposed.
		public ValueTask Dispose(
			IDisposable target,
			Func<bool, ValueTask> OnDisposeAsync,
			Action<bool> OnDispose,
			bool calledExplicitly)
		{
			ValueTask? result = null;
			// Lock disposal...
			if (ALIVE == _disposeState && ALIVE == Interlocked.CompareExchange(ref _disposeState, DISPOSING, ALIVE)) // IsAlive? Yes? Mark as State.Disposing
			{
				lock(this) {
					// Ensure we've gained ownership at this point.
					// By locking we've allowed any actions to complete,
					// but nothing else can enter since the dispose state is disposing (not alive).
				}
				// For calledExplicitly, throw on errors.
				// If by the GC (aka finalizer) don't throw,
				// since it's ignored anyway and creates overhead.

				// Fire events first because some internals may need access.
				try
				{
					if (BeforeDispose != null)
					{
						BeforeDispose(this, EventArgs.Empty);
						BeforeDispose = null;
					}

					// Events should only fire if there are listeners...
					(target as DisposableBase)?.FireBeforeDispose();

				}
				catch (Exception eventBeforeDisposeException)
				{
					if (!calledExplicitly)
					{
						if (Debugger.IsAttached)
							Debug.Fail(eventBeforeDisposeException.ToString());
					}
					else
						throw;
				}

				if (OnDisposeAsync != null)
				{
					try
					{
						result = OnDisposeAsync.Invoke(calledExplicitly);
					}
					catch (Exception onDisposeException)
					{
						if (!calledExplicitly)
						{
							if (Debugger.IsAttached)
								Debug.Fail(onDisposeException.ToString());
						}
						else
							throw;
					}
				}

				if (OnDispose != null)
				{
					// Then do internal cleanup.
					try
					{
						OnDispose.Invoke(calledExplicitly);
					}
					catch (Exception onDisposeException)
					{
						if (!calledExplicitly)
						{
							if (Debugger.IsAttached)
								Debug.Fail(onDisposeException.ToString());
						}
						else
							throw;
					}
				}

				Interlocked.Exchange(ref _disposeState, DISPOSED); // State.Disposed
			}

			// This object will be cleaned up by the Dispose method.
			// Therefore, you should call GC.SupressFinalize to
			// take this object off the finalization queue
			// and prevent finalization code for this object
			// from executing a second time.
			if (calledExplicitly)
				// ReSharper disable once GCSuppressFinalizeForTypeWithoutDestructor
				GC.SuppressFinalize(target);

			return result ?? new ValueTask();

		}
	}
}
