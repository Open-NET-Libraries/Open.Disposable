/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIThttps://github.com/electricessence/Open.Disposable/blob/master/LISCENSE.md
 */

using System;
using System.Threading;

namespace Open.Disposable
{
	public class DisposeHandler : DisposableBase
	{
		public DisposeHandler(Action action)
		{
			_action = action ?? throw new ArgumentNullException(nameof(action));
		}

		Action _action;

		protected override void OnDispose(bool calledExplicitly)
			=> Interlocked.Exchange(ref _action, null).Invoke();
		
	}

	public class DisposeHandler<T> : DisposeHandler
	{
		public DisposeHandler(T value, Action action) :base(action)
		{
			Value = value;
		}

		public T Value { get; private set; }

		protected override void OnDispose(bool calledExplicitly)
		{
			Value = default;
			base.OnDispose(calledExplicitly);
		}
	}
}
