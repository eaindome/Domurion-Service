using System.Threading.Channels;

namespace Domurion.Helpers
{
    public interface IEmailQueue
    {
        ValueTask EnqueueAsync(EmailMessage message, CancellationToken cancellationToken = default);
        IAsyncEnumerable<EmailMessage> DequeueAllAsync(CancellationToken cancellationToken = default);
    }

    public class InMemoryEmailQueue : IEmailQueue, IDisposable
    {
        private readonly Channel<EmailMessage> _channel = Channel.CreateUnbounded<EmailMessage>();

        public ValueTask EnqueueAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            if (!_channel.Writer.TryWrite(message))
                return _channel.Writer.WriteAsync(message, cancellationToken);
            return ValueTask.CompletedTask;
        }

        public async IAsyncEnumerable<EmailMessage> DequeueAllAsync([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            while (await _channel.Reader.WaitToReadAsync(cancellationToken))
            {
                while (_channel.Reader.TryRead(out var msg))
                {
                    yield return msg;
                }
            }
        }

        public void Dispose()
        {
            _channel.Writer.TryComplete();
        }
    }
}
