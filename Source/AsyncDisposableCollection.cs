#if NETSTANDARD2_0
# else
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Open.Disposable;

public class AsyncDisposableCollection : AsyncDisposableBase
{
	readonly LinkedList<object> Disposables = new();
	readonly Dictionary<object, LinkedListNode<object>> Lookup = [];

	public void Add(object disposable)
	{
		switch (disposable)
		{
			case IAsyncDisposable a:
				Add(a);
				break;

			case IDisposable d:
				Add(d);
				break;

			default:
				throw new ArgumentException("Is not IDisposable or IAsyncDisposable.", nameof(disposable));
		}
	}

	public void Add(IDisposable disposable)
	{
		if (disposable is DisposableStateBase d) Add(d);
		else AddWithLookup(disposable);
	}

	public void Add(IAsyncDisposable disposable)
	{
		if (disposable is DisposableStateBase d) Add(d);
		else AddWithLookup(disposable);
	}

	private void Add(DisposableStateBase disposable)
	{
		var node = AddWithLookup(disposable);
		disposable.BeforeDispose += Disposable_BeforeDispose;
	}

	private void Disposable_BeforeDispose(object? sender, EventArgs _) => Remove(sender!);

	private LinkedListNode<object> AddWithLookup(object disposable)
	{
		AssertIsAlive();
		if (disposable is null) throw new ArgumentNullException(nameof(disposable));
		var node = Disposables.AddFirst(disposable);
		Lookup.Add(disposable, node);
		return node;
	}

	public bool Remove(object disposable)
		=> Lookup.TryGetValue(disposable, out var node)
		&& node is not null
		&& Remove(node);

	private bool Remove(LinkedListNode<object> node)
	{
		var list = node.List;
		if (!Lookup.Remove(node.Value) || list is null)
			throw new InvalidOperationException("Potential concurrent access.");

		list.Remove(node);
		if (node.Value is DisposableStateBase d)
			d.BeforeDispose -= Disposable_BeforeDispose;

		return true;
	}

	public void Clear()
	{
		AssertIsAlive();
		var first = Disposables.First;
		while (first is not null)
		{
			Remove(first);
			first = Disposables.First;
		}
		Debug.Assert(Lookup.Count == 0);
	}

	protected override async ValueTask OnDisposeAsync()
	{
		var first = Disposables.First;
		while (first is not null)
		{
			Remove(first);
			switch (first.Value)
			{
				case IAsyncDisposable a:
					await a.DisposeAsync().ConfigureAwait(false);
					break;

				case IDisposable d:
					d.Dispose();
					break;
			}
			first = Disposables.First;
		}
		Debug.Assert(Lookup.Count == 0);
	}
}
#endif
