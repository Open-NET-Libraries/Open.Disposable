/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Open/blob/dotnet-core/LICENSE.md
 */

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Open.Disposable
{
	public class AsyncDisposeHandler : AsyncDisposableBase
	{
		Func<ValueTask> _action;
		public AsyncDisposeHandler(Func<ValueTask> action)
		{
			_action = action ?? throw new ArgumentNullException(nameof(action));
		}

		protected override ValueTask OnDisposeAsync()
			=> Interlocked.Exchange(ref _action, null).Invoke();
	}
}
