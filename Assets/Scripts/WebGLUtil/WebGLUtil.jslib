var WebGLUtil = {

    $fallbackCopyTextToClipboard: function(text) {
        var textArea = document.createElement("textarea");
        //
        // *** This styling is an extra step which is likely not required. ***
        //
        // Why is it here? To ensure:
        // 1. the element is able to have focus and selection.
        // 2. if element was to flash render it has minimal visual impact.
        // 3. less flakyness with selection and copying which **might** occur if
        //    the textarea element is not visible.
        //
        // The likelihood is the element won't even render, not even a
        // flash, so some of these are just precautions. However in
        // Internet Explorer the element is visible whilst the popup
        // box asking the user for permission for the web page to
        // copy to the clipboard.
        //

        // Place in top-left corner of screen regardless of scroll position.
        textArea.style.position = 'fixed';
        textArea.style.top = 0;
        textArea.style.left = 0;

        // Ensure it has a small width and height. Setting to 1px / 1em
        // doesn't work as this gives a negative w/h on some browsers.
        textArea.style.width = '2em';
        textArea.style.height = '2em';

        // We don't need padding, reducing the size if it does flash render.
        textArea.style.padding = 0;

        // Clean up any borders.
        textArea.style.border = 'none';
        textArea.style.outline = 'none';
        textArea.style.boxShadow = 'none';

        // Avoid flash of white box if rendered for any reason.
        textArea.style.background = 'transparent';

        textArea.value = text;

        // Avoid scrolling to bottom
        textArea.style.top = "0";
        textArea.style.left = "0";
        textArea.style.position = "fixed";

        document.body.appendChild(textArea);
        textArea.focus();
        textArea.select();

        try {
            var successful = document.execCommand('copy');
            var msg = successful ? 'successful' : 'unsuccessful';
            console.log('Fallback: Copying text command was ' + msg);
        } catch (err) {
            console.log('Fallback: Oops, unable to copy', err);
            window.parent.postMessage({type: 'copy-to-clipboard', toCopy: text}, '*');
        }

        document.body.removeChild(textArea);
    },
    
    copyTextToClipboard: function(inputText) {
        var text = Pointer_stringify(inputText);
        console.log("[WebGLUtil] copyToClipboard(text = " + text + ")");
        
        if (!navigator.clipboard) {
            fallbackCopyTextToClipboard(text);
            return;
        }
        navigator.clipboard.writeText(text).then(function() {
            console.log('Async: Copying to clipboard was successful!');
        }, function(err) {
            console.log('Async: Could not copy text: ', err);
            window.parent.postMessage({type: 'copy-to-clipboard', toCopy: text}, '*');
        });
    },
    
    pasteTextFromClipboard: function (onComplete, buffer) {
        console.log("pasteTextFromClipboard()");
        var handleMessage = function (message) {
            if (message && message.data && message.data.type) {
                if (message.data.type === 'paste-from-clipboard' && message.data.toPaste) {
                    stringToUTF8(message.data.toPaste, buffer, lengthBytesUTF8(message.data.toPaste) + 1);
                    Runtime.dynCall('v', onComplete, []);
                }
            }
            window.removeEventListener('message', handleMessage);
        };
        var success = function (data) {
            console.log("pasteTextFromClipboard() pasted: " + data);
            stringToUTF8(data, buffer, lengthBytesUTF8(data) + 1);
            Runtime.dynCall('v', onComplete, []);
        };
        var fail = function () {
            window.addEventListener('message', handleMessage);
            window.parent.postMessage({type: 'paste-from-clipboard'}, '*');
        };
        if (navigator.clipboard) {
            navigator.clipboard.readText().then(success, fail);
        }
    },
    
    getURLSearchParam: function (name, val) {
        var params = new URLSearchParams(document.location.search.substring(1));
        var paramName = Pointer_stringify(name);
        var value = params.get(paramName);
        
        // if we're in the zoom browser, explicitly set this param
        if (paramName == "zoom" && /Zoom/i.test(navigator.userAgent)){
            value = "true";
        }
        
        if (value) {
            stringToUTF8(value, val, lengthBytesUTF8(value) + 1);
        }
        else {
            stringToUTF8("", val, lengthBytesUTF8("") + 1);
        }
    },
    
    getURL: function (url) {
        var ud = decodeURI(document.location.href);
        return stringToUTF8(ud, url, lengthBytesUTF8(ud) + 1);
    },
    
    openURL: function (url, target) {
        if (window.LGN) {
            LGN.openUrl({url: Pointer_stringify(url), target: Pointer_stringify(target)});
        }
        else {
            window.open(Pointer_stringify(url), Pointer_stringify(target));   
        }
    },

    // XXXHACK: override call in Packages/Analytics Library/Plugins/WindowUtil.jslib
    // That method is not calling Pointer_stringify() on the URL, resulting in an error.
    OpenNewWindow: function (url) {
        window.open(Pointer_stringify(url), "_blank");
    }
    
};

autoAddDeps(WebGLUtil, '$fallbackCopyTextToClipboard');
mergeInto(LibraryManager.library, WebGLUtil);