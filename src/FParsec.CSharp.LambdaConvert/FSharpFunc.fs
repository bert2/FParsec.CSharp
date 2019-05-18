namespace FParsec.CSharp

open System
open System.Runtime.CompilerServices

[<Extension>]
type public FSharpFunc = 

    [<Extension>] 
    static member ToFSharpFunc<'a> (f: Func<'a>) = fun () -> f.Invoke()

    [<Extension>] 
    static member ToFSharpFunc<'a,'b> (f: Func<'a,'b>) = fun x -> f.Invoke(x)

    [<Extension>] 
    static member ToFSharpFunc<'a,'b,'c> (f: Func<'a,'b,'c>) = fun x y -> f.Invoke(x, y)

    [<Extension>] 
    static member ToFSharpFunc<'a,'b,'c,'d> (f: Func<'a,'b,'c,'d>) = fun x y z -> f.Invoke(x, y, z)

    [<Extension>] 
    static member ToFSharpFunc<'a,'b,'c,'d,'e> (f: Func<'a,'b,'c,'d,'e>) = fun w x y z -> f.Invoke(w, x, y, z)

    [<Extension>] 
    static member ToFSharpFunc<'a,'b,'c,'d,'e,'f> (f: Func<'a,'b,'c,'d,'e,'f>) = fun v w x y z -> f.Invoke(v, w, x, y, z)

    static member From<'a> (f: Func<'a>) = FSharpFunc.ToFSharpFunc f

    static member From<'a,'b> (f: Func<'a,'b>) = FSharpFunc.ToFSharpFunc f

    static member From<'a,'b,'c> (f: Func<'a,'b,'c>) = FSharpFunc.ToFSharpFunc f

    static member From<'a,'b,'c,'d> (f: Func<'a,'b,'c,'d>) = FSharpFunc.ToFSharpFunc f

    static member From<'a,'b,'c,'d,'e> (f: Func<'a,'b,'c,'d,'e>) = FSharpFunc.ToFSharpFunc f

    static member From<'a,'b,'c,'d,'e,'f> (f: Func<'a,'b,'c,'d,'e,'f>) = FSharpFunc.ToFSharpFunc f
