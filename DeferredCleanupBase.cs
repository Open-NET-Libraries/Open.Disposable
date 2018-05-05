/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Open/blob/dotnet-core/LICENSE.md
 */

using Open.Diagnostics;
using System;
using System.Threading;
using System.Threading.Tasks;


namespace Open.Disposable
{
	public abstract class DeferredCleanupBase : DisposableBase
	{

		protected DeferredCleanupBase() : base() { }

		public enum CleanupMode
		{
			ImmediateSynchronous, // Cleanup immediately within the current thread.
			ImmediateSynchronousIfPastDue, // Cleanup immediately if time is past due.
			ImmediateDeferred, // Cleanup immediately within another thread.
			ImmediateDeferredIfPastDue, // Cleanup immedidately in another thread if time is past due.
			Deferred // Extend the timer.
		}

		private int _cleanupDelay = 50;
		// So far 50 ms seems optimal...
		public int CleanupDelay
		{
			get { return _cleanupDelay; }
			set
			{
				if (value < 0)
					throw new ArgumentOutOfRangeException(nameof(value), value, "Cannot be a negative value.");
				_cleanupDelay = value;
			}
		}

		public DateTime LastCleanup
		{
			get;
			private set;
		}

		public bool IsCleanupPastDue
		{
			get { return DateTime.Now.Ticks - (_cleanupDelay + 100) * TimeSpan.TicksPerMillisecond > LastCleanup.Ticks; }
		}

		public bool IsRunning
		{
			get;
			private set;
		}

		private readonly object _timerSync = new Object();
		private Timer _cleanupTimer;


		protected void ResetTimer()
		{
			Timer ct2 = null;
			lock (_timerSync)
			{
				ct2 = Interlocked.Exchange(ref _cleanupTimer, null);
			}
			ct2?.Dispose();
		}

		public void SetCleanup(CleanupMode mode = CleanupMode.Deferred)
		{
			if (IsDisposed)
				return;

			switch (mode)
			{
				case CleanupMode.ImmediateSynchronousIfPastDue:
					if (!IsRunning)
						goto case CleanupMode.Deferred;

					if (IsCleanupPastDue)
						goto case CleanupMode.ImmediateSynchronous;

					break;

				case CleanupMode.ImmediateSynchronous:
					Cleanup();
					break;

				case CleanupMode.ImmediateDeferredIfPastDue:
					if (!IsRunning)
						goto case CleanupMode.Deferred;

					if (IsCleanupPastDue)
						goto case CleanupMode.ImmediateDeferred;

					break;

				case CleanupMode.ImmediateDeferred:
					lock (_timerSync)
					{
						if (!IsDisposed && LastCleanup != DateTime.MaxValue)
						{
							// No past due action in order to prevent another thread from firing...
							LastCleanup = DateTime.MaxValue;
							DeferCleanup();
							Task.Factory.StartNew(() => Cleanup());
						}
					}
					break;

				case CleanupMode.Deferred:
					DeferCleanup();
					break;
			}
		}

		public void DeferCleanup()
		{
			if (!IsDisposed)
			{
				lock (_timerSync)
				{
					if (!IsDisposed)
					{

						IsRunning = true;

						if (_cleanupTimer == null)
							_cleanupTimer = new Timer(Cleanup, null, _cleanupDelay, Timeout.Infinite);
						else
							_cleanupTimer.Change(_cleanupDelay, Timeout.Infinite);
					}
				}
			}
		}

		public void ClearCleanup()
		{
			lock (_timerSync)
			{
				IsRunning = false;
				LastCleanup = DateTime.MaxValue;
				//if(_cleanupTimer!=null)
				//_cleanupTimer.Change(Timeout.Infinite, Timeout.Infinite);
				ResetTimer();
			}
		}

		private void Cleanup(object state = null)
		{
			if (IsDisposed)
				return; // If another thread enters here after disposal don't allow.

			try
			{
				OnCleanup();
			}
			catch (Exception ex)
			{
				ex.WriteToDebug();
			}


			lock (_timerSync)
			{
				LastCleanup = DateTime.Now;
			}

		}

		protected abstract void OnCleanup();

		protected override void OnDispose(bool calledExplicitly)
		{
			ResetTimer();
		}

	}
}
