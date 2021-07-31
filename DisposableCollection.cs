using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Open.Disposable
{
	public class DisposableCollection : DisposableBase
	{
		readonly LinkedList<IDisposable> Disposables = new();
		readonly Dictionary<IDisposable, LinkedListNode<IDisposable>> Lookup = new();

		public void Add(IDisposable disposable)
		{
			if (disposable is DisposableStateBase d) Add(d);
			else AddWithLookup(disposable);
		}

		private void Add(DisposableStateBase disposable)
		{
			var node = AddWithLookup((IDisposable)disposable);
			disposable.BeforeDispose += Disposable_BeforeDispose;
		}

		private void Disposable_BeforeDispose(object? sender, EventArgs _)
			=> Remove((IDisposable)sender!);

		private LinkedListNode<IDisposable> AddWithLookup(IDisposable disposable)
		{
			AssertIsAlive();
			if (disposable is null) throw new ArgumentNullException(nameof(disposable));
			var node = Disposables.AddFirst(disposable);
			Lookup.Add(disposable, node);
			return node;
		}

		public bool Remove(IDisposable disposable)
			=> Lookup.TryGetValue(disposable, out var node) && node is not null && Remove(node);

		private bool Remove(LinkedListNode<IDisposable> node)
		{
			var list = node.List;
			if (!Lookup.Remove(node.Value) || list == null)
				throw new InvalidOperationException("Potential concurrent access.");

			list.Remove(node);
			if (node.Value is DisposableBase d)
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

		protected override void OnDispose()
		{
			var first = Disposables.First;
			while (first is not null)
			{
				Remove(first);
				first.Value.Dispose();
				first = Disposables.First;
			}
			Debug.Assert(Lookup.Count == 0);
		}
	}
}
