using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using blog_personal.Models;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using Xunit;

namespace blog_personal.Test.Controllers;

public class PostControllerIntegrationTests : IClassFixture<BlogWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PostControllerIntegrationTests(BlogWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_EmptyBlog_ReturnsEmptyList()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var existingPostsResponse = await _client.GetAsync("/api/post");
        var existingPosts = await existingPostsResponse.Content.ReadFromJsonAsync<List<Post>>();
        foreach (var post in existingPosts)
        {
            await _client.DeleteAsync($"/api/post/{post.Id}");
        }

        // Act
        var response = await _client.GetAsync("/api/post");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var posts = JsonSerializer.Deserialize<List<Post>>(content, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        Assert.NotNull(posts);
        Assert.Empty(posts);
    }

    [Fact]
    public async Task GetById_NonExistentPost_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/post/nonexistent");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithValidPost_ReturnsCreated()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var newPost = new
        {
            Id = "asd",
            Title = "Test Post",
            Content = "This is test content",
        };

        var json = JsonSerializer.Serialize(newPost);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/post", content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var createdPost = JsonSerializer.Deserialize<Post>(responseContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        Assert.NotNull(createdPost);
        Assert.Equal("Test Post", createdPost.Title);
        Assert.False(string.IsNullOrEmpty(createdPost.Id));
    }

    [Fact]
    public async Task Create_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var newPost = new
        {
            Title = "Test Post",
            Content = "This is test content"
        };

        var json = JsonSerializer.Serialize(newPost);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/post", content);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task FullCrudWorkflow_CreateReadUpdateDelete_WorksCorrectly()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 1 CREATE
        var newPost = new
        {
            Id = "asdf",
            Title = "CRUD Test Post",
            Content = "Original content",
        };

        var json = JsonSerializer.Serialize(newPost);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var createResponse = await _client.PostAsync("/api/post", content);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createdContent = await createResponse.Content.ReadAsStringAsync();
        var createdPost = JsonSerializer.Deserialize<Post>(createdContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        var postId = createdPost!.Id;

        // 2 READ(id)
        var getResponse = await _client.GetAsync($"/api/post/{postId}");
        getResponse.EnsureSuccessStatusCode();

        var getContent = await getResponse.Content.ReadAsStringAsync();
        var retrievedPost = JsonSerializer.Deserialize<Post>(getContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        Assert.Equal("CRUD Test Post", retrievedPost!.Title);

        // 3 UPDATE
        var updatedPost = new
        {
            Id = postId,
            Title = "Updated CRUD Test Post",
            Content = "Updated content",
            PublishedDate = createdPost.PublishedDate 
        };

        var updateJson = JsonSerializer.Serialize(updatedPost);
        var updateContent = new StringContent(updateJson, Encoding.UTF8, "application/json");

        var updateResponse = await _client.PutAsync($"/api/post/{postId}", updateContent);
        updateResponse.EnsureSuccessStatusCode();

        // verificar
        var getUpdatedResponse = await _client.GetAsync($"/api/post/{postId}");
        var updatedContent = await getUpdatedResponse.Content.ReadAsStringAsync();
        var finalPost = JsonSerializer.Deserialize<Post>(updatedContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        Assert.Equal("Updated CRUD Test Post", finalPost!.Title);

        // 4 DELETE
        var deleteResponse = await _client.DeleteAsync($"/api/post/{postId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // verificar
        var getDeletedResponse = await _client.GetAsync($"/api/post/{postId}");
        Assert.Equal(HttpStatusCode.NotFound, getDeletedResponse.StatusCode);
    }

    [Fact]
    public async Task Update_WithMismatchedId_ReturnsBadRequest()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var postData = new
        {
            Id = "different-id",
            Title = "Test",
            Content = "Test content"
        };

        var json = JsonSerializer.Serialize(postData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync("/api/post/original-id", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_NonExistentPost_ReturnsNotFound()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var postData = new
        {
            Id = "nonexistent",
            Title = "Test",
            Content = "Test content"
        };

        var json = JsonSerializer.Serialize(postData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync("/api/post/nonexistent", content);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_WithMultiplePosts_ReturnsAllPosts()
    {
        var postss = await _client.GetAsync("/api/post");
        var existingPosts = await postss.Content.ReadFromJsonAsync<List<Post>>();
        
        foreach (var post in existingPosts)
        {
            await _client.DeleteAsync($"/api/post/{post.Id}");
        }
        
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        await CreateTestPost("Post 1", "Content 1", token);
        await CreateTestPost("Post 2", "Content 2", token);
        await CreateTestPost("Post 3", "Content 3", token);

        // Act
        var response = await _client.GetAsync("/api/post");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var posts = JsonSerializer.Deserialize<List<Post>>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        Assert.NotNull(posts);
        Assert.Equal(3, posts.Count);
        
        Assert.Contains(posts, p => p.Title == "Post 1");
        Assert.Contains(posts, p => p.Title == "Post 2");
        Assert.Contains(posts, p => p.Title == "Post 3");
    }

    // Helpers
    private async Task<string> GetAuthTokenAsync()
    {
        var loginRequest = new
        {
            Username = "admin",
            Password = "admin123"
        };

        var json = JsonSerializer.Serialize(loginRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/auth/login", content);

        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        return result.GetProperty("token").GetString()!;
    }

    private async Task<Post> CreateTestPost(string title, string content, string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var newPost = new
        {
            Id = Guid.NewGuid().ToString(),
            Title = title,
            Content = content,
            PublishedDate = DateTime.Now
        };

        var json = JsonSerializer.Serialize(newPost);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/post", httpContent);

        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Post>(responseContent, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }
}