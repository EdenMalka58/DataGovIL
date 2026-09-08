using System.Threading.Channels;

namespace DataGovIL.Api.Services;

public interface IManufacturerExportWorkQueue
{
    bool TryEnqueue();

    ValueTask WaitForWorkAsync(CancellationToken ct);
}

/// <summary>
/// Single-consumer channel used to hand export requests from HTTP to the hosted worker
/// without tying the work to a request CancellationToken.
/// </summary>
public sealed class ManufacturerExportWorkQueue : IManufacturerExportWorkQueue
{
    private readonly Channel<bool> _channel = Channel.CreateUnbounded<bool>(
        new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

    public bool TryEnqueue() => _channel.Writer.TryWrite(true);

    public async ValueTask WaitForWorkAsync(CancellationToken ct)
    {
        var reader = _channel.Reader;
        await reader.WaitToReadAsync(ct).ConfigureAwait(false);
        reader.TryRead(out _);
    }
}
