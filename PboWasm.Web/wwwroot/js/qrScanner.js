window.qrScanner = {
    html5QrCode: null,
    startScanner: async (videoElementId, canvasElementId) => {
        return new Promise((resolve, reject) => {
            if (window.qrScanner.html5QrCode) {
                return;
            }
            window.qrScanner.html5QrCode = new Html5Qrcode(videoElementId);
            window.qrScanner.html5QrCode.start(
                { facingMode: "environment" },
                {
                    fps: 15,
                    qrbox: { width: 250, height: 250 }
                },
                (decodedText, decodedResult) => {
                    // Le QR code est détecté avec succès
                    resolve(decodedText);
                },
                (errorMessage) => {
                    // Ignorer les erreurs d'analyse (déclenché pour chaque image sans QR code)
                }
            ).catch(err => {
                console.error("Erreur Html5Qrcode:", err);
                reject(err.message || err);
            });
        });
    },
    stopScanner: async (videoElementId) => {
        if (window.qrScanner.html5QrCode) {
            try {
                await window.qrScanner.html5QrCode.stop();
                window.qrScanner.html5QrCode.clear();
            } catch (err) {
                console.error("Failed to stop scanner", err);
            } finally {
                window.qrScanner.html5QrCode = null;
            }
        }
    },
    capturePhoto: (videoElementId, canvasElementId) => {
        const container = document.getElementById(videoElementId);
        if (container) {
            const video = container.querySelector('video');
            if (video) {
                const canvas = document.createElement("canvas");
                canvas.width = video.videoWidth;
                canvas.height = video.videoHeight;
                const context = canvas.getContext("2d");
                context.drawImage(video, 0, 0, canvas.width, canvas.height);
                return canvas.toDataURL("image/png");
            }
        }
        return null;
    }
};
