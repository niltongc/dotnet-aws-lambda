using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace DotnetLambda.Controllers;

[Route("[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private static readonly string _tableName = "users";
    private readonly IAmazonDynamoDB _dynamoDBClient;

    public UserController(IAmazonDynamoDB dynamoDBClient)
    {
        _dynamoDBClient = dynamoDBClient;
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUser([FromRoute] string userId)
    {
        var request = new GetItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "id", new AttributeValue { S = userId } }
            }
        };

        var response = await _dynamoDBClient.GetItemAsync(request);

        if (response.Item == null || response.Item.Count == 0)
            return NotFound();

        if (!Guid.TryParse(response.Item["id"].S, out Guid userGuid))
            return BadRequest("Invalid Guid");

        var user = new User
        {
            Id = userGuid,
            Name = response.Item["name"].S,
            Email = response.Item["email"].S
        };

        return Ok(user);
    }
}

