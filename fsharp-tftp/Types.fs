//https://tools.ietf.org/html/rfc1350
//http://stackoverflow.com/questions/7101068/tftp-protocol-implementation-and-difference-between-netascii-and-octect
module Types
open System

(*
   TFTP supports five types of packets, all of which have been mentioned
   above:

          opcode  operation
            1     Read request (RRQ)
            2     Write request (WRQ)
            3     Data (DATA)
            4     Acknowledgment (ACK)
            5     Error (ERROR)
*)

type PacketCodeValue = 
    | RRQ   = 1
    | WRQ   = 2
    | DATA  = 3
    | ACK   = 4
    | ERROR = 5


(*
Error Codes

   Value     Meaning

   0         Not defined, see error message (if any).
   1         File not found.
   2         Access violation.
   3         Disk full or allocation exceeded.
   4         Illegal TFTP operation.
   5         Unknown transfer ID.
   6         File already exists.
   7         No such user.
*)
type ErrorCodeValue = 
    | NotDefined = 0
    | FileNotFound = 1
    | AccessViolation = 2
    | DiskFull = 3
    | IllegalOperation = 4
    | UnknownTransferId = 5
    | FileAlreadyExists = 6
    | NoSuchUser = 7

type ErrorCode =
    | NotDefined of string
    | FileNotFound of string
    | AccessViolation of string
    | DiskFull of string//or allocation exceeded
    | IllegalOperation of string
    | UnknownTransferId of string
    | FileAlreadyExists of string
    | NoSuchUser of string

(* 
Three modes of transfer are currently supported: netascii (This is
   ascii as defined in "USA Standard Code for Information Interchange"
   [1] with the modifications specified in "Telnet Protocol
   Specification" [3].)  Note that it is 8 bit ascii.  The term
   "netascii" will be used throughout this document to mean this
   particular version of ascii.); octet (This replaces the "binary" mode
   of previous versions of this document.) raw 8 bit bytes; mail,
   netascii characters sent to a user rather than a file.  (The mail
   mode is obsolete and should not be implemented or used.)  Additional
   modes can be defined by pairs of cooperating hosts.
*)
type TransferMode =
    | NetAscii
    | Octet

(*
Any transfer begins with a request to read or write a file, which
   also serves to request a connection.
*)
type Request =
    | Read
    | Write

(* 
If the server grants the
   request, the connection is opened and the file is sent in fixed
   length blocks of 512 bytes.

   A data packet of less than 512 bytes
   signals termination of a transfer.
*)
type DataBlock = ByteList of byte[]


(* 
Each data packet contains one block of
   data, and must be acknowledged by an acknowledgment packet before the
   next packet can be sent.

   If a packet gets lost in the
   network, the intended recipient will timeout and may retransmit his
   last packet (which may be data or an acknowledgment), thus causing
   the sender of the lost packet to retransmit that lost packet. 
*)
type Acknowledgement =
    | NotYet of DateTime
    | Yes
    | No

(*
An error is
   signalled by sending an error packet.

   This packet is not
   acknowledged, and not retransmitted

   Therefore timeouts are used to detect
   such a termination when the error packet has been lost.

   Errors are caused by three types of events: not being able to satisfy the
   request (e.g., file not found, access violation, or no such user),
   receiving a packet which cannot be explained by a delay or
   duplication in the network (e.g., an incorrectly formed packet), and
   losing access to a necessary resource (e.g., disk full or access
   denied during a transfer).
*)


(*
    2 bytes     string    1 byte     string   1 byte
    ------------------------------------------------
   | Opcode |  Filename  |   0  |    Mode    |   0  |
    ------------------------------------------------

                Figure 5-1: RRQ/WRQ packet
*)

type ReadPacket = {
    FileName: string;
    Mode: TransferMode
}

type WritePacket = {
    FileName: string;
    Mode: TransferMode
}

(*
    2 bytes     2 bytes      n bytes
    ----------------------------------
   | Opcode |   Block #  |   Data     |
    ----------------------------------

        Figure 5-2: DATA packet
*)

type DataPacket = {
    Block: int;
    Data: DataBlock;
}


(*
    2 bytes     2 bytes
    ---------------------
   | Opcode |   Block #  |
    ---------------------

    Figure 5-3: ACK packet
*)

type AckPacket = {
    Block: int;
}

(*
    2 bytes     2 bytes      string    1 byte
    -----------------------------------------
   | Opcode |  ErrorCode |   ErrMsg   |   0  |
    -----------------------------------------

            Figure 5-4: ERROR packet
*)

type ErrorPacket = {
    Error: ErrorCode
}

type File = {
    File: DataBlock[];
    Acknowledged: bool;
    }