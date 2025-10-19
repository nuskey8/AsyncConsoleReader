using AsyncConsoleReader;

var cts = new CancellationTokenSource();
// cts.CancelAfter(500);

try
{
    while (true)
    {
        var line = AsyncConsole.ReadLine(cts.Token);
        Console.WriteLine(line);
        _ = Console.CursorTop;
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}