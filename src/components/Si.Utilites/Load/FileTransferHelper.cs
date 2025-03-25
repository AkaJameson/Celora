using System.Net;
using System.Net.Http;
using System.Security.Cryptography;

namespace Si.Utilites.Load;

/// <summary>
/// 文件传输进度事件参数
/// </summary>
public class TransferProgressEventArgs : EventArgs
{
    /// <summary>
    /// 已传输的字节数
    /// </summary>
    public long BytesTransferred { get; }

    /// <summary>
    /// 总字节数
    /// </summary>
    public long TotalBytes { get; }

    /// <summary>
    /// 传输进度（0-100）
    /// </summary>
    public int ProgressPercentage { get; }

    public TransferProgressEventArgs(long bytesTransferred, long totalBytes)
    {
        BytesTransferred = bytesTransferred;
        TotalBytes = totalBytes;
        ProgressPercentage = totalBytes > 0 ? (int)(bytesTransferred * 100 / totalBytes) : 0;
    }
}

/// <summary>
/// 文件传输结果
/// </summary>
public class TransferResult
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 文件路径
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// 文件MD5
    /// </summary>
    public string? FileMd5 { get; set; }

    /// <summary>
    /// 耗时（毫秒）
    /// </summary>
    public long ElapsedMilliseconds { get; set; }
}

/// <summary>
/// 文件传输工具类
/// </summary>
public class FileTransferHelper : IDisposable
{
    private readonly HttpClient _httpClient;
    private const int BufferSize = 81920; // 80KB

    /// <summary>
    /// 传输进度事件
    /// </summary>
    public event EventHandler<TransferProgressEventArgs>? ProgressChanged;

    public FileTransferHelper()
    {
        _httpClient = new HttpClient();
    }

    public FileTransferHelper(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// 下载文件
    /// </summary>
    /// <param name="url">文件URL</param>
    /// <param name="savePath">保存路径</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>下载结果</returns>
    public async Task<TransferResult> DownloadFileAsync(string url, string savePath, CancellationToken cancellationToken = default)
    {
        var result = new TransferResult { Success = false };
        var watch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            var bytesTransferred = 0L;

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var fileStream = File.Create(savePath);
            using var md5 = MD5.Create();

            var buffer = new byte[BufferSize];
            int bytesRead;

            while ((bytesRead = await stream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                md5.TransformBlock(buffer, 0, bytesRead, null, 0);

                bytesTransferred += bytesRead;
                ProgressChanged?.Invoke(this, new TransferProgressEventArgs(bytesTransferred, totalBytes));
            }

            md5.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            
            result.Success = true;
            result.FilePath = savePath;
            result.FileSize = bytesTransferred;
            result.FileMd5 = BitConverter.ToString(md5.Hash!).Replace("-", "").ToLowerInvariant();
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.Message;
        }
        finally
        {
            watch.Stop();
            result.ElapsedMilliseconds = watch.ElapsedMilliseconds;
        }

        return result;
    }

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="url">上传URL</param>
    /// <param name="filePath">文件路径</param>
    /// <param name="formFieldName">表单字段名</param>
    /// <param name="additionalFields">附加字段</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>上传结果</returns>
    public async Task<TransferResult> UploadFileAsync(string url, string filePath, string formFieldName = "file", 
        Dictionary<string, string>? additionalFields = null, CancellationToken cancellationToken = default)
    {
        var result = new TransferResult { Success = false };
        var watch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            using var fileStream = File.OpenRead(filePath);
            using var md5 = MD5.Create();
            using var content = new MultipartFormDataContent();

            // 计算文件MD5
            result.FileMd5 = BitConverter.ToString(md5.ComputeHash(fileStream)).Replace("-", "").ToLowerInvariant();
            fileStream.Position = 0;

            // 添加文件内容
            var fileContent = new ProgressStreamContent(fileStream, BufferSize, (bytesTransferred, totalBytes) =>
            {
                ProgressChanged?.Invoke(this, new TransferProgressEventArgs(bytesTransferred, totalBytes));
            });
            
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(GetMimeType(filePath));
            content.Add(fileContent, formFieldName, Path.GetFileName(filePath));

            // 添加附加字段
            if (additionalFields != null)
            {
                foreach (var field in additionalFields)
                {
                    content.Add(new StringContent(field.Value), field.Key);
                }
            }

            // 发送请求
            using var response = await _httpClient.PostAsync(url, content, cancellationToken);
            response.EnsureSuccessStatusCode();

            result.Success = true;
            result.FilePath = filePath;
            result.FileSize = fileStream.Length;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.Message;
        }
        finally
        {
            watch.Stop();
            result.ElapsedMilliseconds = watch.ElapsedMilliseconds;
        }

        return result;
    }

    /// <summary>
    /// 获取文件的MIME类型
    /// </summary>
    private string GetMimeType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".txt" => "text/plain",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            _ => "application/octet-stream"
        };
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// 支持进度跟踪的流内容
/// </summary>
internal class ProgressStreamContent : StreamContent
{
    private readonly Stream _stream;
    private readonly int _bufferSize;
    private readonly Action<long, long> _progress;

    public ProgressStreamContent(Stream stream, int bufferSize, Action<long, long> progress) : base(stream, bufferSize)
    {
        _stream = stream;
        _bufferSize = bufferSize;
        _progress = progress;
    }

    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        var buffer = new byte[_bufferSize];
        var totalBytes = _stream.Length;
        var bytesTransferred = 0L;

        while (true)
        {
            var bytesRead = await _stream.ReadAsync(buffer);
            if (bytesRead == 0) break;

            await stream.WriteAsync(buffer.AsMemory(0, bytesRead));
            bytesTransferred += bytesRead;
            _progress(bytesTransferred, totalBytes);
        }
    }
} 