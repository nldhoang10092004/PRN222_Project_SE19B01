using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;

namespace CoreLibrary.Payment
{
    /// <summary>
    /// Thin wrapper over the PayOS SDK. Re-exposes the operations EasyJapanese
    /// needs (create / get / cancel payment link, confirm &amp; verify webhook)
    /// so call sites do not depend on the SDK's resource types directly.
    /// </summary>
    public interface IPaymentService
    {
        /// <summary>
        /// Create a new payment link and return the PayOS checkout URL / QR code.
        /// </summary>
        Task<CreatePaymentLinkResponse> CreatePaymentLinkAsync(
            CreatePaymentLinkRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Fetch the current state of a payment link by its order code.
        /// </summary>
        Task<PaymentLink> GetPaymentLinkAsync(
            long orderCode,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Cancel a still-pending payment link. Optionally provide a reason
        /// that will be stored on PayOS and shown to the buyer.
        /// </summary>
        Task<PaymentLink> CancelPaymentLinkAsync(
            long orderCode,
            string? cancellationReason = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Verify the HMAC signature on a webhook payload received from PayOS
        /// and return the parsed <see cref="WebhookData"/>. Throws
        /// <c>PayOS.Exceptions.WebhookException</c> if the signature is invalid
        /// or the payload is malformed.
        /// </summary>
        Task<WebhookData> VerifyWebhookAsync(
            Webhook webhook,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Register / confirm the webhook URL that PayOS should call when a
        /// payment's status changes. Must be called once after deployment.
        /// </summary>
        Task<ConfirmWebhookResponse> ConfirmWebhookAsync(
            string webhookUrl,
            CancellationToken cancellationToken = default);
    }
}
