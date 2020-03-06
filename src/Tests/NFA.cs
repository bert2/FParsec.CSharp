using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests {
    public static class NFA {
        // NFA execution

        public static bool IsMatch(IState start, string text) => text
            .Aggregate(
                start.Expand(new HashSet<IState>()),
                (states, c) => states
                    .SelectMany(s => s.Consume(c))
                    .SelectMany(s => s.Expand(new HashSet<IState>())))
            .OfType<Final>()
            .Any();

        public static bool Matches(this IState start, string text) => IsMatch(start, text);

        // Monoid implemention for lazy construction

        public delegate IState ProtoState(IState exit);
        public static readonly ProtoState Zero = exit => exit;
        public static ProtoState Connect(ProtoState first, ProtoState second) => exit => first(second(exit));
        public static ProtoState Concat(IEnumerable<ProtoState> protos) => protos.Aggregate(Zero, Connect);

        // NFA construction

        public static ProtoState Accept(ILetter letter) => exit => new State(letter, exit);
        public static ProtoState Branch(ProtoState left, ProtoState right) => exit => new Split(left(exit), right(exit));

        public static ProtoState Loop(ProtoState body, bool atLeastOnce = false) => exit => {
            IState? _body = null;
            var loop = new Split(() => _body, exit);
            _body = body(loop);
            return atLeastOnce ? _body : loop;
        };

        public static ProtoState MakeChar(char c) => Accept(new CharL(c));
        public static ProtoState MakeAnyChar() => Accept(new AnyCharL());
        public static ProtoState MakeCharRange(char min, char max) => Accept(new CharRangeL(min, max));
        public static ProtoState MakeAlternation(ProtoState left, ProtoState right) => Branch(left, right);
        public static ProtoState MakeZeroOrOne(ProtoState body) => Branch(body, Zero);
        public static ProtoState MakeZeroOrMore(ProtoState body) => Loop(body);
        public static ProtoState MakeOneOrMore(ProtoState body) => Loop(body, atLeastOnce: true);
    }

    public interface IState {
        // Returns all states the can be reached from the current state by consuming the input character.
        IEnumerable<IState> Consume(char c);

        // Epsilon expansion: returns all states that can be reached from the current state without
        // consuming anything. Breaks transition loops by keeping track of already expanded states
        // using the set `visited`.
        IEnumerable<IState> Expand(HashSet<IState> visited);
    }

    public class State : IState {
        private readonly ILetter letter;
        private readonly IState next;
        public State(ILetter letter, IState next) { this.letter = letter; this.next = next; }
        public IEnumerable<IState> Consume(char c) { if (letter.Matches(c)) yield return next; }
        public IEnumerable<IState> Expand(HashSet<IState> visited) { yield return this; }
        public override string? ToString() => letter.ToString();
    }

    public class Split : IState {
        private IState? left;
        private readonly Func<IState?>? leftf;
        private readonly IState right;
        public Split(IState left, IState right) { this.left = left; this.right = right; }
        public Split(Func<IState?> leftf, IState right) { this.leftf = leftf; this.right = right; }
        public IEnumerable<IState> Consume(char c) { yield break; }
        public IEnumerable<IState> Expand(HashSet<IState> visited) => visited.Add(this) ? ExpandBoth(visited) : Enumerable.Empty<IState>();
        public override string ToString() => "Split";
        private IEnumerable<IState> ExpandBoth(HashSet<IState> visited) => GetLeft().Expand(visited).Concat(right.Expand(visited));
        private IState GetLeft() => left ?? (left = leftf?.Invoke()) ?? throw new InvalidOperationException("Uninitialized left branch of split");
    }

    public class Final : IState {
        public IEnumerable<IState> Consume(char c) { yield break; }
        public IEnumerable<IState> Expand(HashSet<IState> visited) { yield return this; }
        public override string ToString() => "Final";
    }

    public interface ILetter { bool Matches(char c); }

    public class CharL : ILetter {
        private readonly char c;
        public CharL(char c) => this.c = c;
        public bool Matches(char c) => c == this.c;
        public override string ToString() => $"Char('{c}')";
    }

    public class AnyCharL : ILetter {
        public bool Matches(char c) => true;
        public override string ToString() => "AnyChar";
    }

    public class CharRangeL : ILetter {
        private readonly char min;
        private readonly char max;
        public CharRangeL(char min, char max) { this.min = min; this.max = max; }
        public bool Matches(char c) => min <= c && c <= max;
        public override string ToString() => $"CharRange('{min}'-'{max}')";
    }
}
