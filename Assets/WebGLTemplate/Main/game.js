var buildUrl = "Build";
var loaderUrl = buildUrl + "/{{{ LOADER_FILENAME }}}?v={{{ PRODUCT_VERSION }}}";
var config = {
    dataUrl: buildUrl + "/{{{ DATA_FILENAME }}}?v={{{ PRODUCT_VERSION }}}",
    frameworkUrl: buildUrl + "/{{{ FRAMEWORK_FILENAME }}}?v={{{ PRODUCT_VERSION }}}",
    codeUrl: buildUrl + "/{{{ CODE_FILENAME }}}?v={{{ PRODUCT_VERSION }}}",
    streamingAssetsUrl: "StreamingAssets",
    companyName: "{{{ COMPANY_NAME }}}",
    productName: "{{{ PRODUCT_NAME }}}",
    productVersion: "{{{ PRODUCT_VERSION }}}",
};

var gameLoaded = false;
var gameLoadStarted = false;
var container = document.querySelector("#unity-container");
var canvas = document.querySelector("#unity-canvas");
var loadingBar = document.querySelector("#unity-loading-bar");
var progressBarFull = document.querySelector("#unity-progress-bar-full");
var fullscreenButton = document.querySelector("#unity-fullscreen-button");
var mobileWarning = document.querySelector("#unity-mobile-warning");
const errorContainer = document.querySelector("#error-container");
const zoomParam = "zoom";
const lgnParam = "lgn";

if (/iPhone|iPad|iPod|Android/i.test(navigator.userAgent)) {
    container.className = "unity-mobile";
    config.devicePixelRatio = 1;
    mobileWarning.style.display = "block";
    setTimeout(() => {
        mobileWarning.style.display = "none";
    }, 5000);
}
loadingBar.style.display = "block";

var urlParams = new URLSearchParams(window.location.search);
var isZoom = "true" === urlParams.get(zoomParam);
var isLGN = "true" === urlParams.get(lgnParam);
const LGN_unityContainer = document.getElementById('unity-container');

const gameCanvas = document.getElementById("unity-canvas");

const LGN_resize = () => {
    if (isZoom || isLGN) {
        if (!gameLoadStarted && window.innerWidth < 501) {
            canvas.style.display = "none";
            loadingBar.style.display = "none";
            errorContainer.style.display = "block";
        } else {
            canvas.style.display = "block";
            if (!gameLoaded) {
                loadingBar.style.display = "block";
            }
            errorContainer.style.display = "none";
            if (!gameLoadStarted) {
                loadGame();
            }
        }
    }
    if (window.innerWidth / window.innerHeight > 1.7777) {
        LGN_unityContainer.style.maxWidth = `${Math.round(window.innerHeight * 1.7777)}px`;
    }
    else {
        LGN_unityContainer.style.maxWidth = `${Math.round(window.innerWidth)}px`;
    }
}


if (isLGN || isZoom || /Zoom/i.test(navigator.userAgent)) {
    XMLHttpRequest.prototype.originalOpen = XMLHttpRequest.prototype.open;
    XMLHttpRequest.prototype.open = function(_, url) {
        if (url.indexOf('cdp.cloud.unity3d.com') >= 0) {
            arguments[1] = url.replace('https://cdp.cloud.unity3d.com', '/fx/unity/analytics');
        }
        else if (url.indexOf('userreporting.cloud.unity3d.com') >= 0) {
            arguments[1] = url.replace('https://userreporting.cloud.unity3d.com', '/fx/unity/userreporting');
        }
        else if (url.indexOf('data-optout-service.uca.cloud.unity3d.com') >= 0) {
            arguments[1] = url.replace('https://data-optout-service.uca.cloud.unity3d.com', '/fx/unity/data-optout-service');
        }
        else if (url.indexOf('config.uca.cloud.unity3d.com') >= 0) {
            arguments[1] = url.replace('https://config.uca.cloud.unity3d.com', '/fx/unity/config');
        }
        
        let original = this.originalOpen.apply(this, arguments);
        if (url.indexOf('kitsuneapi.com/fx/kitsune/api/login') >= 0) {
            this.withCredentials = true;
        }
        return original;
    }
    
    var lgnSdk = document.createElement("script");
    lgnSdk.src = "lgn-apps.js?v={{{ PRODUCT_VERSION }}}";
    lgnSdk.onload = () => {
        var context = zoomParam;
        if (isLGN)
            context = lgnParam;
        
        console.log("Loading Context: " + context);
        
        LGN.init(context).then(() => {
            if (LGN.runningContext === "inMeeting") {
                window.onresize = LGN_resize;
                LGN_resize();
            } else {
                LGN_unityContainer.innerHTML = '<img style="width: 100%" alt="Please enter a meeting before loading ' + config.productName + '" src="no_meeting.png">';
            }
        });
    };
    document.body.appendChild(lgnSdk);
}
else {
    var videoLib = document.createElement("script");
    videoLib.src = "lgnvideo.js?v={{{ PRODUCT_VERSION }}}";
    document.body.appendChild(videoLib);
    loadGame();
}

function loadGame() {
    gameLoadStarted = true;
    var script = document.createElement("script");
    script.src = loaderUrl;
    script.onload = () => {
        createUnityInstance(canvas, config, (progress) => {
            progressBarFull.style.width = 100 * progress + "%";
            gameLoaded = true;
        }).then((unityInstance) => {
            loadingBar.style.display = "none";
        }).catch((message) => {
            alert(message);
        });
    };
    document.body.appendChild(script);
}

