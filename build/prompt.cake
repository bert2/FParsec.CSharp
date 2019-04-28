using _Task = System.Threading.Tasks.Task;

string Prompt(string message, int timeoutSeconds = 60) {
    Warning(message);
    Console.Write("> ");

    string response = null;

    _Task.WhenAny(
        _Task.Run(() => response = Console.ReadLine()),
        _Task.Delay(TimeSpan.FromSeconds(timeoutSeconds))
    ).Wait();

    if (response == null)
        throw new Exception($"User prompt timed out.");

    return response;
}
