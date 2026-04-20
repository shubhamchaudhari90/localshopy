// /wwwroot/firebase-messaging-sw.js
importScripts("https://www.gstatic.com/firebasejs/10.7.1/firebase-app-compat.js");
importScripts("https://www.gstatic.com/firebasejs/10.7.1/firebase-messaging-compat.js");

const firebaseConfig = {
    apiKey: "AIzaSyCSbdB43LrVMSD4aHDFsLpWhcRwjnWFskI",
    authDomain: "localshopy-shubham.firebaseapp.com",
    projectId: "localshopy-shubham",
    storageBucket: "localshopy-shubham.firebasestorage.app",
    messagingSenderId: "1088554972520",
    appId: "1:1088554972520:web:0bc48a2e179c9e700752dd"
};

firebase.initializeApp(firebaseConfig);
const messaging = firebase.messaging();

// Handle background messages
messaging.onBackgroundMessage(function (payload) {
    console.log("[SW] Background message received:", payload);

    const notificationTitle = payload.notification?.title || "Notification";
    const notificationOptions = {
        body: payload.notification?.body || "",
        icon: payload.notification?.icon || "/img/notification.png",
        data: payload.data || {}
    };

    self.registration.showNotification(notificationTitle, notificationOptions);
});

// Handle notification clicks
self.addEventListener("notificationclick", function (event) {
    console.log("[SW] Notification click received:", event.notification.data);
    event.notification.close();

    event.waitUntil(
        clients.matchAll({ type: "window", includeUncontrolled: true }).then(windowClients => {
            for (let client of windowClients) {
                if (client.url.includes("/") && "focus" in client) return client.focus();
            }
            if (clients.openWindow) return clients.openWindow("/");
        })
    );
});
