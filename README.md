# Open.Disposable
Provides a set of useful classes when implementing a disposable.

## Expected Dispose Phases
1) State is changed from Alive, to Disposing.
2) Block finalizer if called explicitly.
2) OnBeforeDispose is called.
3) BeforeDispose events are invoked.
4) Async disposal is initiated and waited for.
5) Synchronous disposal is executed.