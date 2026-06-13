namespace CoreLibrary.Storage
{
    /// <summary>
    /// High-level service over Cloudflare R2. Re-exposes the operations the
    /// EasyJapanese app needs (upload, delete, public URL) so call sites do
    /// not deal with S3 SDK types directly.
    /// </summary>
    public interface IStorageService
    {
        /// <summary>
        /// Upload a file (image, video, PDF) to R2 and return the public URL
        /// where it can be fetched via the Cloudflare CDN.
        /// </summary>
        /// <param name="key">Object key inside the bucket, e.g. "avatars/42.jpg".</param>
        /// <param name="content">File contents.</param>
        /// <param name="contentType">MIME type, e.g. "image/jpeg", "video/mp4".</param>
        Task<string> UploadAsync(string key, Stream content, string contentType, CancellationToken cancellationToken = default);

        /// <summary>Delete an object by key. No-op if the object does not exist.</summary>
        Task DeleteAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Build the public URL for an object without checking it exists.
        /// Use this when you have just stored the object and know the key.
        /// </summary>
        string GetPublicUrl(string key);

        /// <summary>
        /// Generate a presigned PUT URL so a browser can upload directly to R2
        /// without proxying through your server. Useful for large video files.
        /// </summary>
        /// <param name="key">Object key the client should upload to.</param>
        /// <param name="contentType">Expected content type. Browser must match.</param>
        /// <param name="expiresIn">How long the URL is valid.</param>
        string GenerateUploadUrl(string key, string contentType, TimeSpan expiresIn);
    }
}
