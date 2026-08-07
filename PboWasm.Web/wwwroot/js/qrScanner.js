window.qrScanner = {
    html5QrCode: null,
    videoStream: null,
    startScanner: async (videoElementId, canvasElementId, enableQrScan) => {
        return new Promise(async (resolve, reject) => {
            if (enableQrScan) {
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
                        resolve(decodedText);
                    },
                    (errorMessage) => {
                    }
                ).catch(err => {
                    console.error("Erreur Html5Qrcode:", err);
                    reject(err.message || err);
                });
            } else {
                const container = document.getElementById(videoElementId);
                if (!container) {
                    reject("Container not found");
                    return;
                }
                let video = container.querySelector('video');
                if (!video) {
                    video = document.createElement('video');
                    video.style.width = '100%';
                    video.style.height = '100%';
                    video.style.objectFit = 'cover';
                    video.setAttribute("playsinline", true);
                    container.appendChild(video);
                }
                try {
                    const stream = await navigator.mediaDevices.getUserMedia({ video: { facingMode: "environment", width: { ideal: 1280 }, height: { ideal: 720 } } });
                    video.srcObject = stream;
                    video.play();
                    window.qrScanner.videoStream = stream;
                    // Ne resolve pas pour que la camera reste ouverte
                } catch (err) {
                    reject(err);
                }
            }
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
        if (window.qrScanner.videoStream) {
            window.qrScanner.videoStream.getTracks().forEach(track => track.stop());
            window.qrScanner.videoStream = null;
            const container = document.getElementById(videoElementId);
            if (container) container.innerHTML = '';
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
