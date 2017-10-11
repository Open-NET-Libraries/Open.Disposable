﻿/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Open/blob/dotnet-core/LICENSE.md
 */

using System;
using System.Collections.Generic;
using System.Threading;


namespace Open.Disposable
{

    /// <summary>
    /// A base class for implementing other disposables.  Properly implements the (thread-safe) dispose pattern using DisposeHelper.
    /// 
    /// Provides useful properties and methods to allow for checking if this instance has already been disposed and provides a "BeforeDispose" event for other classes to react to.
    /// </summary>
    public abstract class DisposableBase : IDisposable
	{
		protected DisposeHelper DisposingHelper = new DisposeHelper();

		#region IDisposable Members
		/// <summary>
		/// Standard IDisposable 'Dispose' method.
		/// Triggers cleanup of this class and suppresses garbage collector usage.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
		}
		#endregion

		protected void Dispose(bool calledExplicitly)
		{
			try
			{
				OnBeforeDispose();
			}
			finally
			{
				Interlocked.Exchange(ref DisposingHelper, null)?
                    .Dispose(this, OnDispose, calledExplicitly);
			}
		}

		// Can occur multiple times.
		protected virtual void OnBeforeDispose() { }

		// Occurs only once.
		protected abstract void OnDispose(bool calledExplicitly);

		// Being called by the GC...
		~DisposableBase()
		{
			Dispose(false);
		}

        /// <summary>
        /// This event is triggered by the DisposeHelper immediately before 
        /// </summary>
		public event EventHandler BeforeDispose;
		internal void FireBeforeDispose()
		{
            // Events should only fire if there are listeners...
            if (BeforeDispose != null)
			{
				BeforeDispose(this, EventArgs.Empty);
				BeforeDispose = null;
			}
		}

		public bool IsDisposed
		{
			get
			{
				return !(DisposingHelper?.IsAlive ?? false);
			}
		}

		public bool AssertIsAlive()
		{
			//Contract.Ensures(!IsDisposed);
			if (IsDisposed)
				throw new ObjectDisposedException(this.ToString());

			return true;
		}

		protected static TNullable Nullify<TNullable>(ref TNullable x)
			where TNullable : class
		{
			var y = x;
			x = null;
			return y;
		}

		protected static void DisposeOf<T>(ref T x)
			where T : class, IDisposable
		{
			var y = x;
			x = null;
			y?.Dispose();
		}

	}

	public static class DisposableExtensions
	{

        /// <summary>
        /// A useful utility extension for disposing of a list of disposables.
        /// </summary>
        /// <param name="target"></param>
		public static void DisposeAll(this IEnumerable<IDisposable> target)
		{
			if (target == null)
				throw new ArgumentNullException("target");

			foreach (var d in target)
			{
				if (d != null)
					d.Dispose();
			}
		}

        /// <summary>
        /// This extension calls .Clear() on a collection and then .Dispose() if that collection is IDisposable.
        /// </summary>
        /// <typeparam name="T">The generic type for the collection.</typeparam>
        /// <param name="target">The target collection to be disposed.</param>
		public static void Dispose<T>(this ICollection<T> target)
		{
			if (target != null)
			{
				target.Clear();
				if (target is IDisposable)
				{
					((IDisposable)target).Dispose();
				}
			}
		}
	}
}
