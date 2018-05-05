using System;
using System.Diagnostics;
using System.Threading;

namespace Open.Disposable
{
	/// <summary>
	/// This class is used to facilitate the proper thread-safe dispose pattern even if a class is not DisposableBase.
	/// See DisposableBase for implementation.
	/// </summary>
	public sealed class DisposeHelper
	{
		enum State : int
		{
			Alive = 0,
			Disposing = 1,
			Disposed = 2
		}

		// Since all write operations are done through Interlocked, no need for volatile.
		private int _disposeState;

		/// <summary>
		/// Returns true if the container instance has not yet been disposed nor is in the process of disposing.
		/// </summary>
		public bool IsAlive
		{
			get { return _disposeState == (int)State.Alive; }
		}

		/// <summary>
		/// Will throw an ObjectDisposedException if the container instance is no longer alive (in the proccess of disposing or already disposed).
		/// </summary>
		/// <exception cref="ObjectDisposedException">{Object}</exception>
		public void AssertIsAlive()
		{
			if (!IsAlive)
				throw new ObjectDisposedException(this.ToString());
		}


		/// <summary>
		/// Returns true if the container instance is no longer alive (in the proccess of disposing or already disposed).
		/// </summary>
		public bool IsDisposed
		{
			get { return _disposeState == (short)State.Disposed; }
		}

		public event EventHandler BeforeDispose;

		// Dispose(bool calledExplicitly) executes in two distinct scenarios.
		// If calledExplicitly equals true, the method has been called directly
		// or indirectly by a user's code. Managed and unmanaged resources
		// can be disposed.
		// If calledExplicitly equals false, the method has been called by the
		// runtime from inside the finalizer and you should not reference
		// other objects. Only unmanaged resources can be disposed.
		public void Dispose(IDisposable target, Action<bool> OnDispose, bool calledExplicitly)
		{
			// Lock disposal...
			if (0 == Interlocked.CompareExchange(ref _disposeState, 1, 0)) // IsAlive? Yes? Mark as State.Disposing
			{
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

				// Then do internal cleanup.
				try
				{
					OnDispose?.Invoke(calledExplicitly);
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

				Interlocked.Exchange(ref _disposeState, 2); // State.Disposed
			}

			// This object will be cleaned up by the Dispose method.
			// Therefore, you should call GC.SupressFinalize to
			// take this object off the finalization queue
			// and prevent finalization code for this object
			// from executing a second time.
			if (calledExplicitly)
				GC.SuppressFinalize(target);

		}

	}
}
