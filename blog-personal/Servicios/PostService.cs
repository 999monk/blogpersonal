using System.Text.Json;
using blog_personal.Models;

namespace blog_personal.Servicios;

public class PostService : IPostService
{
    private readonly string _dataPath;
    private readonly string _indexPath;

    public PostService(string basePath = null)
    {
        basePath ??= "wwwroot";
        _dataPath = Path.Combine(basePath, "data", "posts");
        _indexPath = Path.Combine(basePath, "data", "index.json");
        
        if (!Directory.Exists(_dataPath))
            Directory.CreateDirectory(_dataPath);
        if (!File.Exists(_indexPath))
            File.WriteAllText(_indexPath, "[]");
    }

    public async Task<List<Post>> GetAllPostsAsync()
    {
        try
        {
            if (!File.Exists(_indexPath))
                return new List<Post>();
        
            string json = await File.ReadAllTextAsync(_indexPath);
            return JsonSerializer.Deserialize<List<Post>>(json) ?? new List<Post>();
        }
        catch (IOException ex)
        {
            return new List<Post>();
        }
    }

    public async Task<Post?> GetPostByIdAsync(string id)
    {
        string postPath = Path.Combine(_dataPath, $"{id}.json");
        if (!File.Exists(postPath))
            return null;
        
        string json = await  File.ReadAllTextAsync(postPath);   
        return JsonSerializer.Deserialize<Post>(json);
    }

    public async Task<Post> CreatePostAsync(Post post)
    {
        // asignar id
        if (string.IsNullOrWhiteSpace(post.Id))
            post.Id = Guid.NewGuid().ToString();
        
        post.PublishedDate = DateTime.Now;
        
        // guardar post
        string postPath = Path.Combine(_dataPath, $"{post.Id}.json");
        string jsonPost = JsonSerializer.Serialize(post, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(postPath, jsonPost);
        
        // act index
        var posts = await GetAllPostsAsync();
        posts.Add(post);
        string jsonIndex = JsonSerializer.Serialize(posts, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_indexPath, jsonIndex);
        
        return post;
    }

    public async Task<Post> UpdatePostAsync(Post post)
    {
        var existing = await GetPostByIdAsync(post.Id);
        if (existing == null)
            throw new ArgumentException("Post no encontrado");
        
        //mantener fecha
        post.PublishedDate = existing.PublishedDate;
        post.ActualizedPubDate = DateTime.Now;
        
        // act post
        string postPath = Path.Combine(_dataPath, $"{post.Id}.json");
        string jsonPost = JsonSerializer.Serialize(post, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(postPath, jsonPost);
        
        //act index
        var posts = await GetAllPostsAsync();
        int index = posts.FindIndex(p => p.Id == post.Id);
        if (index != -1)
            posts[index] = post;
        
        string jsonIndex = JsonSerializer.Serialize(posts, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_indexPath, jsonIndex);
        
        return post;
    }

    public async Task DeletePostAsync(string id)
    {
        // elimina
        string postPath = Path.Combine(_dataPath, $"{id}.json");
        
        if (!File.Exists(postPath))
            throw new  ArgumentException("Post no encontrado");
        
        File.Delete(postPath);
        
        // act index
        var posts = await GetAllPostsAsync();
        posts.RemoveAll(p => p.Id == id);
        
        string jsonIndex = JsonSerializer.Serialize(posts, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_indexPath, jsonIndex);
    }
    
}