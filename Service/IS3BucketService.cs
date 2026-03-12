using Amazon.S3;
using Amazon.S3.Model;

namespace BotApiTemplate.Service
{
    public interface IS3BucketService
    {
        Task UploadAsync(Stream stream, string key);
        Task<Stream> DownloadAsync(string key);
        Task DeleteAsync(string key);
    }

    public class S3BucketService : IS3BucketService
    {
        private readonly IAmazonS3 _s3;
        private readonly string? _bucket;

        public S3BucketService(IAmazonS3 s3)
        {
            _s3 = s3;
            _bucket = Environment.GetEnvironmentVariable("AWS_S3_BUCKET_NAME");
        }

        public async Task UploadAsync(Stream stream, string key)
        {
            string extension = Path.GetExtension(key).ToLower();

            string contentType = extension switch
            {
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
            var request = new PutObjectRequest
            {
                BucketName = _bucket,
                Key = key,
                InputStream = stream,
                ContentType = contentType,
                AutoCloseStream = false,
                Headers =
                {
                    ContentLength = stream.Length
                }
            };

            DisableChunkEncodingIfSupported(request);
            await _s3.PutObjectAsync(request);
        }


        private static void DisableChunkEncodingIfSupported(PutObjectRequest request)
        {
            var property = request.GetType().GetProperty("UseChunkEncoding");
            if (property?.CanWrite == true && property.PropertyType == typeof(bool))
            {
                property.SetValue(request, false);
            }
        }

        public async Task<Stream> DownloadAsync(string key)
        {
            var response = await _s3.GetObjectAsync(_bucket, key);
            return response.ResponseStream;
        }

        public async Task DeleteAsync(string key)
        {
            await _s3.DeleteObjectAsync(_bucket, key);
        }
    }
}
