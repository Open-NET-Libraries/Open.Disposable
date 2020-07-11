# Open.Disposable

Provides a set of useful classes when implementing a disposable.

[![NuGet](https://img.shields.io/nuget/v/Open.Disposable.svg)](https://www.nuget.org/packages/Open.Disposable/)

## Core principles

* For most use cases, disposal should only occur once and be final.
* Implementing `IDisposable` in combination with `IAsyncDisposable` is not typical and should be avoided.  The consumer should understand the difference.
* `.Dispose()` and `.DisposeAsync()` should be thread-safe and calling either multiple times should not block or throw. (Typically done through an interlock method.)

### Avoid anti-patterns

#### Memory

It can be all too easy to allocate more memory by constructing more classes or starting new tasks in order to complete disposal.  This is counter to the intention of cleaning up a class and reducing the amount of work the garbage collector has to do.

#### Plan for the worst, but code for the best

Thread safety is important and in many cases should be assured.  But when the 99% case is the local use of a class, over-engineering can simply create unnecessary overhead and contention.

#### Let the GC do its job

Aggressively attempting to help out the garbage collector can be a serious anti-pattern as you are simply slowing down your own application in order to avoid GC operations which might actually be helping your performance in total by deferring cleanup.

## Classes

### `DisposableBase`

Simply implement `void OnDispose()` in order to manage disposal.

> Note: `DisposableBase` exposes a `BeforeDispose` event which will be triggered once just before disposing commences.  This allows for others to react to this disposal event.  The `DisposableBase` is still considered 'live' until after the `BeforeDispose` event cycle has completed.

### `AsyncDisposableBase`

Simply implement `ValueTask OnDisposeAsync()` in order to manage disposal.

### `DisposeHandler` & `AsyncDisposeHandler`

A simple set of classes for triggering an action (once) when disposed.  Can also contain a value (T) which is cleared on dispose.
