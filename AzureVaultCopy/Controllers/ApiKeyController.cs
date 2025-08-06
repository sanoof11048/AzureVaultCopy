using AzureVaultCopy.Models;
using AzureVaultCopy.Services;
using Microsoft.AspNetCore.Http;
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
        [HttpGet("{keyName}")]
        public async Task<IActionResult> GetMetadata(string keyName)
        {
            var metadata = await _service.GetMetadataAsync(keyName);
            return metadata == null ? NotFound() : Ok(metadata);
        }

        [HttpPost("validate")]
        public async Task<IActionResult> Validate([FromBody] string key)
        {
            var isValid = await _service.ValidateKeyAsync(key);
            return Ok(new { isValid });
        }
        [HttpGet]
        public async Task<IActionResult> GetAllKeys()
        {
            var keys = await _service.GetAllKeysAsync();
            return Ok(keys);
        }
    }
}
