(function () {
    "use strict";

    WinJS.Binding.optimizeBindingReferences = true;

    var app = WinJS.Application;
    var activation = Windows.ApplicationModel.Activation;

    app.onactivated = function (args) {
        if (args.detail.kind === activation.ActivationKind.launch) {
            if (args.detail.previousExecutionState !== activation.ApplicationExecutionState.terminated) {
            } else {
            }

            //Note that the button handlers are reentrant as they'll just create additional iterators that
            //run parallel to existing ones. The output will overlap, but all the iterators will run.
            document.getElementById("btnRun1").addEventListener("click", runTest1);
            document.getElementById("btnRun2").addEventListener("click", runTest2);

            args.setPromise(WinJS.UI.processAll());
        }
    };

    app.start();


    //Work and aggregator functions for simple counting
    function calcPartialSum(args) {
        var result = 0;

        for (var i = args.start; i < args.end; i++) {
            result += i;
        };

        return result;
    }

    function addToSum(currentSum, addedAmount) {
        return currentSum + addedAmount;
    }


    //Work and aggregator functions for object construction
    function createObject(args) {
        //Though we just create a simple object here, there could be much more work
        //done in this function, such as retrieving data from a service.
        return { start: args.start, end: args.end }
    }

    function addToObject(currentArray, addedObj) {
        currentArray.push(addedObj);
        return currentArray;
    }


    //Tests for AsyncIteratorBasic

    function runTest1() {
        document.getElementById("outputB1").innerText = "";
        document.getElementById("outputB3").innerText = "";

        //Some test cases for the basic iterator. The first takes a while to calculate (at least running in debug
        //mode in a debugger). The second will take longer, and is canceled when the first is complete. The third
        //should finish more quickly and output first.
        var iterator1 = new Utilities.AsyncIteratorBasic(0, 2000000, 500, 0, calcPartialSum, addToSum, sumComplete1a);
        var iterator2 = new Utilities.AsyncIteratorBasic(0, 6000000, 500, 0, calcPartialSum, addToSum, sumComplete1b);
        var iterator3 = new Utilities.AsyncIteratorBasic(0, 25, 1, [], createObject, addToObject, objectConstructed);

        function sumComplete1a(result) {            
            console.log("Iterator1 complete: result = " + result);
            document.getElementById("outputB1").innerText = result;

            //Demonstrate cancelation with iterator2 that should be running one loop behind iterator1.
            //You can look in it to see the intermediate results.
            iterator2.cancel();
        }

        function sumComplete1b(result) {                        
            console.log("Iterator2 complete: result = " + result);
            document.getElementById("outputB2").innerText = result;
        }

        function objectConstructed(result) {
            var val = JSON.stringify(result);            
            console.log("Object constructed by Iterator3: result = " + val);
            document.getElementById("outputB3").innerText = val;
        }
    }


    //Tests for AsyncIteratorPromise

    function runTest2() {
        document.getElementById("outputP1").innerText = "";
        document.getElementById("outputP1P").innerText = "";
        document.getElementById("outputP3").innerText = "";

        //Some test cases for the promise iterator. The first takes a while to calculate (at least running in debug
        //mode in a debugger). The second will take longer, and is canceled when the first is complete. The third
        //should finish more quickly and output first.
        var promise1 = new Utilities.AsyncIteratorPromise(0, 2000000, 500, 0, calcPartialSum, addToSum);
        var promise2 = new Utilities.AsyncIteratorPromise(0, 6000000, 500, 0, calcPartialSum, addToSum);
        var promise3 = new Utilities.AsyncIteratorPromise(0, 25, 1, [], createObject, addToObject);

        //Instead of passing a completed handler directly to the constructor, we use the promise pattern. This allows
        //chaining of promises as well as having error and progress handlers.
        promise1.done(sumComplete2a, error2a, progress2a);
        promise2.done(sumComplete2b);
        promise3.done(objectConstructed2);

        function sumComplete2a(result) {
            console.log("Promise1 complete: result = " + result);
            document.getElementById("outputP1").innerText = result;

            //Canceling the promise will cancel the operation
            promise2.cancel();
        }

        function error2a(e) {
            document.getElementById("outputP1P").innerText = e.message;            
        }

        function progress2a(result) {            
            document.getElementById("outputP1P").innerText = result;
        }

        function sumComplete2b(result) {
            console.log("Promise2 complete: result = " + result);
            document.getElementById("outputP2").innerText = result;
        }

        function objectConstructed2(result) {            
            var val = JSON.stringify(result);
            console.log("Object constructed by Promise3: result = " + val);
            document.getElementById("outputP3").innerText = val;
        }
    }

})();
