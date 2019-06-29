/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIThttps://github.com/electricessence/Open.Disposable/blob/master/LISCENSE.md
 */

using System.Threading.Tasks;

namespace Open.Disposable
{
    public abstract class AsyncDisposableBase
        : DisposableStateBase, Open.Disposable.Async.IAsyncDisposable
    {
        protected abstract ValueTask OnDisposeAsync();

        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            if (!StartDispose())
                return new ValueTask();

            var dispose = true;
            try
            {
                var d = OnDisposeAsync();
                if (!d.IsCompletedSuccessfully)
                {
                    dispose = false;
                    return OnDisposeAsyncInternal();
                }
            }
            finally
            {
                if (dispose) Disposed();
            }

            return new ValueTask();
        }

        private async ValueTask OnDisposeAsyncInternal()
        {
            try
            {
                await OnDisposeAsync();
            }
            finally
            {
                Disposed();
            }
        }

    }
}