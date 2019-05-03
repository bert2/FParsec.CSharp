namespace Tests {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class NFA {
        public delegate IState Proto(IState exit);

        public static bool Matches(this IState start, string text) => IsMatch(start, text);

        public static bool IsMatch(IState start, string text) => text
            .Aggregate(
                start.Expand(),
                (states, c) => states
                    .SelectMany(s => s.Consume(c))
                    .SelectMany(s => s.Expand()))
            .OfType<Final>()
            .Any();

        public static Proto Zero = exit => exit;
        public static Proto Connect(Proto first, Proto second) => exit => first(second(exit));
        public static Proto Concat(IEnumerable<Proto> protos) => exit => protos.Aggregate(Zero, Connect)(exit);

        public static Proto MakeChar(char c) => exit => new CharState(c, exit);
        public static Proto MakeAnyChar() => exit => new AnyCharState(exit);
        public static Proto MakeCharRange(char min, char max) => exit => new CharRange(min, max, exit);

        public static Proto MakeZeroOrMore(Proto body) => exit => {
            IState enter = null;
            IState split = new Split(() => enter, exit);
            enter = body(split);
            return split;
        };
    }

    public interface IState {
        IEnumerable<IState> Consume(char c);
        IEnumerable<IState> Expand();
    }

    public class CharState : IState {
        private readonly char c;
        private readonly IState next;
        public CharState(char c, IState next) { this.c = c; this.next = next; }
        public IEnumerable<IState> Consume(char c) { if (this.c == c) yield return next; }
        public IEnumerable<IState> Expand() { yield return this; }
        public override string ToString() => $"Char('{c}')";
    }

    public class AnyCharState : IState {
        private readonly IState next;
        public AnyCharState(IState next) => this.next = next;
        public IEnumerable<IState> Consume(char c) { yield return next; }
        public IEnumerable<IState> Expand() { yield return this; }
        public override string ToString() => "AnyChar";
    }

    public class CharRange : IState {
        private readonly char min;
        private readonly char max;
        private readonly IState next;
        public CharRange(char min, char max, IState next) { this.min = min; this.max = max; this.next = next; }
        public IEnumerable<IState> Consume(char c) { if (min <= c && c <= max) yield return next; }
        public IEnumerable<IState> Expand() { yield return this; }
        public override string ToString() => $"CharRange('{min}-{max}')";
    }

    public class Split : IState {
        private readonly Func<IState> leftf;
        private readonly IState left;
        private readonly IState right;
        public Split(IState left, IState right) { this.left = left; this.right = right; }
        public Split(Func<IState> leftf, IState right) { this.leftf = leftf; this.right = right; }
        public IEnumerable<IState> Consume(char c) { yield break; }
        public IEnumerable<IState> Expand() => (left ?? leftf()).Expand().Concat(right.Expand());
        public override string ToString() => "Split";
    }

    public class Final : IState {
        public IEnumerable<IState> Consume(char c) { yield break; }
        public IEnumerable<IState> Expand() { yield return this; }
        public override string ToString() => "Final";
    }
}
