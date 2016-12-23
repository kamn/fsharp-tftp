open System
open System.Net
open System.Net.Sockets

open Types

let getPacketCode (bytes: byte[]) =
    match bytes.[1] with
        | 1uy -> PacketCodeValue.RRQ
        | 2uy -> PacketCodeValue.WRQ
        | 3uy -> PacketCodeValue.DATA
        | 4uy -> PacketCodeValue.ACK
        | 5uy -> PacketCodeValue.ERROR
        | _ -> PacketCodeValue.ERROR

let getFileName (bytes: byte[]) =
    let mutable i = 2
    let mutable str = ""
    while bytes.[i] <> 0uy do
        str <- str +  (bytes.[i] |> Convert.ToChar |> Char.ToString)
        i <- i + 1
    (str, i)

let getModeString startIdx (bytes: byte[]) =
    let mutable i = startIdx + 1
    let mutable str = ""
    while bytes.[i] <> 0uy do
        str <- str +  (bytes.[i] |> Convert.ToChar |> Char.ToString)
        i <- i + 1
    str

let getModeFromString (modeStr: string) =
    match (modeStr.ToLower()) with
        | "netascii" -> TransferMode.NetAscii
        | "octet" -> TransferMode.Octet
        | _ -> raise (new Exception("Not valid mode"))

let sleepWorkflow  = async {
    printfn "Starting sleep workflow at %O" DateTime.Now.TimeOfDay
    do! Async.Sleep 2000
    printfn "Finished sleep workflow at %O" DateTime.Now.TimeOfDay
    }

let basicServerThread () = async {
    let udpServer = new UdpClient(69);
    let serverEP = new IPEndPoint(IPAddress.Any, 0); 
    while true do
        let result = udpServer.Receive(ref serverEP)
        printfn "packetCode %A" (getPacketCode result)
        let (filename, nextIdx) =  getFileName result
        printfn "filename %s" filename
        let mode = getModeString nextIdx result |> getModeFromString
        printfn "mode %A" mode
        //printfn "%A" result
        let! childWorkflow =  Async.StartChild sleepWorkflow
        ()
    } 

//http://stackoverflow.com/questions/26706149/f-continuous-loop-in-f
[<EntryPoint>]
let main argv = 
    printfn "Fsharp TFTP"
    printfn "==="
    printfn ""
    let isServer = Seq.exists (fun elem -> elem = "--server") argv
    if isServer then
        printfn "Server mode"
     
        let cts = new System.Threading.CancellationTokenSource()
        Async.Start(basicServerThread (), cts.Token)
        printfn "Press Enter to exit"
        Console.ReadLine () |> ignore
        cts.Cancel ()
    else
        printfn "Is Client"
    0 // return an integer exit code
