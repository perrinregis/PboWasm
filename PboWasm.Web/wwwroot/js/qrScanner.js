window.qrScanner = {
    startScanner: async (videoElementId, canvasElementId) => {
        const video = document.getElementById(videoElementId);
        const canvas = document.getElementById(canvasElementId);
        const context = canvas.getContext("2d", { willReadFrequently: true });

        try {
            const stream = await navigator.mediaDevices.getUserMedia({ video: { facingMode: "environment" } });
            video.srcObject = stream;
            video.setAttribute("playsinline", true); // required to tell iOS safari we don't want fullscreen
            video.play();
            
            return new Promise((resolve) => {
                const tick = () => {
                    if (video.readyState === video.HAVE_ENOUGH_DATA) {
                        canvas.height = video.videoHeight;
                        canvas.width = video.videoWidth;
                        context.drawImage(video, 0, 0, canvas.width, canvas.height);
                        const imageData = context.getImageData(0, 0, canvas.width, canvas.height);
                        const code = jsQR(imageData.data, imageData.width, imageData.height, {
                            inversionAttempts: "dontInvert",
                        });
                        if (code) {
                            resolve(code.data);
                            return;
                        }
                    }
                    requestAnimationFrame(tick);
                };
                requestAnimationFrame(tick);
            });
        } catch (err) {
            console.error("Error accessing camera:", err);
            throw err;
        }
    },
    stopScanner: (videoElementId) => {
        const video = document.getElementById(videoElementId);
        if (video && video.srcObject) {
            video.srcObject.getTracks().forEach(track => track.stop());
            video.srcObject = null;
        }
    },
    capturePhoto: (videoElementId, canvasElementId) => {
        const video = document.getElementById(videoElementId);
        const canvas = document.getElementById(canvasElementId);
        const context = canvas.getContext("2d");
        
        canvas.width = video.videoWidth;
        canvas.height = video.videoHeight;
        context.drawImage(video, 0, 0, canvas.width, canvas.height);
        
        return canvas.toDataURL("image/png");
    }
};
