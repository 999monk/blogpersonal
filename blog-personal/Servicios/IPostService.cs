using blog_personal.Models;

namespace blog_personal.Servicios;

public interface IPostService
{
    public Task<List<Post>> GetAllPostsAsync();
    public Task<Post?> GetPostByIdAsync(string id);
    public Task<Post> CreatePostAsync(Post post);
    public Task<Post?> UpdatePostAsync(Post post);
    public Task DeletePostAsync(string id);
}