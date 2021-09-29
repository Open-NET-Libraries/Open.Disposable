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
		[System.Diagnostics.CodeAnalysis.SuppressMessage(
			"Usage",
			"CA1816:Dispose methods should call SuppressFinalize",
			Justification = "Finalization is too much of an edge case and can degrade performance.")]
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
