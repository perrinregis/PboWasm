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
    }
};
