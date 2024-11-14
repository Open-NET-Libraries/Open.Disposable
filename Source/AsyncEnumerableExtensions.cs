#if NETSTANDARD2_0
#else
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Open.Disposable;
public static class AsyncEnumerableExtensions
{
	/// <summary>
	/// Allows subscribing to an IAsyncEnumerable.
	/// </summary>
	/// <remarks>
	/// The 'BeforeDispose' event provided by the AsyncDisposeHandler allows for preemptive cleanup of a disposable.
	/// </remarks>
	/// <typeparam name="T">The type emitted from the source.</typeparam>
	/// <param name="source">The IAsyncEnumerable providing the values for observation.</param>
	/// <param name="observer">The receiving observer.</param>
	/// <returns>An AsyncDisposeHandler (IAsyncDisposable).</returns>
	public static AsyncDisposeHandler Subscribe<T>(
		this IAsyncEnumerable<T> source,
		IObserver<T> observer)
	{
		if (source is null) throw new ArgumentNullException(nameof(source));
		if (observer is null) throw new ArgumentNullException(nameof(observer));

		var tokenSource = new CancellationTokenSource();
		var token = tokenSource.Token;
		var enumerator = source.GetAsyncEnumerator(token);
		Task? task = null;
		var handler = new AsyncDisposeHandler(async () =>
		{
			if (!task!.IsCompleted)
			{
				tokenSource.Cancel();
				await task!;
			}
			tokenSource.Dispose();
		});

		task = Task.Run(
			async () =>
			{
				try
				{
					while (await enumerator.MoveNextAsync())
						observer.OnNext(enumerator.Current);
					if (!token.IsCancellationRequested)
						observer.OnCompleted();
				}
				catch (OperationCanceledException)
				{
				}
				catch (Exception ex)
				{
					observer.OnError(ex);
				}
			});

		task.ContinueWith(async _ => await handler.DisposeAsync());

		return handler;
	}

	/// <summary>
	/// Allows subscribing to an IAsyncEnumerable.
	/// </summary>
	/// <remarks>
	/// The 'BeforeDispose' event provided by the AsyncDisposeHandler allows for preemptive cleanup of a disposable.
	/// </remarks>
	/// <typeparam name="T">The type emitted from the source.</typeparam>
	/// <param name="source">The IAsyncEnumerable providing the values for observation.</param>
	/// <param name="onNext">The on-next delegate.</param>
	/// <param name="onComplete">The on-complete delegate.</param>
	/// <param name="onError">The on-error delegate.</param>
	/// <returns>An AsyncDisposeHandler (IAsyncDisposable).</returns>
	public static AsyncDisposeHandler Subscribe<T>(
		this IAsyncEnumerable<T> source,
		Action<T>? onNext,
		Action? onComplete = null,
		Action<Exception>? onError = null)
	{
		var tokenSource = new CancellationTokenSource();
		var token = tokenSource.Token;
		var enumerator = source.GetAsyncEnumerator(token);
		Task? task = null;
		var handler = new AsyncDisposeHandler(async () =>
		{
			if (!task!.IsCompleted)
			{
				tokenSource.Cancel();
				await task!;
			}
			tokenSource.Dispose();
		});

		task = Task.Run(
			async () =>
			{
				try
				{
					while (await enumerator.MoveNextAsync())
						onNext?.Invoke(enumerator.Current);
					if (!token.IsCancellationRequested)
						onComplete?.Invoke();
				}
				catch (OperationCanceledException)
				{
				}
				catch (Exception ex)
				{
					onError?.Invoke(ex);
				}
			});

		task.ContinueWith(async _ => await handler.DisposeAsync());

		return handler;
	}
}
#endif
