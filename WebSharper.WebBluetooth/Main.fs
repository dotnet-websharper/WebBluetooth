namespace WebSharper.WebBluetooth

open WebSharper
open WebSharper.JavaScript
open WebSharper.InterfaceGenerator

module Definition =
    
    let ManufacturerDataFilter =
        Pattern.Config "ManufacturerDataFilter" {
            Required = [
                "companyIdentifier", T<int>
            ]
            Optional = [
                "dataPrefix", T<ArrayBuffer>
                "mask", T<ArrayBuffer>
            ]
        }
    
    let ServiceDataFilter =
        Pattern.Config "ServiceDataFilter" {
            Required = [
                "service", T<string>
            ]
            Optional = [
                "dataPrefix", T<ArrayBuffer>
                "mask", T<ArrayBuffer>
            ]
        }
    
    let BluetoothDeviceFilter =
        Pattern.Config "BluetoothDeviceFilter" {
            Required = []
            Optional = [
                "services", !| T<string>
                "name", T<string>
                "namePrefix", T<string>
                "manufacturerData", !| ManufacturerDataFilter.Type
                "serviceData", !| ServiceDataFilter.Type
            ]
        }
    
    let RequestDeviceOptions =
        Pattern.Config "RequestDeviceOptions" {
            Required = []
            Optional = [
                "filters", !| BluetoothDeviceFilter.Type
                "exclusionFilters", !| BluetoothDeviceFilter.Type
                "optionalServices", !| T<string>
                "optionalManufacturerData", !| T<int>
                "acceptAllDevices", T<bool>
            ]
        }

    let BluetoothRemoteGATTServer =
        Class "BluetoothRemoteGATTServer"
    
    let BluetoothDevice =
        Class "BluetoothDevice"
        |=> Inherits T<Dom.EventTarget>
        |+> Instance [
            "id" =? T<string>
            "name" =? T<string>
            "gatt" =? BluetoothRemoteGATTServer.Type 
        ]
    
    let BluetoothCharacteristicProperties =
        Class "BluetoothCharacteristicProperties"
        |+> Instance [
            "authenticatedSignedWrites" =? T<bool>
            "broadcast" =? T<bool>
            "indicate" =? T<bool>
            "notify" =? T<bool>
            "read" =? T<bool>
            "reliableWrite" =? T<bool>
            "writableAuxiliaries" =? T<bool>
            "write" =? T<bool>
            "writeWithoutResponse" =? T<bool>
        ]

    let BluetoothRemoteGATTService =
        Class "BluetoothRemoteGATTService"

    let BluetoothRemoteGATTCharacteristic =
        Class "BluetoothRemoteGATTCharacteristic"

    let BluetoothRemoteGATTDescriptor =
        Class "BluetoothRemoteGATTDescriptor"
        |+> Instance [
            "characteristic" =? BluetoothRemoteGATTCharacteristic.Type 
            "uuid" =? T<string> 
            "value" =? T<DataView> 
            
            "readValue" => T<unit> ^-> T<Promise<ArrayBuffer>>
            "writeValue" => T<ArrayBuffer>?array ^-> T<Promise<unit>>
        ]

    BluetoothRemoteGATTCharacteristic
    |=> Inherits T<Dom.EventTarget> 
    |+> Instance [
        "service" =? BluetoothRemoteGATTService 
        "uuid" =? T<string> 
        "properties" =? BluetoothCharacteristicProperties 
        "value" =? T<DataView> 
        
        "getDescriptor" => !?T<string>?bluetoothDescriptorUUID ^-> T<Promise<_>>[BluetoothRemoteGATTDescriptor]
        "getDescriptors" => !?T<string>?bluetoothDescriptorUUID ^-> T<Promise<_>>[!| BluetoothRemoteGATTDescriptor]
        "readValue" => T<unit> ^-> T<Promise<DataView>>
        "writeValue" => T<ArrayBuffer>?value ^-> T<Promise<unit>>
        "writeValueWithResponse" => T<ArrayBuffer>?value ^-> T<Promise<unit>>
        "writeValueWithoutResponse" => T<ArrayBuffer>?value ^-> T<Promise<unit>>
        "startNotifications" => T<unit> ^-> T<Promise<_>>[BluetoothRemoteGATTCharacteristic]
        "stopNotifications" => T<unit> ^-> T<Promise<unit>>
    ]
    |> ignore

    BluetoothRemoteGATTService
    |=> Inherits T<Dom.EventTarget> 
    |+> Instance [
        "device" =? BluetoothDevice
        "isPrimary" =? T<bool>
        "uuid" =? T<string>

        "getCharacteristic" => T<string>?characteristic ^-> T<Promise<_>>[BluetoothRemoteGATTCharacteristic]
        "getCharacteristics" => T<string>?characteristic ^-> T<Promise<_>>[!|BluetoothRemoteGATTCharacteristic]
    ]
    |> ignore
    
    BluetoothRemoteGATTServer
    |+> Instance [
        "connected" =? T<bool>
        "device" =? BluetoothDevice.Type

        "connect" => T<unit> ^-> T<Promise<_>>[BluetoothRemoteGATTServer]
        "disconnect" => T<unit> ^-> T<unit>
        "getPrimaryService" => T<string>?bluetoothServiceUUID ^-> T<Promise<_>>[BluetoothRemoteGATTService]
        "getPrimaryServices" => T<string>?bluetoothServiceUUID ^-> T<Promise<_>>[!|BluetoothRemoteGATTService]
    ] |> ignore
        
    let BluetoothService =
        Class "BluetoothService"
        |+> Instance [
            "uuid" =? T<string>
            "getCharacteristic" => T<string> ^-> T<Promise<obj>> 
        ]
    
    let BluetoothCharacteristic =
        Class "BluetoothCharacteristic"
        |+> Instance [
            "uuid" =? T<string>
            "properties" =? BluetoothCharacteristicProperties.Type
            "readValue" => T<unit> ^-> T<Promise<DataView>>
            "writeValue" => T<ArrayBuffer>?value ^-> T<Promise<unit>>
            "startNotifications" => T<unit> ^-> T<Promise<unit>>
            "stopNotifications" => T<unit> ^-> T<Promise<unit>>
        ]
    
    let Bluetooth =
        Class "Bluetooth"
        |=> Inherits T<Dom.EventTarget>
        |+> Instance [
            "getAvailability" => T<unit> ^-> T<Promise<bool>>
            "getDevices" => T<unit> ^-> T<Promise<_>>[!|BluetoothDevice]
            "requestDevice" => !?RequestDeviceOptions?options ^-> T<Promise<_>>[BluetoothDevice]
        ]

    let BluetoothUUID =
        Class "BluetoothUUID"
        |+> Static [
            "canonicalUUID" => T<string>?alias ^-> T<string> 
            "getCharacteristic" => T<string>?name ^-> T<string> 
            "getDescriptor" => T<string>?name ^-> T<string> 
            "getService" => T<string>?name ^-> T<string> 
        ]
    
    let Navigator =
        Class "Navigator"
        |+> Instance [
            "bluetooth" =? Bluetooth.Type
        ]

    let Assembly =
        Assembly [
            Namespace "WebSharper.WebBluetooth" [
                 Navigator
                 BluetoothUUID
                 Bluetooth
                 BluetoothCharacteristic
                 BluetoothRemoteGATTDescriptor
                 BluetoothRemoteGATTCharacteristic
                 BluetoothRemoteGATTService
                 BluetoothCharacteristicProperties
                 BluetoothDevice
                 BluetoothRemoteGATTServer
                 RequestDeviceOptions
                 BluetoothDeviceFilter
                 ServiceDataFilter
                 ManufacturerDataFilter
            ]
        ]

[<Sealed>]
type Extension() =
    interface IExtension with
        member ext.Assembly =
            Definition.Assembly

[<assembly: Extension(typeof<Extension>)>]
do ()
