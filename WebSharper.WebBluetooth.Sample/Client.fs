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
    // Define the connection to the HTML template
    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    // Variable to display the Bluetooth connection status
    let statusMessage = Var.Create "Click the button to connect Bluetooth."

    // Function to request a Bluetooth device connection
    let connectBluetooth () =
        promise {
            try
                // Request a device supporting the battery service
                let! device = JS.Window.Navigator.Bluetooth.RequestDevice(RequestDeviceOptions(
                    AcceptAllDevices = true,
                    OptionalServices = [| "battery_service" |]
                ))

                // Ensure the device has a valid name
                let deviceName = if isNull(device.Name) then "Unknown Device" else device.Name

                statusMessage := $"Connected to: {deviceName}"
                Console.Log("Device Info:", device)

                // Connect to the GATT server
                let! server = device.Gatt.Connect()
                Console.Log("Connected to GATT Server!")

                // Get the battery service
                let! service = server.GetPrimaryService("battery_service")
                let! characteristic = service.GetCharacteristic("battery_level")

                // Read battery level
                let! value = characteristic.ReadValue() |> Promise.AsAsync
                let batteryLevel = value.GetUint8(0)

                statusMessage := sprintf "%s - Battery: %d%%" statusMessage.Value batteryLevel
            with ex ->
                // Handle errors in Bluetooth connection
                Console.Error("Bluetooth Connection Failed:", ex.Message)
                statusMessage := "Bluetooth connection failed!"
        }

    [<SPAEntryPoint>]
    let Main () =
        // Initialize the UI template and bind Bluetooth connection status
        IndexTemplate.Main()
            .connectBluetooth(fun _ ->
                async {
                    do! connectBluetooth().AsAsync()
                }
                |> Async.Start
            )
            .status(statusMessage.V)
            .Doc()
        |> Doc.RunById "main"