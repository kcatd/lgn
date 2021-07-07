var LGNContextInfo = {
    
    $CI_stringToNewUTF8: function (jsString) {
        var length = lengthBytesUTF8(jsString) + 1;
        var cString = _malloc(length);
        stringToUTF8(jsString, cString, length);
        return cString;
    },
    
    CI_getRunningContext: function () {
        return CI_stringToNewUTF8(LGN.runningContext);
    },

    CI_getMeetingID: function () {
        return CI_stringToNewUTF8(LGN.meetingID);
    },

    CI_getMeetingUUID: function () {
        return CI_stringToNewUTF8(LGN.meetingUUID);
    },

    CI_getRole: function () {
        return CI_stringToNewUTF8(LGN.role);
    },

    CI_getScreenName: function () {
        return CI_stringToNewUTF8(LGN.screenName);
    },

    CI_getSessionId: function () {
        return CI_stringToNewUTF8(LGN.sessionId);
    }
};
autoAddDeps(LGNContextInfo, '$CI_stringToNewUTF8');
mergeInto(LibraryManager.library, LGNContextInfo);