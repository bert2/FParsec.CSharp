namespace FParsec.CSharp {
    using System;
    using FParsec;
    using Microsoft.FSharp.Core;

    public class OPPBuilder<TTerm, TAfterString> {
        private readonly OperatorPrecedenceParser<TTerm, TAfterString, Unit> opp = new OperatorPrecedenceParser<TTerm, TAfterString, Unit>();

        private readonly Operators<TTerm, TAfterString> operators = new Operators<TTerm, TAfterString>();

        private FSharpFunc<CharStream<Unit>, Reply<TTerm>> termParser;

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
            termParser = p(opp.ExpressionParser);
            return this;
        }

        public OperatorPrecedenceParser<TTerm, TAfterString, Unit> Build() {
            opp.TermParser = termParser;

            foreach (var op in operators)
                opp.AddOperator(op);

            return opp;
        }
    }
}
