# Kafka Offset Manager

[![Build status](https://img.shields.io/github/workflow/status/tautvydasversockas/kafka-offset-manager/publish-nuget)](https://github.com/tautvydasversockas/kafka-offset-manager/actions/workflows/pipeline.yml)
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
using Kafka.OffsetManagement;

// `maxOutstanding` specifies the maximum number of offsets that
// can be kept unacknowledged at the same time.
using var offsetManager = new OffsetManager(maxOutstanding: 10000);

// Call `GetAckIdAsync` to get an acknowledgement ID that later can be
// used to acknowledge successfully processed message offsets.
var ackId = await offsetManager.GetAckIdAsync(offset: 5);

// Process messages in parallel and call `Ack` to acknowledge 
// successfully processed message offset.
offsetManager.Ack(ackId);

// Call `MarkAsAcked` to mark message offset as sucessfully processed.
// `MarkAsAcked` can only be used to acknowledge offsets in sequential manner -
// offset 5 can only be marked as acknowledged before marking offset 6 as acknowledged 
// or getting acknowledgement (`GetAckIdAsync`) ID for offset 6.
// `MarkAsAcked` is usually used to mark message as acknowledged without 
// further processing it.
offsetManager.MarkAsAcked(offset: 6);

// Call `GetCommitOffset` to get the offset that can be safely committed.
// Safely commitable offset = last sequentialy processed offset + 1. 
// If offsets 3, 4, 7 are acknowledged, `GetCommitOffset` will return 5.
// Only when offsets 5 and 6 are acknowledged `GetCommitOffset` will return 8.
// `GetCommitOffset` is usually called from a separate from message processing thread.
var commitableOffset = offsetManager.GetCommitOffset();
```

## Support

<a href="https://www.buymeacoffee.com/tautvydasverso"> 
    <img align="left" src="https://cdn.buymeacoffee.com/buttons/v2/default-yellow.png" height="50" width="210"  alt="tautvydasverso" />
</a>
