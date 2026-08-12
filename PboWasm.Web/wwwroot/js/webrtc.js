// WebRTC JS Interop for Blazor WASM
window.webrtcInterop = {
    peerConnection: null,
    dataChannel: null,
    dotnetHelper: null,

    // Initialize the RTCPeerConnection
    initialize: function (dotnetHelper) {
        this.dotnetHelper = dotnetHelper;

        const config = {
            iceServers: [
                { urls: "stun:stun.l.google.com:19302" },
                {
                    urls: "turn:openrelay.metered.ca:80",
                    username: "openrelayproject",
                    credential: "openrelayproject"
                },
                {
                    urls: "turn:openrelay.metered.ca:443",
                    username: "openrelayproject",
                    credential: "openrelayproject"
                },
                {
                    urls: "turn:openrelay.metered.ca:443?transport=tcp",
                    username: "openrelayproject",
                    credential: "openrelayproject"
                }
            ]
        };

        this.peerConnection = new RTCPeerConnection(config);

        // When we get an ICE candidate, send it to the other peer via SignalR
        this.peerConnection.onicecandidate = (event) => {
            if (event.candidate) {
                dotnetHelper.invokeMethodAsync("OnIceCandidate", JSON.stringify(event.candidate));
            }
        };

        // When the other peer opens a DataChannel to us
        this.peerConnection.ondatachannel = (event) => {
            this.dataChannel = event.channel;
            this._setupDataChannel();
        };

        this.peerConnection.onconnectionstatechange = () => {
            dotnetHelper.invokeMethodAsync("OnConnectionStateChanged", this.peerConnection.connectionState);
        };

        console.log("WebRTC PeerConnection initialized.");
    },

    // Create an offer (Peer 1 calls this)
    createOffer: async function () {
        this.dataChannel = this.peerConnection.createDataChannel("chat");
        this._setupDataChannel();

        const offer = await this.peerConnection.createOffer();
        await this.peerConnection.setLocalDescription(offer);
        return JSON.stringify(offer);
    },

    // Handle a received offer and create an answer (Peer 2 calls this)
    handleOffer: async function (offerJson) {
        const offer = JSON.parse(offerJson);
        await this.peerConnection.setRemoteDescription(new RTCSessionDescription(offer));

        const answer = await this.peerConnection.createAnswer();
        await this.peerConnection.setLocalDescription(answer);
        return JSON.stringify(answer);
    },

    // Handle a received answer (Peer 1 calls this)
    handleAnswer: async function (answerJson) {
        const answer = JSON.parse(answerJson);
        await this.peerConnection.setRemoteDescription(new RTCSessionDescription(answer));
    },

    // Handle a received ICE candidate
    addIceCandidate: async function (candidateJson) {
        const candidate = JSON.parse(candidateJson);
        await this.peerConnection.addIceCandidate(new RTCIceCandidate(candidate));
    },

    // Send a message via the DataChannel (P2P, no server!)
    sendMessage: function (message) {
        if (this.dataChannel && this.dataChannel.readyState === "open") {
            this.dataChannel.send(message);
            return true;
        }
        return false;
    },

    // Internal: wire up DataChannel events
    _setupDataChannel: function () {
        this.dataChannel.onopen = () => {
            console.log("DataChannel is open!");
            this.dotnetHelper.invokeMethodAsync("OnDataChannelStateChanged", "open");
        };
        this.dataChannel.onclose = () => {
            console.log("DataChannel is closed.");
            this.dotnetHelper.invokeMethodAsync("OnDataChannelStateChanged", "closed");
        };
        this.dataChannel.onmessage = (event) => {
            this.dotnetHelper.invokeMethodAsync("OnMessageReceived", event.data);
        };
    },

    // Read an image, resize it to fit within 800x800, compress to JPEG and send it
    sendImageFromInput: async function (inputElementId) {
        const input = document.getElementById(inputElementId);
        if (!input || !input.files || input.files.length === 0) return null;
        const file = input.files[0];
        
        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.onload = (e) => {
                const img = new Image();
                img.onload = () => {
                    const canvas = document.createElement("canvas");
                    const MAX_WIDTH = 800;
                    const MAX_HEIGHT = 800;
                    let width = img.width;
                    let height = img.height;

                    if (width > height) {
                        if (width > MAX_WIDTH) {
                            height *= MAX_WIDTH / width;
                            width = MAX_WIDTH;
                        }
                    } else {
                        if (height > MAX_HEIGHT) {
                            width *= MAX_HEIGHT / height;
                            height = MAX_HEIGHT;
                        }
                    }
                    canvas.width = width;
                    canvas.height = height;
                    const ctx = canvas.getContext("2d");
                    ctx.drawImage(img, 0, 0, width, height);
                    
                    const dataUrl = canvas.toDataURL("image/jpeg", 0.6);
                    
                    const payload = JSON.stringify({ type: 'image', content: dataUrl });
                    
                    if (this.dataChannel && this.dataChannel.readyState === "open") {
                        try {
                            this.dataChannel.send(payload);
                            resolve(dataUrl);
                        } catch(err) {
                            console.error("Failed to send image", err);
                            resolve(null);
                        }
                    } else {
                        resolve(null);
                    }
                };
                img.src = e.target.result;
            };
            reader.readAsDataURL(file);
        });
    }
};
