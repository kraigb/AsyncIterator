(function () {

WinJS.Namespace.define("Utilities", {
    AsyncIteratorBasic: function (min, max, step, initialResult, workFunction, aggregator, completedHandler) {
        /// <signature  helpKeyword="Utilities.AsyncIterator">
        /// <summary>
        /// Implementation of an asynchronous for loop, where we yield to the UI thread between each
        /// iteration.
        /// </summary>
        /// <param name="min" type="Number">
        /// The beginning point of the iterations.
        /// </param>
        /// <param name="max" type="Number">
        /// The ending point of the iterations.
        /// </param>
        /// <param name="step" type="Number">
        /// The step to take between iterations.
        /// </param>
        /// <param name="initialResult" type="Object">
        /// The initial value for the result. This can be a number, an object, an array, etc., depending
        /// on how it's treated within the other functons.
        /// </param>
        /// <param name="workFunction" type="Function">
        /// The function that performs the work of each iteration, returning any results from that iteration.
        /// </param>
        /// <param name="aggregator" type="Function">
        /// A function that aggregates intermediate results from workFunction into the final results. 
        /// </param>
        /// <param name="completedHandler" type="Function">
        /// The function that is called with the final results.
        /// </param>
        /// </signature>

        //Return on error conditions

        //Nothing to do is max isn't higher than min and the step
        if (max < min + step) {
            return;
        }

        if (workFunction == null || typeof workFunction != "function" 
            || aggregator == null || typeof aggregator != "function"
            || completedHandler == null || typeof completedHandler != "function") {
            return;
        }

        this.iteration = min;
        this.aggregateResult = initialResult;
        this.intermediateResult;
        this.canceled = false;

        this._iterate = function (start, end) {
            //If canceled, we don't call setImmediate again and thus discard the work
            if (this.canceled) {
                return;
            }

            this.intermediateResult = workFunction(start, end); 
            this.aggregateResult = aggregator(this.aggregateResult, this.intermediateResult);
            this.iteration += step;

            if (this.iteration >= max) {
                completedHandler(this.aggregateResult);
            } else {
                //Do next iteration after yielding
                setImmediate(this._iterate.bind(this), end, Math.min(end + step, max));
            }
        }

        this.cancel = function () {
            /// <signature helpKeyword="Utilities.AsyncIterator.cancel">
            /// <summary>
            /// Cancels an async iteration in progress. Has no effect on a completed iteration.
            /// </summary>
            /// </signature>
            this.canceled = true;
        }

        //Kick off the work
        setImmediate(this._iterate.bind(this), 0, step);
    },


    AsyncIteratorPromise: function (min, max, step, initialResult, workFunction, aggregator) {
        /// <signature  helpKeyword="Utilities.AsyncIterator">
        /// <summary>
        /// Implementation of an asynchronous for loop, where we yield to the UI thread between each
        /// iteration.
        /// </summary>
        /// <param name="min" type="Number">
        /// The beginning point of the iterations.
        /// </param>
        /// <param name="max" type="Number">
        /// The ending point of the iterations.
        /// </param>
        /// <param name="step" type="Number">
        /// The step to take between iterations.
        /// </param>
        /// <param name="initialResult" type="Object">
        /// The initial value for the result. This can be a number, an object, an array, etc., depending
        /// on how it's treated within the other functons.
        /// </param>
        /// <param name="workFunction" type="Function">
        /// The function that performs the work of each iteration, returning any results from that iteration.
        /// </param>
        /// <param name="aggregator" type="Function">
        /// A function that aggregates intermediate results from workFunction into the final results. 
        /// </param>
        /// <returns type="Object">
        /// A promise that's fulfilled when the work is complete.
        /// </returns>
        /// </signature>

        this.iteration = min;
        this.aggregateResult = initialResult;
        this.intermediateResult;
        this.canceled = false;
        this.completedHandler = null;
        this.errorHandler = null;
        this.progressHandler = null;

        this._iterate = function (start, end) {
            //If canceled, we don't call setImmediate again and thus discard the work. Note that this is
            //not an error condition; we neither call the completed nor error handlers.
            if (this.canceled) {
                return;
            }
            
            this.intermediateResult = workFunction(start, end);
            this.aggregateResult = aggregator(this.aggregateResult, this.intermediateResult);
            this.iteration += step;

            if (this.iteration >= max) {
                if (this.completedHandler) {
                    this.completedHandler(this.aggregateResult);
                }                
            } else {
                //Report progress
                if (this.progressHandler) {
                    this.progressHandler(this.aggregateResult);
                }

                //Do next iteration after yielding
                setImmediate(this._iterate.bind(this), end, Math.min(end + step, max));                    
            }
        }


        //Initializer function that's called when we create the promise
        this._init = function (c, e, p) {
            this.completedHandler = c;
            this.errorHandler = e;
            this.progressHandler = p;

            //Nothing to do is max isn't higher than min and the step, so call error function
            if (max < min + step) {
                if (this.errorHandler) {
                    this.errorHandler(new WinJS.ErrorFromName("Bad parameters", "max is less than min + step"));
                    return;
                }
            }

            if (workFunction == null || typeof workFunction != "function") {
                if (this.errorHandler) {
                    this.errorHandler(new WinJS.ErrorFromName("Bad parameter", "workFunction must be a valid function"));
                    return;
                }
            }

            if (aggregator == null || typeof aggregator != "function") {
                if (this.errorHandler) {
                    this.errorHandler(new WinJS.ErrorFromName("Bad parameter", "aggregator must be a valid function"));
                    return;
                }
            }

            //Kick off the work as soon as we're constructed
            setImmediate(this._iterate.bind(this), 0, step);
        };

        //Cancelation function called if promise.cancel() is called in the consumer
        this._cancel = function () {
            _canceled = true;
        };

        //Construct the promise with these two functions
        return new WinJS.Promise(this._init.bind(this), this._cancel.bind(this));
    }
});


})();