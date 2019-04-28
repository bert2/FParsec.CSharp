string FormatReleaseNotes(string text) {
    if (string.IsNullOrEmpty(text)) throw new Exception("Release notes are empty.");

    var withUpperFirst = text.Skip(1).Prepend(char.ToUpper(text.First()));
    var withPeriod = text.Last() == '.' ? withUpperFirst : withUpperFirst.Append('.');

    return string.Concat(withPeriod);
}
