using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlazorLearnSite.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    /// <summary>学习进度记录表：每个用户对每节课最多一条完成记录。</summary>
    public DbSet<LessonProgress> LessonProgresses => Set<LessonProgress>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<LessonProgress>(entity =>
        {
            entity.HasIndex(p => new { p.UserId, p.LessonId }).IsUnique();
        });
    }
}
