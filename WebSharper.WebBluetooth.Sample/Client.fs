namespace WebSharper.WebBluetooth.Sample

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Notation
open WebSharper.UI.Client
open WebSharper.UI.Templating
open WebSharper.WebBluetooth

[<JavaScript>]
module Client =
    // The template is loaded from the DOM, so editing index.html and refreshing
    // the browser is enough; a rebuild is only needed when holes change.
    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    let status = Var.Create "Ready."
    let deviceName = Var.Create "No device connected"
    let serviceCount = Var.Create "0"
    let batteryLevel = Var.Create "—"
    let logMessage = Var.Create ""

    let appendLog (line: string) =
        logMessage :=
            if logMessage.Value = "" then line
            else logMessage.Value + "\n" + line

    // navigator.bluetooth is undefined in browsers without Web Bluetooth,
    // including most headless / automated Chrome instances. Probe it through a
    // dynamic property read so the check is safe even when it is missing.
    let bluetoothObject: obj = JS.Window.Navigator?bluetooth
    let bluetoothAvailable =
        not (isNull bluetoothObject) && not (bluetoothObject ===. JS.Undefined)

    let capabilities =
        if bluetoothAvailable then
            "Web Bluetooth is available. Pick a BLE device that exposes the standard battery service."
        else
            "Web Bluetooth is not available in this browser. Use Chrome, Edge or Opera over HTTPS (or localhost); headless and automated browsers usually do not expose it."

    let bluetooth = As<Bluetooth> bluetoothObject

    // Request a device, connect to its GATT server, list its primary services,
    // read the battery level characteristic and subscribe to notifications.
    let connect () =
        promise {
            try
                status := "Requesting a Bluetooth device..."
                appendLog "Requesting a device exposing the battery service..."
                let! device =
                    bluetooth.RequestDevice(
                        RequestDeviceOptions(
                            Filters = [| BluetoothDeviceFilter(Services = [| "battery_service" |]) |]))

                let name = if isNull device.Name then "(unnamed device)" else device.Name
                deviceName := name
                appendLog (sprintf "Selected device: %s" name)

                device.OnGattServerDisconnected <- fun _ ->
                    status := "Device disconnected."
                    appendLog "Received gattserverdisconnected."

                let gatt = device.Gatt
                if Optional.isUndefined gatt || isNull (box gatt) then
                    status := "This device does not expose a GATT server."
                    appendLog "device.gatt is undefined."
                else
                    status := "Connecting to the GATT server..."
                    let! server = gatt.Value.Connect()
                    appendLog "Connected to the GATT server."

                    let! services = server.GetPrimaryServices()
                    serviceCount := string services.Length
                    appendLog (sprintf "Found %d primary service(s)." services.Length)

                    let! service = server.GetPrimaryService("battery_service")
                    let! characteristic = service.GetCharacteristic("battery_level")
                    let! value = characteristic.ReadValue()
                    let level = value.GetUint8(0)
                    batteryLevel := sprintf "%d%%" level
                    appendLog (sprintf "Read battery_level = %d%%." level)

                    characteristic.OnCharacteristicValueChanged <- fun _ ->
                        let updated = characteristic.Value
                        if not (isNull (box updated)) then
                            let level = updated.GetUint8(0)
                            batteryLevel := sprintf "%d%%" level
                            appendLog (sprintf "Notification: battery_level = %d%%." level)

                    let! _ = characteristic.StartNotifications()
                    appendLog "Subscribed to battery_level notifications."
                    status := "Connected — reading the battery level and receiving notifications."
            with ex ->
                Console.Error("Bluetooth operation failed:", ex.Message)
                status := "Bluetooth operation failed: " + ex.Message
                appendLog ("Error: " + ex.Message)
        }

    [<SPAEntryPoint>]
    let Main () =
        if not bluetoothAvailable then
            status := "Web Bluetooth is not available in this browser."

        IndexTemplate()
            .connect(fun _ -> connect() |> ignore)
            .status(status.V)
            .device(deviceName.V)
            .serviceCount(serviceCount.V)
            .battery(batteryLevel.V)
            .log(logMessage.V)
            .capabilities(capabilities)
            .Bind()
