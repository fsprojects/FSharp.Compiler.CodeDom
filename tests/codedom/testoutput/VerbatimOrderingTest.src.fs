﻿
//------------------------------------------------------------------------------
// <autogenerated>
//     This code was generated by a tool.
//     Runtime Version: 4.0.30319.18444
//
//     Changes to this file may cause incorrect behavior and will be lost if 
//     the code is regenerated.
// </autogenerated>
//------------------------------------------------------------------------------

namespace global

namespace Namespace1
    // Generated by F# CodeDom
    #nowarn "49" // uppercase argument names
    #nowarn "67" // this type test or downcast will always hold
    #nowarn "66" // this upcast is unnecessary - the types are identical
    #nowarn "58" // possible incorrect indentation..
    #nowarn "57" // do not use create_DelegateEvent
    #nowarn "51" // address-of operator can occur in the code
    #nowarn "1183" // unused 'this' reference
    
    exception ReturnExceptionb1885ddf389a4dd4a56de0c14022104a of obj
    exception ReturnNoneExceptionb1885ddf389a4dd4a56de0c14022104a
    [<AutoOpen>]
    module FuncConvertFinalOverloadb1885ddf389a4dd4a56de0c14022104a =
      // This extension member adds to the FuncConvert type and is the last resort member in the method overloading rules. 
      type global.Microsoft.FSharp.Core.FuncConvert with
          /// A utility function to convert function values from tupled to curried form
          static member FuncFromTupled (f:'T -> 'Res) = f
    
    type
        // Outer Type Comment
        
        Class1 = class
            [<Microsoft.FSharp.Core.DefaultValueAttribute(false)>]
            val mutable _event_Event1 : IDelegateEvent<System.EventHandler>;
            [<Microsoft.FSharp.Core.DefaultValueAttribute(false)>]
            val mutable _invoke_Event1 : obj[] -> unit;
            
            [<Microsoft.FSharp.Core.DefaultValueAttribute(false)>]
            val mutable _event_Event2 : IDelegateEvent<System.EventHandler>;
            [<Microsoft.FSharp.Core.DefaultValueAttribute(false)>]
            val mutable _invoke_Event2 : obj[] -> unit;
            // Field 1 Comment
            [<Microsoft.FSharp.Core.DefaultValueAttribute(false)>]
            val mutable field1:string
            
            [<Microsoft.FSharp.Core.DefaultValueAttribute(false)>]
            val mutable field2:string
            [<CLIEvent>]
            member this.Event1 =
                this._event_Event1
            
            [<CLIEvent>]
            member this.Event2 =
                this._event_Event2
            new() as this =
                {
                } then
                        let t_event_Event2 = new DelegateEvent<System.EventHandler>();
                        this._event_Event2 <- t_event_Event2.Publish;
                        this._invoke_Event2 <- t_event_Event2.Trigger;
                        let t_event_Event1 = new DelegateEvent<System.EventHandler>();
                        this._event_Event1 <- t_event_Event1.Publish;
                        this._invoke_Event1 <- t_event_Event1.Trigger;
                        begin
                            this.field1 <- "value1"
                            this.field2 <- "value2"
                        end
            
            new(value1:string, value2:string) as this =
                {
                } then
                        let t_event_Event2 = new DelegateEvent<System.EventHandler>();
                        this._event_Event2 <- t_event_Event2.Publish;
                        this._invoke_Event2 <- t_event_Event2.Trigger;
                        let t_event_Event1 = new DelegateEvent<System.EventHandler>();
                        this._event_Event1 <- t_event_Event1.Publish;
                        this._invoke_Event1 <- t_event_Event1.Trigger;
            
            abstract Property1 : string with get
            default this.Property1
                with get() : string =
                    this.field1
            
            
            abstract Property2 : string with get
            default this.Property2
                with get() : string =
                    this.field2
            // Method 2 Comment
            // Method 2 Comment
            abstract Method2 : unit -> unit
            default this.Method2  () =
                this._invoke_Event2 [|  box (this);  box (System.EventArgs.Empty) |] |> ignore
            
            abstract Method1 : unit -> unit
            default this.Method1  () =
                this._invoke_Event1 [|  box (this);  box (System.EventArgs.Empty) |] |> ignore
        end