# Open.Disposable

Provides a set of useful classes when implementing a disposable.

## Core principles

* For most use cases, disposal should only occur once and be final.
* Implementing `IDisposable` in combination with `IAsyncDisposable` should be avoided as it is extremely difficult to coordinate both methods.
* `.Dispose()` and `.DisposeAsync()` should be thread-safe and calling either multiple times should not block or throw. (Typically done through an interlock method.)

### Avoid anti-patterns

#### Memory

It can be all too easy to allocate more memory by constructing more classes or starting new tasks in order to complete disposal.  This is counter to the intention of cleaning up a class and reducing the amount of work the garbage collector has to do.

#### Plan for the worst, but code for the best

Thread safety is important and in many cases should be assured.  But when the 99% case is the local use of a class, over-engineering can simply create unnecessary overhead and contention.

#### Let the GC do its job

Aggressively attempting to help out the garbage collector can be serious anti-pattern as you are simply slowing down your own application in order to avoid GC operations which might actually be helping your performance in total by deferring cleanup.
