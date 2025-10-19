using AsyncConsoleReader;

var cts = new CancellationTokenSource();
cts.CancelAfter(500);

try
{
    var line = await AsyncConsole.ReadLineAsync(cts.Token);
    Console.WriteLine(line);
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}