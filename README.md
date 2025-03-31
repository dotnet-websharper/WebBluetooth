# WebSharper Web Bluetooth API Binding

This repository provides an F# [WebSharper](https://websharper.com/) binding for the [Web Bluetooth API](https://developer.mozilla.org/en-US/docs/Web/API/Web_Bluetooth_API), enabling seamless communication with Bluetooth devices in WebSharper applications.

## Repository Structure

The repository consists of two main projects:

1. **Binding Project**:

   - Contains the F# WebSharper binding for the Web Bluetooth API.

2. **Sample Project**:
   - Demonstrates how to use the Web Bluetooth API with WebSharper syntax.
   - Includes a GitHub Pages demo: [View Demo](https://dotnet-websharper.github.io/WebBluetooth/).

## Installation

To use this package in your WebSharper project, add the NuGet package:

```bash
   dotnet add package WebSharper.WebBluetooth
```

## Building

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) installed on your machine.

### Steps

1. Clone the repository:

   ```bash
   git clone https://github.com/dotnet-websharper/WebBluetooth.git
   cd WebBluetooth
   ```

2. Build the Binding Project:

   ```bash
   dotnet build WebSharper.WebBluetooth/WebSharper.WebBluetooth.fsproj
   ```

3. Build and Run the Sample Project:

   ```bash
   cd WebSharper.WebBluetooth.Sample
   dotnet build
   dotnet run
   ```

4. Open the hosted demo to see the Sample project in action:
   [https://dotnet-websharper.github.io/WebBluetooth/](https://dotnet-websharper.github.io/WebBluetooth/)

## Example Usage

Below is an example of how to use the Web Bluetooth API in a WebSharper project:

```fsharp
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
```

This example demonstrates how to request and connect to a Bluetooth device, establish a GATT server connection, and retrieve battery level data using the Web Bluetooth API in a WebSharper project.

## Important Considerations

- **Device Compatibility**: Not all Bluetooth devices support the Web Bluetooth API. Ensure the device has GATT support and the required services.
- **Permissions**: Users must grant permission for Bluetooth access when prompted by the browser.
- **Security**: Web Bluetooth only works on secure contexts (HTTPS) for security reasons.
- **Limited Browser Support**: Some browsers may not fully support Web Bluetooth; check [MDN Web Bluetooth API](https://developer.mozilla.org/en-US/docs/Web/API/Web_Bluetooth_API) for the latest compatibility information.
