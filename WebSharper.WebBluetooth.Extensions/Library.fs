namespace WebSharper.WebBluetooth

open WebSharper
open WebSharper.JavaScript

[<JavaScript; AutoOpen>]
module Extensions =

    type Navigator with
        [<Inline "$this.bluetooth">]
        member this.Bluetooth with get(): Bluetooth = X<Bluetooth>
