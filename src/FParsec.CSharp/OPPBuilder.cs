namespace FParsec.CSharp {
    using System;
    using System.Linq;
    using FParsec;
    using Microsoft.FSharp.Core;
    using static FParsec.CSharp.PrimitivesCS;

    public class OPPBuilder<TTerm, TAfterString> {

        // Split the OPP into two parts in order to deal with implicit operators
        // (see https://stackoverflow.com/questions/29322892).
        // `highOpp` will only be used in case `WithImplicitOperator()` actually was called.
        private readonly OperatorPrecedenceParser<TTerm, TAfterString, Unit> lowOpp = new OperatorPrecedenceParser<TTerm, TAfterString, Unit>();
        private readonly OperatorPrecedenceParser<TTerm, TAfterString, Unit> highOpp = new OperatorPrecedenceParser<TTerm, TAfterString, Unit>();

        private readonly Operators<TTerm, TAfterString> operators = new Operators<TTerm, TAfterString>();

        private (int prec, Func<TTerm, TTerm, TTerm> map) implicitOp;

        private FSharpFunc<CharStream<Unit>, Reply<TTerm>> termParser;

        public OPPBuilder<TTerm, TAfterString> WithImplicitOperator(int precedence, Func<TTerm, TTerm, TTerm> map) {
            if (precedence < 1) throw new ArgumentOutOfRangeException(nameof(precedence), "The operator precedence must be greater than 0.");
            implicitOp = (precedence, map);
            return this;
        }

        public OPPBuilder<TTerm, TAfterString> WithOperators(Action<Operators<TTerm, TAfterString>> configure) {
            configure(operators);
            return this;
        }

        public OPPBuilder<TTerm, TAfterString> WithTerms(FSharpFunc<CharStream<Unit>, Reply<TTerm>> p) {
            termParser = p;
            return this;
        }

        public OPPBuilder<TTerm, TAfterString> WithTerms(
            Func<FSharpFunc<CharStream<Unit>, Reply<TTerm>>, FSharpFunc<CharStream<Unit>, Reply<TTerm>>> p) {
            termParser = p(lowOpp.ExpressionParser);
            return this;
        }

        public OperatorPrecedenceParser<TTerm, TAfterString, Unit> Build() {
            highOpp.TermParser = termParser;
            lowOpp.TermParser = HasImplicitOperator
                ? Many1(highOpp.ExpressionParser).Map(xs => xs.Aggregate(implicitOp.map))
                : termParser;

            foreach (var op in operators)
                SelectOpp(op).AddOperator(op);

            return lowOpp;
        }

        public bool HasImplicitOperator => implicitOp != default;

        private OperatorPrecedenceParser<TTerm, TAfterString, Unit> SelectOpp(Operator<TTerm, TAfterString, Unit> op)
            => HasImplicitOperator && op.Precedence > implicitOp.prec ? highOpp : lowOpp;
    }
}
