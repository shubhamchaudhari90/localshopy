// /wwwroot/js/fcm.js
(async function () {

    if (!("serviceWorker" in navigator) || !("Notification" in window)) {
        console.warn("Service workers or notifications not supported.");
        return;
    }

    // 1️⃣ Import Firebase modules
    const { initializeApp } = await import("https://www.gstatic.com/firebasejs/10.7.1/firebase-app.js");
    const { getMessaging, getToken, onMessage } = await import("https://www.gstatic.com/firebasejs/10.7.1/firebase-messaging.js");

    if (!window.firebaseConfig) {
        console.error("window.firebaseConfig missing!");
        return;
    }

    const app = initializeApp(window.firebaseConfig);
    const messaging = getMessaging(app);

    // 2️⃣ Register service worker
    let registration;
    try {
        registration = await navigator.serviceWorker.register("/firebase-messaging-sw.js");
        console.log("Service Worker registered:", registration);

        registration = await navigator.serviceWorker.ready;
        console.log("Service Worker ready:", registration);
    } catch (err) {
        console.error("SW registration failed:", err);
        return;
    }

    // 3️⃣ Request notification permission
    try {
        const permission = await Notification.requestPermission();
        if (permission !== "granted") {
            console.warn("Notification permission not granted:", permission);
            return;
        }
        console.log("Notification permission granted");
    } catch (err) {
        console.error("Permission request failed:", err);
        return;
    }

    // 4️⃣ Get FCM token
    let token;
    try {
        token = await getToken(messaging, {
            vapidKey: window.firebaseConfig.vapidKey,
            serviceWorkerRegistration: registration
        });
        if (!token) {
            console.warn("No FCM token received");
            return;
        }
        console.log("FCM Token:", token);
    } catch (err) {
        console.error("Error getting FCM token:", err);
        return;
    }

    // 5️⃣ Send token to server
    try {
        console.log('here');
        await fetch("/Order/SaveFcmToken", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(token) // <-- just the token string
        });

        console.log("FCM token sent to server successfully");
    } catch (err) {
        console.error("Failed to send FCM token:", err);
    }

    // 6️⃣ Handle foreground messages
    onMessage(messaging, (payload) => {
        console.log("Foreground message received:", payload);
        if (payload.notification) {
            new Notification(payload.notification.title || "Notification", {
                body: payload.notification.body || "",
                icon: payload.notification.icon || "/img/notification.png"
            });
        }
    });

})();
