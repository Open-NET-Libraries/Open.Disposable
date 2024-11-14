using System;
using System.Threading.Tasks;

namespace Open.Disposable;

public class AsyncDisposeHandler(Func<ValueTask> action)
	: AsyncDisposableBase
{
	Func<ValueTask> _action = action ?? throw new ArgumentNullException(nameof(action));

	protected override ValueTask OnDisposeAsync() => Nullify(ref _action).Invoke();
}

public class AsyncDisposeHandler<T>(T value, Func<ValueTask> action)
	: AsyncDisposeHandler(action)
{
	public T Value { get; private set; } = value;

	protected override ValueTask OnDisposeAsync()
	{
		Value = default!;
		return base.OnDisposeAsync();
	}
}
