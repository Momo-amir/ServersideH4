using Microsoft.EntityFrameworkCore;

namespace Data
{
    public class TodoDbContext : DbContext
    {
        public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options) { }

        public DbSet<Cpr> Cprs { get; set; }
        public DbSet<TodoItem> TodoList { get; set; }
    }

    public class Cpr
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Value { get; set; }
    }

    public class TodoItem
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public bool IsDone { get; set; }
    }
}