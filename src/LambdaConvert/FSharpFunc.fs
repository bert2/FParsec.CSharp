namespace LambdaConvert

open System
open System.Runtime.CompilerServices

[<Extension>]
type public FSharpFunc = 

    [<Extension>] 
    static member ToFSharpFunc<'a> (f: Func<'a>) = fun () -> f.Invoke()

    [<Extension>] 
    static member ToFSharpFunc<'a,'b> (f: Func<'a,'b>) = fun x -> f.Invoke(x)

    static member From<'a> (f: Func<'a>) = FSharpFunc.ToFSharpFunc f

    static member From<'a,'b> (f: Func<'a,'b>) = FSharpFunc.ToFSharpFunc f
