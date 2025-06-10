using Microsoft.AspNetCore.Mvc;
using RagChatPoC.Api.Data;
using RagChatPoC.Api.Repositories;
using RagChatPoC.Domain.Models.Credentials;

namespace RagChatPoC.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CredentialsController(ICredentialsRepository repository) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Credentials>> GetById(Guid id)
    {
        var credentials = await repository.GetById(id);
        if (credentials == null)
        {
            return NotFound();
        }
        return Ok(credentials);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Credentials>>> GetAll()
    {
        var credentials = await repository.GetAll();
        return Ok(credentials);
    }

    [HttpPost]
    public async Task<ActionResult> Add([FromBody] CreateCredentialsDto credentialsDto)
    {
        var credentials = await repository.Add(new Credentials()
        {
            Id = Guid.NewGuid(),
            Data = credentialsDto.Data
        });
        return CreatedAtAction(nameof(GetById), new { id = credentials.Id }, credentials);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] Credentials credentials)
    {
        if (id != credentials.Id)
        {
            return BadRequest("ID mismatch");
        }

        await repository.Update(credentials);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var existingCredentials = await repository.GetById(id);
        if (existingCredentials == null)
        {
            return NotFound();
        }

        await repository.Delete(id);
        return NoContent();
    }
}