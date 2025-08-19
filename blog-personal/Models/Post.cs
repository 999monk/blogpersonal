namespace blog_personal.Models;

public class Post
{
    public string Id { get; set; } 
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime PublishedDate { get; set; }
    public DateTime ActualizedPubDate { get; set; }
    public bool IsPublushed { get; set; }
    public string[] Tags { get; set; } = Array.Empty<string>();
}