using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;

namespace CoreLibrary.Storage
{
    public class R2StorageService : IStorageService
    {
        private readonly IAmazonS3 _s3;
        private readonly R2Options _options;

        public R2StorageService(IAmazonS3 s3, IOptions<R2Options> options)
        {
            _s3 = s3;
            _options = options.Value;
        }

        public async Task<string> UploadAsync(string key, Stream content, string contentType, CancellationToken cancellationToken = default)
        {
            var request = new PutObjectRequest
            {
                BucketName = _options.BucketName,
                Key = key,
                InputStream = content,
                ContentType = contentType,
                // Cache ảnh 1 năm, video 30 ngày
                Headers = { CacheControl = contentType.StartsWith("video/") ? "public, max-age=2592000" : "public, max-age=31536000" }
            };

            await _s3.PutObjectAsync(request, cancellationToken);
            return GetPublicUrl(key);
        }

        public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                await _s3.DeleteObjectAsync(_options.BucketName, key, cancellationToken);
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Idempotent — delete of missing object is success
            }
        }

        public string GetPublicUrl(string key)
        {
            return $"{_options.PublicBaseUrl.TrimEnd('/')}/{key}";
        }

        public string GenerateUploadUrl(string key, string contentType, TimeSpan expiresIn)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _options.BucketName,
                Key = key,
                Verb = HttpVerb.PUT,
                Expires = DateTime.UtcNow.Add(expiresIn),
                ContentType = contentType,
                Protocol = Protocol.HTTPS
            };

            // R2 trả URL dạng https://<accountid>.r2.cloudflarestorage.com/...
            // Browser sẽ gọi URL này để upload trực tiếp, không qua server
            return _s3.GetPreSignedURL(request);
        }
    }
}
