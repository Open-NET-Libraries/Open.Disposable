/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIThttps://github.com/electricessence/Open.Disposable/blob/master/LISCENSE.md
 */

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Open.Disposable
{
	public static class DisposableExtensions
	{
		public static bool AssertIsAlive(this IDisposalState state)
		{
			if (state.WasDisposed)
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

		const int MaxPoolArraySize = 1024 * 1024;
		static void Dummy() { }

		/// <summary>
		/// Rents a buffer from the ArrayPool but returns a DisposeHandler with the buffer as it's value.
		/// Facilitiates containing the temporary use of a buffer within a using block.
		/// If the mimimumLength exceeds 1024*1024, an array will be created at that length for use.
		/// </summary>
		/// <typeparam name="T">The type of the array.</typeparam>
		/// <param name="pool">The pool to get the array from.</param>
		/// <param name="minimumLength">The minimum length of the array.</param>
		/// <param name="clearArrayOnReturn">If true, will clear the array when it is returned to the pool.</param>
		/// <returns>A DisposeHandler containing an array of type T[] that is at least minimumLength in length.</returns>
		public static DisposeHandler<T[]> RentDisposable<T>(this ArrayPool<T> pool, int minimumLength, bool clearArrayOnReturn = false)
		{
			// If the size is too large, facilitate getting an array but don't manage the pool.
			if (minimumLength > MaxPoolArraySize)
				return new DisposeHandler<T[]>(new T[minimumLength], Dummy);

			var a = pool.Rent(minimumLength);
			return new DisposeHandler<T[]>(a, () => pool.Return(a, clearArrayOnReturn));
		}
	}
}
