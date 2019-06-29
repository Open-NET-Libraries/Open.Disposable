/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Open.Disposable/blob/master/LISCENSE.md
 */

using System.Threading.Tasks;

namespace Open.Disposable
{
	/// <summary>
	/// A base class for properly implementing IAsyncDisposable but also allowing for synchronous use of IDispose.
	/// Only implementing OnDisposeAsync is enough to properly handle disposal.
	/// </summary>
    public abstract class AsyncDisposableBase : DisposableBase
#if NETSTANDARD2_1
			,System.IAsyncDisposable
#endif
	{
		/// <summary>
		/// Without overriding OnDispose, OnDisposeAsync will be called no matter what depending on how the object is disposed.
		/// </summary>
		/// <param name="mode">The method by which disposal was triggered.</param>
		protected abstract ValueTask OnDisposeAsync(AsyncDisposeMode mode);

		/// <inheritdoc />
		/// <summary>
		/// If overridden, expect OnDisposeAsync to be called only when DisposeAsync is called, unless base.OnDispose(calledExplicitly) is implemented.
		/// </summary>
		protected override void OnDispose(bool calledExplicitly)
		{
			var dispose = OnDisposeAsync(calledExplicitly ? AsyncDisposeMode.Dispose : AsyncDisposeMode.Finalized);
			// Was it purely synchronous?
			if(!dispose.IsCompletedSuccessfully)
			{
				/*
				 * If the OnDisposeAsync method doesn't complete immediately
				 * (it could internally defer an action with a task)
				 * then it's a signal that we must wait.
				 */

				dispose.AsTask().Wait();

				/* This is obviously not ideal, but is a good fallback for this case. */
			}
		}

		/// <inheritdoc />
		public ValueTask DisposeAsync()
        {
            /*
             * Note about the BeforeDispose event:
             * Although this is asynchronous, it's not this class' responsibility to decide how subscribers will behave.
             * A subscriber should smartly defer responses when possible, or only respond in a properly synchronous non-blockin way.
             */

            if (!StartDispose(true))
                return new ValueTask();

            var dispose = true;
            try
            {
                var d = OnDisposeAsync(AsyncDisposeMode.DisposeAsync);
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
                await onDispose;
            }
            finally
            {
                Disposed();
            }
        }

    }
}