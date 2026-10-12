---
name: rabbitmq-barebone-receiver
description: "Use when creating, scaffolding, or explaining a minimal RabbitMQ message receiver in this project, especially a .NET/C# consumer using RabbitMQ.Client."
---

# RabbitMQ barebone receiver

Create a small, runnable C# console receiver that connects to RabbitMQ, declares or uses a queue, consumes messages, and reports them. Keep it minimal and aligned with this repository.

## Procedure

1. Inspect the target project and its `.csproj`. Reuse its target framework, installed `RabbitMQ.Client` version, naming, and existing topology where appropriate. Check the sender project and related receiver examples before choosing exchange, routing key, or queue names.
2. If the user identifies a target project or file, make the receiver there. Otherwise ask which project to use if more than one plausible target exists; do not overwrite an existing sender or receiver without being asked.
3. Use the async APIs supported by the project's RabbitMQ.Client version. For this repository's RabbitMQ.Client 7.x projects, use `ConnectionFactory.CreateConnectionAsync`, `IConnection.CreateChannelAsync`, and `AsyncEventingBasicConsumer.ReceivedAsync`.
4. Read the broker URI from existing project configuration or an environment variable, not a hard-coded credential. If no connection configuration exists, use a clearly named placeholder/environment variable and explain how to set it. Never copy secrets from other files into source code.
5. Declare/bind the exchange and queue only when needed for the chosen topology. Match existing topology when the receiver is intended to work with this project's sender; explain any names or declaration options that must agree.
6. For a minimal receiver, set prefetch to 1 and consume with manual acknowledgements. Convert `ea.Body` to an array before decoding or retaining it, decode text as UTF-8, and print the received message. Acknowledge only after successful handling. Do not block an async callback with `.Wait()` or `.Result`.
7. Keep the receiver running until cancellation, then cancel the consumer and asynchronously dispose/close the consumer resources using the project's existing patterns. Avoid adding retry, dead-letter, persistence, or concurrency policies unless requested; ask about those when the requested behavior depends on them.
8. Build the target project and resolve errors introduced by the change. Report the file changed, how to configure the broker URI, how to run the receiver, and any broker setup needed.

## Expected minimal behavior

- Open an async RabbitMQ connection and channel.
- Consume from the selected queue with manual acknowledgement.
- Print each UTF-8 message and acknowledge it after successful processing.
- Stop cleanly on cancellation.
- Keep broker credentials out of source control.

## Repository notes

- Existing console projects target `net10.0` and reference `RabbitMQ.Client` 7.2.2; confirm the actual target project before relying on those versions.
- Existing examples use a direct exchange with `test_exchange`, `test_routing_key`, and `test_queue`. Reuse these only when the receiver is meant to pair with the existing sender; otherwise use the topology the user specifies.
