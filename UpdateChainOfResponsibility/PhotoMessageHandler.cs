using Npgsql.Replication.PgOutput.Messages;
using Telegram.Bot;
using Telegram.Bot.Types;
using BotApiTemplate.Service;

namespace BotApiTemplate.UpdateChainOfResponsibility
{
    public class PhotoMessageHandler(
        ITelegramBotClient bot,
        IS3BucketService s3BucketService)
        : IUpdateHandler
    {

        public async Task HandleAsync(Update update, UpdateContext context, Func<Task> next, CancellationToken ct)
        {
            await next();
            return;
            //TODO use code below as example of how to download photo from Telegram and upload to S3 bucket
            //if (update.Message?.Photo?.Any() != true)
            //{
            //    await next();
            //    return;
            //}

            //var fileId = update.Message.Photo.Last().FileId;
            //var key = Guid.NewGuid().ToString();

            //var file = await bot.GetFile(fileId);
            //string extension = Path.GetExtension(file.FilePath);
            //key += extension;

            //using var stream = new MemoryStream();
            //await bot.DownloadFile(file.FilePath, stream);
            //stream.Seek(0, SeekOrigin.Begin);

            //Console.WriteLine($"Uploading photo to S3 bucket with key: {key}");
            //Console.WriteLine($"Photo size: {stream.Length} bytes");
            //Console.WriteLine($"Photo content type: {file.FilePath}");

            //await s3BucketService.UploadAsync(stream, key);

            //Console.WriteLine($"Photo uploaded to S3 bucket with key: {key}");
        }
    }
}
