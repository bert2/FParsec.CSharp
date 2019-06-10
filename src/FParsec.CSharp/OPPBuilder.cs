using System;
using System.Linq;
using Microsoft.FSharp.Core;
using static FParsec.CSharp.PrimitivesCS;

namespace FParsec.CSharp {
    /// <summary>A helper to build `OperatorPrecedenceParser`s.</summary>
    public class OPPBuilder<TUserState, TTerm, TAfterString> {
        // Split the OPP into two parts in order to deal with implicit operators
        // (see https://stackoverflow.com/questions/29322892).
        // `highOpp` will only be used in case `WithImplicitOperator()` actually was called.
        private readonly OperatorPrecedenceParser<TTerm, TAfterString, TUserState> lowOpp = new OperatorPrecedenceParser<TTerm, TAfterString, TUserState>();
        private readonly OperatorPrecedenceParser<TTerm, TAfterString, TUserState> highOpp = new OperatorPrecedenceParser<TTerm, TAfterString, TUserState>();

        private readonly Operators<TUserState, TTerm, TAfterString> operators = new Operators<TUserState, TTerm, TAfterString>();

        private (int prec, Func<TTerm, TTerm, TTerm> map) implicitOp;

        private FSharpFunc<CharStream<TUserState>, Reply<TTerm>> termParser;

        /// <summary>Adds and implicit operator to the OPP.</summary>
        public OPPBuilder<TUserState, TTerm, TAfterString> WithImplicitOperator(int precedence, Func<TTerm, TTerm, TTerm> map) {
            if (precedence < 1) throw new ArgumentOutOfRangeException(nameof(precedence), "The operator precedence must be greater than 0.");
            implicitOp = (precedence, map);
            return this;
        }

        /// <summary>Configures the operators of the OPP to build.</summary>
        public OPPBuilder<TUserState, TTerm, TAfterString> WithOperators(Action<Operators<TUserState, TTerm, TAfterString>> configure) {
            configure(operators);
            return this;
        }

        /// <summary>Sets the term parser of the OPP for non-recursive grammars.</summary>
        public OPPBuilder<TUserState, TTerm, TAfterString> WithTerms(FSharpFunc<CharStream<TUserState>, Reply<TTerm>> p) {
            termParser = p;
            return this;
        }

        /// <summary>Sets the term parser of the OPP for recursive grammars.</summary>
        public OPPBuilder<TUserState, TTerm, TAfterString> WithTerms(
            Func<FSharpFunc<CharStream<TUserState>, Reply<TTerm>>, FSharpFunc<CharStream<TUserState>, Reply<TTerm>>> p) {
            termParser = p(lowOpp.ExpressionParser);
            return this;
        }

        /// <summary>Builds the OPP from the builder configuration.</summary>
        public OperatorPrecedenceParser<TTerm, TAfterString, TUserState> Build() {
            highOpp.TermParser = termParser;
            lowOpp.TermParser = HasImplicitOperator
                ? Many1(highOpp.ExpressionParser).Map(xs => xs.Aggregate(implicitOp.map))
                : termParser;

            foreach (var op in operators)
                SelectOpp(op).AddOperator(op);

            return lowOpp;
        }

        /// <summary>
        /// Indicates whether an implicit operator was configured on the builder.
        /// </summary>
        public bool HasImplicitOperator => implicitOp != default;

        private OperatorPrecedenceParser<TTerm, TAfterString, TUserState> SelectOpp(Operator<TTerm, TAfterString, TUserState> op)
            => HasImplicitOperator && op.Precedence > implicitOp.prec ? highOpp : lowOpp;
    }
}
