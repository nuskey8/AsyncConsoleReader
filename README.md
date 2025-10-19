# AsyncConsoleReader
Provides a cancelable, non-blocking alternative to `Console.Read()`/`ReadKey()`/`ReadLine()`.

[![NuGet](https://img.shields.io/nuget/v/AsyncConsoleReader.svg)](https://www.nuget.org/packages/AsyncConsoleReader)
[![Releases](https://img.shields.io/github/release/nuskey8/AsyncConsoleReader.svg)](https://github.com/nuskey8/AsyncConsoleReader/releases)
[![license](https://img.shields.io/badge/LICENSE-MIT-green.svg)](LICENSE)

English | [日本語](./README_JA.md)

## Overview

AsyncConsoleReader is a library that provides non-blocking standard input reading functionality with `CancellationToken` support.

In C#, `Console.ReadLine()` is commonly used for reading standard input, but it blocks the process until the reading is complete. Additionally, it cannot be canceled using mechanisms like `CancellationToken`. Even if wrapped in `Task.Run`, it occupies a thread from the thread pool until the input is finished.

AsyncConsoleReader reimplements `Read/ReadLine` natively to provide an implementation that supports cancellation. It currently supports macOS, Linux, and Windows.

It also provides async APIs for efficiently performing reads on the thread pool.

> [!NOTE]
> The async APIs of AsyncConsoleReader are helpers for efficiently executing `Read()` on background threads. The reading itself is not performed asynchronously.

## Installation

### NuGet packages

To use AsyncConsoleReader, .NET 8.0 or higher is required. Packages can be obtained from NuGet.

#### .NET CLI

```
dotnet add package AsyncConsoleReader
```

#### Package Manager

```
Install-Package AsyncConsoleReader
```

## Usage

You can call `Read/ReadKey/ReadLine` using `AsyncConsole`. These APIs are equivalent to `System.Console` but allow cancellation by passing a `CancellationToken`.

```cs
using AsyncConsoleReader;

var cts = new CancellationTokenSource();
cts.CancelAfter(500);

try
{
    var line = AsyncConsole.ReadLine(cts.Token);
    Console.WriteLine(line);
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}
```

You can also use the asynchronous API.

```cs
var line = await AsyncConsole.ReadKeyAsync(false, cts.Token);
```

## License

This library is provided under the [MIT License](LICENSE).