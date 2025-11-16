namespace TaskManagement.Core.Entities;

public class Task
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // "Pending", "InProgress", "Completed"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Foreign key
    public int AssignedUserId { get; set; }
    
    // Navigation property
    public User AssignedUser { get; set; } = null!;
}


