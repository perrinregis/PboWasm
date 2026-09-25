window.wahooBluetooth = {
    device: null,
    server: null,
    powerCharacteristic: null,
    
    connect: async function (dotNetHelper) {
        try {
            console.log("Requesting Bluetooth Device...");
            // Standard Cycling Power Service: 0x1818
            this.device = await navigator.bluetooth.requestDevice({
                filters: [{ services: ['cycling_power'] }],
                optionalServices: ['fitness_machine'] // 0x1826
            });

            console.log("Connecting to GATT Server...");
            this.server = await this.device.gatt.connect();

            console.log("Getting Cycling Power Service...");
            const service = await this.server.getPrimaryService('cycling_power');

            console.log("Getting Cycling Power Measurement Characteristic...");
            // Characteristic 0x2A63
            this.powerCharacteristic = await service.getCharacteristic('cycling_power_measurement');

            await this.powerCharacteristic.startNotifications();
            this.powerCharacteristic.addEventListener('characteristicvaluechanged', (event) => {
                let value = event.target.value;
                // Parse the cycling power measurement characteristic
                // Flags: 16 bit (offset 0)
                // Instantaneous Power: INT16 (offset 2)
                let power = value.getInt16(2, true);
                
                dotNetHelper.invokeMethodAsync('UpdatePower', power);
            });

            console.log("Connected and receiving power data.");
            return "Connecté avec succès";
        } catch (error) {
            console.error("Bluetooth connection failed", error);
            return error.toString();
        }
    },

    disconnect: function () {
        if (this.device && this.device.gatt.connected) {
            this.device.gatt.disconnect();
            console.log("Disconnected");
        }
    }
};
