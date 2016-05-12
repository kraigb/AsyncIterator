AsyncIterator
=============

Two JavaScript classes for creating an iterator that yields the UI thread via setImmediate. The iterators essentially implement a yielding for loop with callback for doing work, aggregating results, and completion.

One class, AsyncIteratorBasic, uses a simple callback for completion, does not accomodate mulitple consumers of the iterator (the completed handler is assigned on construction), and does not report progress.

The second class, AsyncIteratorPromise, uses the Windows Library for JavaScript (WinJS) promise helpers to create an iterator that is consumed as a promise, allowing for multiple consumers, promise chaining, and both error and progress callbacks.

The sample project is provided as a simple Windows Store app; the AsyncIteratorBasic can be used in other projects as it has no outside dependencies.

The js/asynciterator.js file contains the class implementations; js/default.js contains some test code.
