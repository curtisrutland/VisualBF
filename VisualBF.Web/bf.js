$(document).ready(function () {
    $('#submit').click(function () {
        var val = $('#input').val();
        interpret(val);
    });

    var iput;

    function interpret(input) {
        $('#output').empty();
        iput = '';
        var data = Array(30000);
        for (var i = 0; i < data.length; i++)
            data[i] = 0;
        var loopIndexes = new Array;
        var source = cleanse(input);
        $('#input').val(source)
        var iPtr = 0;
        var dPtr = 0;
        while (iPtr < source.length) {
            switch (source[iPtr]) {
                case '>':
                    ++dPtr;
                    if (dPtr >= data.length) {
                        setError("Pointer overflow at " + iPtr + ".");
                        return;
                    }
                    break;
                case '<':
                    --dPtr;
                    if (dPtr < 0) {
                        setError("Pointer underflow at " + iPtr + ".");
                        return;
                    }
                    break;
                case '+':
                    if (data[dPtr] > 255)
                        data[dPtr] = 0
                    else ++data[dPtr];
                    break;
                case '-':
                    if (data[dPtr] <= 0)
                        data[dPtr] = 255
                    else --data[dPtr];
                    break;
                case '.':
                    $('#output').append(String.fromCharCode(data[dPtr]));
                    break;
                case ',':
                    //data[dPtr] = prompt("Please enter a byte","");
                    data[dPtr] = getInput();
                    break;
                case '[':
                    loopIndexes.push(iPtr);
                    var endLoopIndex = findBalancingBrace(source, iPtr);
                    if (data[dPtr] == 0) {
                        if (endLoopIndex == -1) {
                            setError("Syntax Error: Unbalanced brace detected at " + iPtr + ".");
                            return;
                        }
                        else
                            iPtr = endLoopIndex;
                    }
                    break;
                case ']':
                    if (loopIndexes.length == 0) {
                        setError("Syntax Error: Unbalanced brace detected at " + iPtr + ".");
                        return;
                    }
                    if (data[dPtr] != 0) {
                        iPtr = loopIndexes.pop()
                        loopIndexes.push(iPtr);
                    }
                    else loopIndexes.pop();
                    break;
            }
            ++iPtr;
        }
    }

    function getInput() {
        if (iput === '')
            iput = prompt('Provide input:');
        if (!iput || iput === '')
            return 0;
        else {
            var res = iput.charCodeAt(0);
            iput = iput.substring(1);
            return res;
        }
    }

    function setError(e) {
        $('#output').empty();
        $('#output').append(e);
    }

    function findBalancingBrace(s, startFrom) {
        var unmatched = 1;
        for (var i = startFrom; i < input.length; i++) {
            if (input[i] == '[')
                ++unmatched;
            else if (input[i] == ']')
                if (--unmatched == 0)
                    return i;
        }
        return -1;
    }

    function cleanse(input) {
        var ops = '><+-.,[]';
        var s = '';
        for (var i = 0; i < input.length; i++)
            if (ops.indexOf(input[i]) != -1)
                s += input[i];
        return s;
    }
});