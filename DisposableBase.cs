/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/Open-NET-Libraries/Open.Disposable/blob/master/LICENSE.md
 */

using System;

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
			if (!StartDispose())
				return;

			try
			{
				OnDispose();
			}
			finally
			{
				Disposed();
			}
		}

		/// <summary>
		/// When implemented will be called (only once) when being disposed.
		/// </summary>
		protected abstract void OnDispose();
	}

}
