# AsyncConsoleReader
Provides a cancelable, non-blocking alternative to `Console.Read()`/`ReadKey()`/`ReadLine()`.

[![NuGet](https://img.shields.io/nuget/v/AsyncConsoleReader.svg)](https://www.nuget.org/packages/AsyncConsoleReader)
[![Releases](https://img.shields.io/github/release/nuskey8/AsyncConsoleReader.svg)](https://github.com/nuskey8/AsyncConsoleReader/releases)
[![license](https://img.shields.io/badge/LICENSE-MIT-green.svg)](LICENSE)

[English](./README.md) | 日本語

## 概要

AsyncConsoleReaderは`CancellationToken`に対応したNon-blocingな標準入力の読み取り機能を提供するライブラリです。

C#では標準入力の読み取りに`Console.ReadLine()`などが用いられますが、これは読み取りが完了するまでプロセスをブロックします。また、`CancellationToken`などを利用したキャンセルが不可能なため、`Task.Run`でラップしても入力が終わるまでスレッドプールのスレッドを占有してしまいます。

AsyncConsoleReaderは`Console.ReadKey()`を用いて`Read/ReadLine`を再実装することにより、キャンセルに対応した実装を提供します。

また、スレッドプール上で効率的にReadを実行するためのasync APIを用意しています。

> [!NOTE]
> AsyncConsoleReaderのasync APIはあくまでバックグランドスレッドで効率的に`Read()`を実行するためのヘルパーであり、読み取り自体が非同期に行われるわけではありません。

## インストール

### NuGet packages

AsyncConsoleReaderを利用するには.NET 8.0以上が必要です。パッケージはNuGetから入手できます。

#### .NET CLI

```
dotnet add package AsyncConsoleReader
```

#### Package Manager

```
Install-Package AsyncConsoleReader
```

## 使い方

`AsyncConsole`を用いて`Read/ReadKey/ReadLine`を呼び出すことができます。これらは`System.Console`と同等のAPIを持ちますが、`CancellationToken`を渡してキャンセル処理を行うことが可能です。

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

また、非同期APIも利用できます。

```cs
var line = await AsyncConsole.ReadKeyAsync(false, cts.Token);
```

## ライセンス

このライブラリは[MITライセンス](LICENSE)の下で提供されています。
