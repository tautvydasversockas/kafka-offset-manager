# Kafka Offset Manager

[![Build status](https://img.shields.io/github/workflow/status/tautvydasversockas/kafka-offset-manager/.NET)](https://github.com/tautvydasversockas/kafka-offset-manager/actions/workflows/pipeline.yml)
[![NuGet downloads](https://img.shields.io/nuget/v/kafka.offsetmanager.svg)](https://www.nuget.org/packages/Kafka.OffsetManager/)
[![GitHub license](https://img.shields.io/github/license/mashape/apistatus.svg)](https://github.com/tautvydasversockas/kafka-offset-manager/blob/main/LICENSE)

This library provides a thread-safe Kafka in-memory offset manager implementation. It helps to track out-of-order offsets enabling multiple thread usage per single topic partition. 

## Installation

Available on [NuGet](https://www.nuget.org/packages/Kafka.OffsetManager/)

```bash
dotnet add package Kafka.OffsetManager
```

```powershell
PM> Install-Package Kafka.OffsetManager
```

## Usage

```csharp
using Kafka.OffsetManager;
using System.Threading.Tasks;

class Program
{
    public static async Task Main(string[] args)
    {
        // `maxOutstanding` specifies the maximum number of offsets that
        // can be kept unacknowledged.
        using var offsetManager = new OffsetManager(maxOutstanding: 10000);

        var offset = 5;

        // `GetAckIdAsync` returns an acknowledgement ID that later can be
        // used to acknowledge successfully processed offsets.
        // In case `OffsetManager` has `maxOutstanding` unacknowledged offsets,
        // `GetAckIdAsync` waits and only returns when at least one of the 
        // unacknowledged offsets is acknowledged.
        var ackId = await offsetManager.GetAckIdAsync(offset);

        // Process messages in parallel.

        // `Ack` acknowledges successfully processed offset.
        offsetManager.Ack(ackId);

        // `GetCommitOffset` returns an offset that can be safely committed.
        // If offsets 3, 4, 7 are acknowledged, `GetCommitOffset` will return 4.
        // Only when offsets 5 and 6 are acknowledged `GetCommitOffset` will return 7.
        // `GetCommitOffset` is usually periodically called on a separate
        // from message processing thread.
        var commitableOffset = offsetManager.GetCommitOffset();
    }
}
```
