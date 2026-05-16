using System.Text;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;
using dotenv.net;
using Microsoft.AspNetCore.Http;
using AWS.Cryptography.DbEncryptionSDK.DynamoDb;
using AWS.Cryptography.MaterialProviders;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace DotnetLambda.Controllers;

[Route("[controller]")]
[ApiController]
public class UserEncryptedController : ControllerBase
{
    private static readonly string _tableName = "users";
    private readonly IAmazonDynamoDB _dynamoDBEncryptedClient;
    private readonly IAmazonKeyManagementService _kmsClient;
    private readonly IConfiguration _configuration;

    public UserEncryptedController(IAmazonDynamoDB dynamoDBEncryptedClient, IAmazonKeyManagementService kmsClient, IConfiguration configuration)
    {
        _dynamoDBEncryptedClient = dynamoDBEncryptedClient;
        _kmsClient = kmsClient;
        _configuration = configuration;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] UserDto user)
    {
        
        var encryptRequest = new EncryptRequest
        {
            KeyId = _configuration["KMS_KEY_ID"],
            Plaintext = new MemoryStream(Encoding.UTF8.GetBytes(user.Email))
        };

        var encryptResponse = await _kmsClient.EncryptAsync(encryptRequest);

        var encryptedEmail = Convert.ToBase64String(
        encryptResponse.CiphertextBlob.ToArray());

        var request = new PutItemRequest
        {
            TableName = _tableName,
            Item = new Dictionary<string, AttributeValue>
            {
                { "id", new AttributeValue { S = user.Id.ToString() } },
                { "name", new AttributeValue { S = user.Name } },
                { "email", new AttributeValue { S = encryptedEmail } }
            }
        };

        await _dynamoDBEncryptedClient.PutItemAsync(request);

        return CreatedAtAction(nameof(CreateUser), new { userId = user.Id }, user);
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(string id)
    {

        var request = new GetItemRequest
        {
            TableName = _configuration["TABLE_NAME"],
            Key = new Dictionary<string, AttributeValue>
            {
                { "id", new AttributeValue { S = id } }
            }
        };

        var response = await _dynamoDBEncryptedClient.GetItemAsync(request);

        if (response.Item == null || response.Item.Count == 0)
            return NotFound();

        var encryptedEmailBase64 = response.Item["email"].S;

        var cipherBytes = Convert.FromBase64String(encryptedEmailBase64);

        var decryptRequest = new DecryptRequest
        {
            CiphertextBlob = new MemoryStream(cipherBytes)
        };

        var decryptResponse = await _kmsClient.DecryptAsync(decryptRequest);

        var email = Encoding.UTF8.GetString(decryptResponse.Plaintext.ToArray());

        var user = new
        {
            Id = response.Item["id"].S,
            Name = response.Item["name"].S,
            Email = email
        };

        return Ok(user);
    }

}

