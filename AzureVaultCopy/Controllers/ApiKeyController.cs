using AzureVaultCopy.Services;
using Microsoft.AspNetCore.Mvc;

namespace AzureVaultCopy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiKeyController : ControllerBase
    {
        private readonly IApiKeyService _service;

        public ApiKeyController(IApiKeyService service)
        {
            _service = service;
        }

        [HttpGet("{keyName}/metadata")]
        public async Task<IActionResult> GetMetadata(string keyName)
        {
            var metadata = await _service.GetMetadataAsync(keyName);
            return metadata == null ? NotFound() : Ok(metadata);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllMetadata()
        {
            var all = await _service.GetAllKeyMetadataAsync();
            return Ok(all);
        }

        [HttpPost("validate")]
        public async Task<IActionResult> ValidateKey([FromBody] string rawKey)
        {
            if (string.IsNullOrWhiteSpace(rawKey))
                return BadRequest("Key is required.");

            var isValid = await _service.ValidateKeyAsync(rawKey);
            return Ok(new { isValid });
        }

        // ❗ Optional: This endpoint should only be for debugging or internal use
        [HttpGet("{keyName}/raw")]
        public async Task<IActionResult> GetRawKeyByName(string keyName)
        {
            var key = await _service.GetKeyValueByNameAsync(keyName);
            return key == null ? NotFound() : Ok(new { KeyValue = key });
        }
    }
}
