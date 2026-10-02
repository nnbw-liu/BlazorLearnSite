using BlazorLearnSite.Data;
using Microsoft.EntityFrameworkCore;

namespace BlazorLearnSite.Services;

/// <summary>
/// 学习进度服务：按用户读写「已完成课程」记录。
/// Blazor Server 中注册为 Scoped，底层复用 Scoped 的 ApplicationDbContext。
/// </summary>
public sealed class LearningProgressService(ApplicationDbContext db)
{
    /// <summary>某用户已完成的全部课程 Id。</summary>
    public async Task<List<string>> GetCompletedAsync(string userId)
        => await db.LessonProgresses
            .Where(p => p.UserId == userId)
            .Select(p => p.LessonId)
            .ToListAsync();

    /// <summary>某用户是否已完成某课。</summary>
    public Task<bool> IsCompletedAsync(string userId, string lessonId)
        => db.LessonProgresses
            .AnyAsync(p => p.UserId == userId && p.LessonId == lessonId);

    /// <summary>
    /// 切换某课的完成状态。返回切换后是否处于「已完成」。
    /// </summary>
    public async Task<bool> ToggleAsync(string userId, string lessonId)
    {
        var existing = await db.LessonProgresses
            .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == lessonId);

        if (existing is null)
        {
            db.LessonProgresses.Add(new LessonProgress
            {
                UserId = userId,
                LessonId = lessonId,
                CompletedAt = DateTime.UtcNow,
            });
            await db.SaveChangesAsync();
            return true;
        }

        db.LessonProgresses.Remove(existing);
        await db.SaveChangesAsync();
        return false;
    }

    /// <summary>某用户已完成的课程数。</summary>
    public Task<int> CountAsync(string userId)
        => db.LessonProgresses.CountAsync(p => p.UserId == userId);
}
