using System.Threading.Tasks;

namespace Open.Disposable;

/// <summary>
/// A base class for properly implementing IAsyncDisposable but also allowing for synchronous use of IDispose.
/// Only implementing OnDisposeAsync is enough to properly handle disposal.
/// </summary>
public abstract class AsyncDisposableBase : DisposableStateBase
#if NETSTANDARD2_0
#else
		, System.IAsyncDisposable
#endif
{
	/// <summary>
	/// Without overriding OnDispose, OnDisposeAsync will be called no matter what depending on how the object is disposed.
	/// </summary>
	protected abstract ValueTask OnDisposeAsync();

	/// <inheritdoc />
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1229:Use async/await when necessary.")]
	public ValueTask DisposeAsync()
	{
		/*
        * Note about the BeforeDispose event:
        * Although this is asynchronous, it's not this class' responsibility to decide how subscribers will behave.
        * A subscriber should smartly defer responses when possible, or only respond in a properly synchronous non-blockin way.
        */

		if (!StartDispose())
			return new ValueTask();

		var dispose = true;
		try
		{
			var d = OnDisposeAsync();
			if (!d.IsCompletedSuccessfully)
			{
				dispose = false;
				return OnDisposeAsyncInternal(d);
			}
		}
		finally
		{
			if (dispose) Disposed();
		}

		return new ValueTask();
	}

	private async ValueTask OnDisposeAsyncInternal(ValueTask onDispose)
	{
		try
		{
			await onDispose.ConfigureAwait(false);
		}
		finally
		{
			Disposed();
		}
	}
}
