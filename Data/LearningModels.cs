using System.ComponentModel.DataAnnotations;

namespace BlazorLearnSite.Data;

/// <summary>
/// 一条学习进度记录：某个用户在某时刻完成了某节课。
/// 主键由 EF Core 自动生成，UserId + LessonId 唯一。
/// </summary>
public sealed class LessonProgress
{
    public int Id { get; set; }

    /// <summary>ASP.NET Core Identity 用户主键（ApplicationUser.Id）。</summary>
    [MaxLength(450)]
    public required string UserId { get; set; }

    /// <summary>课程 Id（见 LearningCatalog 中 Lesson.Id，如 "basic-01"）。</summary>
    [MaxLength(64)]
    public required string LessonId { get; set; }

    /// <summary>完成时间。</summary>
    public DateTime CompletedAt { get; set; }
}
