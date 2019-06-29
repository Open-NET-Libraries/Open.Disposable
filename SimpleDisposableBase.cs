/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Open.Disposable/blob/dotnet-core/LICENSE.md
 */

using Open.Disposable.Async;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Open.Disposable
{
    public abstract class SimpleDisposableBase : IDisposable, IDisposalState
    {
        private bool _wasDisposed = false;
        public bool WasDisposed => _wasDisposed;

		#region IDisposable Members
		/// <summary>
		/// Standard IDisposable 'Dispose' method.
		/// Triggers cleanup of this class and suppresses garbage collector usage.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
		}
		#endregion

        protected abstract void OnDispose(bool calledExplicitly);

		protected void Dispose(bool calledExplicitly)
		{
			try
			{
				OnBeforeDispose();
			}
			finally
			{
				if(DisposingHelper!=null)
				{
					Interlocked.Exchange(ref DisposingHelper, null)?
						.Dispose(this, OnDispose, calledExplicitly);
				}
			}
		}
    }
}