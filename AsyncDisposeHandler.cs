using System;
using System.Threading.Tasks;

namespace Open.Disposable;

public class AsyncDisposeHandler : AsyncDisposableBase
{
	public AsyncDisposeHandler(Func<ValueTask> action) => _action = action ?? throw new ArgumentNullException(nameof(action));

	Func<ValueTask> _action;

	protected override ValueTask OnDisposeAsync() => Nullify(ref _action).Invoke();
}

public class AsyncDisposeHandler<T> : AsyncDisposeHandler
{
	public AsyncDisposeHandler(T value, Func<ValueTask> action) : base(action) => Value = value;

	public T Value { get; private set; }

	protected override ValueTask OnDisposeAsync()
	{
		Value = default!;
		return base.OnDisposeAsync();
	}
}
