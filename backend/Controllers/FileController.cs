using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DotnetLambda.Controllers;

// https://aws.amazon.com/blogs/compute/securing-amazon-s3-presigned-urls-for-serverless-applications/
// https://docs.aws.amazon.com/AmazonS3/latest/API/s3_example_s3_Scenario_PresignedUrl_section.html

[Route("[controller]")]
[ApiController]
public class FileController : ControllerBase
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName = "dotnet-lambda-cd6fa0ef-deb2-4228-945e-4222b61013ec";

    public FileController(IAmazonS3 s3Client)
    {
        _s3Client = s3Client;
    }

    // POST /file/upload-url
    [HttpPost("upload-url")]
    public IActionResult GetUploadUrl([FromBody] FileRequest request)
    {
        if (string.IsNullOrEmpty(request.Key))
            return BadRequest("O campo 'Key' é obrigatório.");

        try
        {
            var preSignedUrl = GeneratePreSignedUrl(request.Key, HttpVerb.PUT, TimeSpan.FromMinutes(5));
            return Ok(new { url = preSignedUrl });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao gerar URL de upload: {ex.Message}");
        }
    }

    // POST /file/download-url
    [HttpPost("download-url")]
    public IActionResult GetDownloadUrl([FromBody] FileRequest request)
    {
        if (string.IsNullOrEmpty(request.Key))
            return BadRequest("O campo 'Key' é obrigatório.");

        try
        {
            var preSignedUrl = GeneratePreSignedUrl(request.Key, HttpVerb.GET, TimeSpan.FromMinutes(5));
            return Ok(new { url = preSignedUrl });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao gerar URL de download: {ex.Message}");
        }
    }

    // Função interna que gera a URL pré-assinada
    private string GeneratePreSignedUrl(string key, HttpVerb verb, TimeSpan duration)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = key,
            Verb = verb,
            Expires = DateTime.UtcNow.Add(duration)
        };

        return _s3Client.GetPreSignedURL(request);
    }

    // DTO simples para receber a key do arquivo
    public class FileRequest
    {
        public string Key { get; set; } = string.Empty;
    }

}

