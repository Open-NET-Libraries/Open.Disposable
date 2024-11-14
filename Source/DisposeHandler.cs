using System;

namespace Open.Disposable;

public class DisposeHandler(Action action)
	: DisposableBase
{
	Action _action = action ?? throw new ArgumentNullException(nameof(action));

	protected override void OnDispose() => Nullify(ref _action).Invoke();
}

public class DisposeHandler<T>(T value, Action action)
	: DisposeHandler(action)
{
	public T Value { get; private set; } = value;

	protected override void OnDispose()
	{
		Value = default!;
		base.OnDispose();
	}
}
