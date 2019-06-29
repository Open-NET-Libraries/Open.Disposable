/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIThttps://github.com/electricessence/Open.Disposable/blob/master/LISCENSE.md
 */
 
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Open.Disposable
{
	public static class DisposableExtensions
	{
		public static bool IsAlive(this IDisposalState state)
			=> state.DisposeState == DisposeState.Alive;

		public static bool WasDisposed(this IDisposalState state)
			=> state.DisposeState != DisposeState.Alive;

		public static bool IfAlive<TState>(this TState state, Action action)
			where TState : class, IDisposalState
		{
			if(state==null || state.WasDisposed()) return false;
			lock(state)
			{
				if(state.WasDisposed()) return false;
				action();
			}
			return true;
		}

        public static bool AssertIsAlive(this IDisposalState state)
		{
			if (state.DisposeState == DisposeState.Alive)
				throw new ObjectDisposedException(state.ToString());

			return true;
		}

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
				d?.Dispose();
		}

		/// <summary>
		/// This extension calls .Clear() on a collection and then .Dispose() if that collection is IDisposable.
		/// </summary>
		/// <typeparam name="T">The generic type for the collection.</typeparam>
		/// <param name="target">The target collection to be disposed.</param>
		/// <param name="disposeContents">If true, will dispose of each item (if disposable) before calling clear.</param>
		public static void Dispose<T>(this ICollection<T> target, bool disposeContents = false)
		{
			if (target == null) return;

			// Disposing of each may trigger events that cause removal of from the underlying collection so allow for that before clearing the collection.
			if (disposeContents)
				foreach (var c in target.ToArray())
					if (c is IDisposable d) d.Dispose();

			target.Clear();
			if (target is IDisposable t) t.Dispose();
		}
	}
}
