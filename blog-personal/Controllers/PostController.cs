using blog_personal.Models;
using blog_personal.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace blog_personal.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostController : ControllerBase
{
    private readonly IPostService _postService;
    
    public PostController(IPostService postService)
    {
        _postService = postService;
    }

    // GET: api/post
    [HttpGet]
    public async Task<ActionResult<List<Post>>> GetAll()
    {
        var posts = await _postService.GetAllPostsAsync();
        return Ok(posts);
    }

    // GET: api/post/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Post>> GetById(string id)
    { 
        var post  = await _postService.GetPostByIdAsync(id);
        if (post == null)
            return NotFound();
        
        return Ok(post);
    }
    
    // POST: api/post
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Post>> Create(Post post)
    {
        var newPost = await _postService.CreatePostAsync(post);
        return CreatedAtAction(nameof(GetById), new { id = newPost.Id }, newPost);
    }

    // PUT: api/post/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Post>> Update(string id, Post post)
    {
        try
        {
            if (id != post.Id)
                return BadRequest("ID mismatch");
            
            var updatedPost = await _postService.UpdatePostAsync(post);
            return Ok(updatedPost);
        }
        catch (ArgumentException ex) when (ex.Message.Contains("no encontrado"))
        {
            return NotFound(); 
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    // DELETE: api/post/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(string id)
    {
        await _postService.DeletePostAsync(id);
        return NoContent();
    }
}