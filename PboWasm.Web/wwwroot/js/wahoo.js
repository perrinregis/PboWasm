window.wahooBluetooth = {
    device: null,
    server: null,
    powerCharacteristic: null,
    controlPointCharacteristic: null,
    isMock: false,
    mockInterval: null,
    mockTargetPower: 150,
    
    connect: async function (dotNetHelper, useMock = false) {
        this.isMock = useMock;
        
        if (useMock) {
            console.log("Démarrage du mode simulation (Mock)...");
            this.mockTargetPower = 150;
            
            // Simule l'envoi de données de puissance toutes les secondes
            this.mockInterval = setInterval(() => {
                // Simule une puissance qui fluctue un peu autour de la cible (+/- 5%)
                let fluctuation = (Math.random() * 0.1 - 0.05) * this.mockTargetPower;
                let current = Math.round(this.mockTargetPower + fluctuation);
                if (current < 0) current = 0;
                dotNetHelper.invokeMethodAsync('UpdatePower', current);
            }, 1000);
            
            return "Connecté avec succès (SIMULATEUR)";
        }

        try {
            console.log("Requesting Bluetooth Device...");
            // We look for either Cycling Power or Fitness Machine service
            this.device = await navigator.bluetooth.requestDevice({
                filters: [{ services: ['cycling_power'] }, { services: ['fitness_machine'] }],
                optionalServices: ['cycling_power', 'fitness_machine']
            });

            console.log("Connecting to GATT Server...");
            this.server = await this.device.gatt.connect();

            // 1. Try to get Power Data
            try {
                console.log("Getting Cycling Power Service...");
                const cpService = await this.server.getPrimaryService('cycling_power');
                this.powerCharacteristic = await cpService.getCharacteristic('cycling_power_measurement');
                
                await this.powerCharacteristic.startNotifications();
                this.powerCharacteristic.addEventListener('characteristicvaluechanged', (event) => {
                    let value = event.target.value;
                    let power = value.getInt16(2, true);
                    dotNetHelper.invokeMethodAsync('UpdatePower', power);
                });
            } catch (e) {
                console.warn("Cycling power service not available:", e);
            }

            // 2. Try to get FTMS Control Point for ERG mode
            try {
                console.log("Getting Fitness Machine Service...");
                const ftmsService = await this.server.getPrimaryService('fitness_machine');
                this.controlPointCharacteristic = await ftmsService.getCharacteristic('fitness_machine_control_point');
                
                // Op code 0x00 : Request Control
                console.log("Requesting FTMS control...");
                await this.controlPointCharacteristic.writeValue(new Uint8Array([0x00]));
                console.log("FTMS control granted.");
            } catch(e) {
                console.warn("FTMS control point not available:", e);
            }

            console.log("Connected.");
            return "Connecté avec succès";
        } catch (error) {
            console.error("Bluetooth connection failed", error);
            return error.toString();
        }
    },

    setTargetPower: async function (power) {
        if (this.isMock) {
            console.log("SIMULATEUR: Target power set to " + power + "W");
            this.mockTargetPower = power;
            return true;
        }

        if (!this.controlPointCharacteristic) {
            console.error("FTMS Control point not available.");
            return false;
        }
        try {
            // Op code 0x05 : Set Target Power (SINT16, little-endian)
            const buffer = new ArrayBuffer(3);
            const view = new DataView(buffer);
            view.setUint8(0, 0x05); // Op Code
            view.setInt16(1, power, true); // Power in Watts
            
            await this.controlPointCharacteristic.writeValue(buffer);
            console.log("Target power set to " + power + "W");
            return true;
        } catch(e) {
            console.error("Failed to set target power", e);
            return false;
        }
    },

    disconnect: function () {
        if (this.isMock) {
            if (this.mockInterval) clearInterval(this.mockInterval);
            console.log("SIMULATEUR: Disconnected");
            this.isMock = false;
            return;
        }
        
        if (this.device && this.device.gatt.connected) {
            this.device.gatt.disconnect();
            console.log("Disconnected");
        }
    }
};
