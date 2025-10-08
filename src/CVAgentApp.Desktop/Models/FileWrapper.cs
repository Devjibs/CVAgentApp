using Microsoft.AspNetCore.Http;

namespace CVAgentApp.Desktop.Models;

public class FileWrapper : IFormFile
{
    private readonly Stream _stream;
    private readonly string _fileName;

    public FileWrapper(Stream stream, string fileName)
    {
        _stream = stream;
        _fileName = fileName;
    }

    public string ContentType => "application/octet-stream";
    public string ContentDisposition => $"form-data; name=\"file\"; filename=\"{_fileName}\"";
    public IHeaderDictionary Headers => new HeaderDictionary();
    public long Length => _stream.Length;
    public string Name => "file";
    public string FileName => _fileName;

    public void CopyTo(Stream target)
    {
        _stream.CopyTo(target);
    }

    public async Task CopyToAsync(Stream target, CancellationToken cancellationToken = default)
    {
        await _stream.CopyToAsync(target, cancellationToken);
    }

    public Stream OpenReadStream()
    {
        return _stream;
    }
}
