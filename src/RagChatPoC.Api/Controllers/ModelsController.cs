using Microsoft.AspNetCore.Mvc;
using RagChatPoC.Api.Data;
using RagChatPoC.Api.Repositories;

namespace RagChatPoC.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ModelsController : ControllerBase
{
    private readonly IChatModelRepository _repository;

    public ModelsController(IChatModelRepository repository)
    {
        _repository = repository;
    }

    [HttpPost]
    public async Task<IActionResult> AddChatModel([FromBody] ChatModel chatModel, CancellationToken cancellationToken)
    {
        await _repository.AddChatModel(chatModel, cancellationToken);
        return CreatedAtAction(nameof(GetChatModelById), new { id = chatModel.Id }, chatModel);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetChatModelById(Guid id, CancellationToken cancellationToken)
    {
        var chatModel = await _repository.GetChatModel(id, cancellationToken);
        if (chatModel == null)
        {
            return NotFound();
        }
        return Ok(chatModel);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllChatModels(CancellationToken cancellationToken)
    {
        var chatModels = await _repository.GetAllChatModels(cancellationToken);
        return Ok(chatModels);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateChatModel(Guid id, [FromBody] ChatModel chatModel, CancellationToken cancellationToken)
    {
        if (id != chatModel.Id)
        {
            return BadRequest("ID mismatch.");
        }

        await _repository.UpdateChatModel(chatModel, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteChatModel(Guid id, CancellationToken cancellationToken)
    {
        await _repository.DeleteChatModel(id, cancellationToken);
        return NoContent();
    }
}