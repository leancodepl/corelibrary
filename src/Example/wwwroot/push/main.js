'use strict';

const applicationServerPublicKey = 'AIzaSyADO0MIBxxPE4L4kYJuM6KSE4dJ7Fb-HRE';
const uid = 'a329159c-1600-4308-ad17-ca23f9690588';

(function () {
    const messaging = firebase.messaging();

    messaging.requestPermission()
        .then(function () {
            console.log('Notification permission granted.');
            messaging.getToken()
                .then(function (currentToken) {
                    console.log('Token retrieved.');
                    sendToken(currentToken);
                })
                .catch(function (err) {
                    console.log('An error occurred while retrieving token. ', err);
                    alert('Unable to retrieve token');
                });
        })
        .catch(function (err) {
            console.log('Unable to get permission to notify.', err);
            alert('Unable to get premission to notify');
        });

    messaging.onTokenRefresh(function () {
        messaging.getToken()
            .then(function (refreshedToken) {
                console.log('Token refreshed.');
                sendToken(refreshedToken);
            })
            .catch(function (err) {
                console.log('Unable to retrieve refreshed token ', err);
                alert('Unable to retrieve refreshed token');
            });
    });

    function sendToken(token) {
        var h = new Headers();
        h.append("Content-Type", "application/json");
        fetch("/api/command/LeanCode.Example.CQRS.RegisterToken", {
            method: "POST",
            headers: h,
            body: JSON.stringify({
                "UserId": uid,
                "Token": token
            })
        }).then(function (r) {
            if (r.ok) {
                console.log("Token updated");
            } else {
                console.warn("Cannot update token ", r.statusText)
                alert("Cannot update token")
            }
        }).catch(function (err) {
            console.warn("Cannot update token ", err);
            alert("Cannot update token");
        })
    }
})();
