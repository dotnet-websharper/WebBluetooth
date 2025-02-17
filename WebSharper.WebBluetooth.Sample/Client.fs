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
    // The templates are loaded from the DOM, so you just can edit index.html
    // and refresh your browser, no need to recompile unless you add or remove holes.
    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    let statusMessage = Var.Create "Click the button to connect Bluetooth."

    let bluetooth = As<Navigator>(JS.Window.Navigator).Bluetooth

    let connectBluetooth () =
        promise {
            try
                let! device = bluetooth.RequestDevice(RequestDeviceOptions(
                    AcceptAllDevices = true,
                    OptionalServices = [| "battery_service" |]
                ))

                let deviceName = if isNull(device.Name) then "Unknown Device" else device.Name

                statusMessage := $"Connected to: {deviceName}"
                Console.Log("Device Info:", device)

                // Connect to GATT server
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
                Console.Error("Bluetooth Connection Failed:", ex.Message)
                statusMessage := "Bluetooth connection failed!"
        }

    [<SPAEntryPoint>]
    let Main () =

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
