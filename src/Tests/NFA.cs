namespace Tests {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class NFA {
        // Executing the NFA

        public static bool IsMatch(IState start, string text) => text
            .Aggregate(
                start.Expand(new HashSet<IState>()),
                (states, c) => states
                    .SelectMany(s => s.Consume(c))
                    .SelectMany(s => s.Expand(new HashSet<IState>())))
            .OfType<Final>()
            .Any();

        public static bool Matches(this IState start, string text) => IsMatch(start, text);

        // Monoid implemention

        public delegate IState ProtoState(IState exit);
        public static readonly ProtoState Zero = exit => exit;
        public static ProtoState Connect(ProtoState first, ProtoState second) => exit => first(second(exit));
        public static ProtoState Concat(IEnumerable<ProtoState> protos) => protos.Aggregate(Zero, Connect);

        // Control structures

        public static ProtoState MakeChar(char c) => exit => new CharS(c, exit);
        public static ProtoState MakeAnyChar() => exit => new AnyCharS(exit);
        public static ProtoState MakeCharRange(char min, char max) => exit => new CharRange(min, max, exit);
        public static ProtoState MakeAlternation(ProtoState left, ProtoState right) => Branch(left, right);
        public static ProtoState MakeZeroOrOne(ProtoState body) => Branch(body, Zero);
        public static ProtoState MakeZeroOrMore(ProtoState body) => exit => Loop(body, exit).loop;
        public static ProtoState MakeOneOrMore(ProtoState body) => exit => Loop(body, exit).body;

        // Generalizations

        public static ProtoState Branch(ProtoState left, ProtoState right) => exit => new Split(left(exit), right(exit));

        public static (IState body, Split loop) Loop(ProtoState body, IState exit) {
            IState enter = null;
            var loop = new Split(() => enter, exit);
            enter = body(loop);
            return (enter, loop);
        }
    }

    public interface IState {
        // Returns all states the can be reached from the current state by consuming the input character.
        IEnumerable<IState> Consume(char c);

        // Epsilon expansion: returns all states that can be reached from the current state without
        // consuming anything. Breaks transition loops by keeping track of already expanded states
        // using the set `visited`.
        IEnumerable<IState> Expand(HashSet<IState> visited);
    }

    public class CharS : IState {
        private readonly char c;
        private readonly IState next;
        public CharS(char c, IState next) { this.c = c; this.next = next; }
        public IEnumerable<IState> Consume(char c) { if (this.c == c) yield return next; }
        public IEnumerable<IState> Expand(HashSet<IState> visited) { yield return this; }
        public override string ToString() => $"Char('{c}')";
    }

    public class AnyCharS : IState {
        private readonly IState next;
        public AnyCharS(IState next) => this.next = next;
        public IEnumerable<IState> Consume(char c) { yield return next; }
        public IEnumerable<IState> Expand(HashSet<IState> visited) { yield return this; }
        public override string ToString() => "AnyChar";
    }

    public class CharRange : IState {
        private readonly char min;
        private readonly char max;
        private readonly IState next;
        public CharRange(char min, char max, IState next) { this.min = min; this.max = max; this.next = next; }
        public IEnumerable<IState> Consume(char c) { if (min <= c && c <= max) yield return next; }
        public IEnumerable<IState> Expand(HashSet<IState> visited) { yield return this; }
        public override string ToString() => $"CharRange('{min}'-'{max}')";
    }

    public class Split : IState {
        private readonly Func<IState> leftf;
        private readonly IState right;
        private IState left;
        public Split(IState left, IState right) { this.left = left; this.right = right; }
        public Split(Func<IState> leftf, IState right) { this.leftf = leftf; this.right = right; }
        public IEnumerable<IState> Consume(char c) { yield break; }
        public IEnumerable<IState> Expand(HashSet<IState> visited) => visited.Add(this) ? ExpandBoth(visited) : Enumerable.Empty<IState>();
        public override string ToString() => "Split";
        private IEnumerable<IState> ExpandBoth(HashSet<IState> visited) => GetLeft().Expand(visited).Concat(right.Expand(visited));
        private IState GetLeft() => left ?? (left = leftf());
    }

    public class Final : IState {
        public IEnumerable<IState> Consume(char c) { yield break; }
        public IEnumerable<IState> Expand(HashSet<IState> visited) { yield return this; }
        public override string ToString() => "Final";
    }
}
