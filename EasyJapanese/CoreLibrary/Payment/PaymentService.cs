using Microsoft.Extensions.Options;
using PayOS;
using PayOS.Models;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;

namespace CoreLibrary.Payment
{
    public class PaymentService : IPaymentService
    {
        private readonly PayOSClient _client;

        public PaymentService(PayOSClient client, IOptions<PayOSOptions> options)
        {
            // options is bound at registration time; it is held here so consumers
            // that want to read the configured ChecksumKey for offline work
            // (e.g. custom verification) can do so via DI.
            _ = options;
            _client = client;
        }

        public Task<CreatePaymentLinkResponse> CreatePaymentLinkAsync(
            CreatePaymentLinkRequest request,
            CancellationToken cancellationToken = default)
        {
            var opts = new RequestOptions<CreatePaymentLinkRequest>
            {
                Body = request,
                CancellationToken = cancellationToken
            };
            return _client.PaymentRequests.CreateAsync(request, opts);
        }

        public Task<PaymentLink> GetPaymentLinkAsync(
            long orderCode,
            CancellationToken cancellationToken = default)
        {
            var opts = new RequestOptions { CancellationToken = cancellationToken };
            return _client.PaymentRequests.GetAsync(orderCode, opts);
        }

        public Task<PaymentLink> CancelPaymentLinkAsync(
            long orderCode,
            string? cancellationReason = null,
            CancellationToken cancellationToken = default)
        {
            var opts = new RequestOptions<CancelPaymentLinkRequest>
            {
                Body = new CancelPaymentLinkRequest
                {
                    CancellationReason = cancellationReason
                },
                CancellationToken = cancellationToken
            };
            return _client.PaymentRequests.CancelAsync(orderCode, cancellationReason, opts);
        }

        public Task<WebhookData> VerifyWebhookAsync(
            Webhook webhook,
            CancellationToken cancellationToken = default)
        {
            return _client.Webhooks.VerifyAsync(webhook);
        }

        public Task<ConfirmWebhookResponse> ConfirmWebhookAsync(
            string webhookUrl,
            CancellationToken cancellationToken = default)
        {
            var opts = new RequestOptions<ConfirmWebhookRequest>
            {
                Body = new ConfirmWebhookRequest { WebhookUrl = webhookUrl },
                CancellationToken = cancellationToken
            };
            return _client.Webhooks.ConfirmAsync(webhookUrl, opts);
        }
    }
}
