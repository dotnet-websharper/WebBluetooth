namespace WebSharper.WebBluetooth

open WebSharper
open WebSharper.JavaScript
open WebSharper.InterfaceGenerator

module Definition =
    
    let ManufacturerDataFilter =
        Pattern.Config "ManufacturerDataFilter" {
            Required = [
                ("companyIdentifier", T<int>)
            ]
            Optional = [
                "dataPrefix", T<ArrayBuffer>
                "mask", T<ArrayBuffer>
            ]
        }
    
    let ServiceDataFilter =
        Pattern.Config "ServiceDataFilter" {
            Required = [
                ("service", T<string>)
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
            "gatt" =? !?BluetoothRemoteGATTServer.Type

            "watchAdvertisements" => !? T<obj> ^-> T<Promise<unit>>
            "unwatchAdvertisements" => T<unit> ^-> T<unit>
            "forget" => T<unit> ^-> T<Promise<unit>>

            "ongattserverdisconnected" =@ T<unit> ^-> T<unit>
            |> ObsoleteWithMessage "Use OnGattServerDisconnected instead"
            "ongattserverdisconnected" =@ T<Dom.Event> ^-> T<unit>
            |> WithSourceName "OnGattServerDisconnected"
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

        "oncharacteristicvaluechanged" =@ T<unit> ^-> T<unit>
        |> ObsoleteWithMessage "Use OnCharacteristicValueChanged instead"
        "oncharacteristicvaluechanged" =@ T<Dom.Event> ^-> T<unit>
        |> WithSourceName "OnCharacteristicValueChanged"
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
        "getIncludedService" => T<string> ^-> T<Promise<_>>[BluetoothRemoteGATTService]
        "getIncludedServices" => !? T<string> ^-> T<Promise<_>>[!| BluetoothRemoteGATTService]
    ]
    |> ignore
    
    BluetoothRemoteGATTServer
    |+> Instance [
        "connected" =? T<bool>
        "device" =? BluetoothDevice.Type

        "connect" => T<unit> ^-> T<Promise<_>>[BluetoothRemoteGATTServer]
        "disconnect" => T<unit> ^-> T<unit>
        "getPrimaryService" => T<string>?bluetoothServiceUUID ^-> T<Promise<_>>[BluetoothRemoteGATTService]
        "getPrimaryServices" => !? T<string>?bluetoothServiceUUID ^-> T<Promise<_>>[!|BluetoothRemoteGATTService]
    ] |> ignore
        
    let BluetoothAdvertisingEventInit =
        Pattern.Config "BluetoothAdvertisingEventInit" {
            Required = [
                ("device", BluetoothDevice.Type)
            ]
            Optional = [
                "uuids", !| T<string>
                "name", T<string>
                "appearance", T<int>
                "txPower", T<int>
                "rssi", T<int>
                "manufacturerData", T<obj>
                "serviceData", T<obj>
            ]
        }

    let BluetoothAdvertisingEvent =
        Class "BluetoothAdvertisingEvent"
        |=> Inherits T<Dom.Event>
        |+> Static [
            Constructor (T<string>?``type`` * !?BluetoothAdvertisingEventInit?init)
        ]
        |+> Instance [
            "device" =? BluetoothDevice.Type
            "uuids" =? !| T<string>
            "name" =? T<string>
            "appearance" =? T<int>
            "txPower" =? T<int>
            "rssi" =? T<int>
            "manufacturerData" =? T<obj>
            "serviceData" =? T<obj>
        ]

    BluetoothDevice
    |+> Instance [
        "onadvertisementreceived" =@ T<unit> ^-> T<unit>
        |> ObsoleteWithMessage "Use OnAdvertisementReceived instead"
        "onadvertisementreceived" =@ BluetoothAdvertisingEvent ^-> T<unit>
        |> WithSourceName "OnAdvertisementReceived"
    ]
    |> ignore

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

    let Assembly =
        Assembly [
            Namespace "WebSharper.WebBluetooth" [
                 BluetoothUUID
                 Bluetooth
                 BluetoothAdvertisingEvent
                 BluetoothAdvertisingEventInit
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
