using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DotnetLambda.Controllers;

// https://aws.amazon.com/blogs/compute/securing-amazon-s3-presigned-urls-for-serverless-applications/
// https://docs.aws.amazon.com/AmazonS3/latest/API/s3_example_s3_Scenario_PresignedUrl_section.html

[Route("[controller]")]
[ApiController]
public class FileController : ControllerBase
{
    
}

