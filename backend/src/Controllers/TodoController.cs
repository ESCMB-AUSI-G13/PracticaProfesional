using Microsoft.AspNetCore.Mvc;
using PracticaProfesional.Models;

namespace PracticaProfesional.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodoController : ControllerBase
{
    private static readonly List<TodoItem> _todos = new()
    {
        new TodoItem { Id = 1, Title = "Aprender Angular", Completed = false },
        new TodoItem { Id = 2, Title = "Aprender ASP.NET Core", Completed = false },
        new TodoItem { Id = 3, Title = "Conectar frontend con backend", Completed = true },
    };
    private static int _nextId = 4;

    [HttpGet]
    public ActionResult<IEnumerable<TodoItem>> GetAll() => Ok(_todos);

    [HttpPost]
    public ActionResult<TodoItem> Create([FromBody] CreateTodoDto dto)
    {
        var item = new TodoItem { Id = _nextId++, Title = dto.Title };
        _todos.Add(item);
        return CreatedAtAction(nameof(GetAll), item);
    }

    [HttpPatch("{id}/toggle")]
    public ActionResult Toggle(int id)
    {
        var item = _todos.FirstOrDefault(t => t.Id == id);
        if (item is null) return NotFound();
        item.Completed = !item.Completed;
        return Ok(item);
    }

    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        var item = _todos.FirstOrDefault(t => t.Id == id);
        if (item is null) return NotFound();
        _todos.Remove(item);
        return NoContent();
    }
}

public record CreateTodoDto(string Title);
