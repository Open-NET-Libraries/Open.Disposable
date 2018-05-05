using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Open.Disposable
{

	public static class DisposableExtensions
	{

		/// <summary>
		/// A useful utility extension for disposing of a list of disposables.
		/// </summary>
		/// <param name="target"></param>
		public static void DisposeAll(this IEnumerable<IDisposable> target)
		{
			if (target == null)
				throw new ArgumentNullException(nameof(target));
			Contract.EndContractBlock();

			foreach (var d in target)
			{
				d?.Dispose();
			}
		}

		/// <summary>
		/// This extension calls .Clear() on a collection and then .Dispose() if that collection is IDisposable.
		/// </summary>
		/// <typeparam name="T">The generic type for the collection.</typeparam>
		/// <param name="target">The target collection to be disposed.</param>
		/// <param name="disposeContents">If true, will dispose of each item (if disposable) before calling clear.</param>
		public static void Dispose<T>(this ICollection<T> target, bool disposeContents = false)
		{
			if (target != null)
			{
				// Disposing of each may trigger events that cause removal of from the underlying collection so allow for that before clearing the collection.
				if (disposeContents)
				{
					foreach (var c in target.ToArray())
					{
						(c as IDisposable)?.Dispose();
					}
				}

				target.Clear();
				(target as IDisposable)?.Dispose();
			}
		}

		static readonly ActionBlock<IDisposable> Disposer = new ActionBlock<IDisposable>(disposable => disposable.Dispose());

		public static void QueueForDisposal(this IDisposable disposable)
		{
			if (disposable == null) return;
			if (!Disposer.Post(disposable))
				Task.Run(() => disposable.Dispose());
		}

	}
}
