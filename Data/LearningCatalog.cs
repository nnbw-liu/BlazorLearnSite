using Markdig;

namespace BlazorLearnSite.Data;

/// <summary>
/// 一节课：元数据 + Markdown 正文。
/// 正文由 Markdig 渲染为 HTML，页面直接用 <see cref="Html"/>。
/// </summary>
public sealed record Course(
    string Id,
    string Title,
    string Summary,
    int Minutes,
    string Body)
{
    /// <summary>Markdown 正文渲染后的 HTML（首次访问时惰性渲染）。</summary>
    public string Html => _html ??= Markdig.Markdown.ToHtml(Body);
    private string? _html;
}

/// <summary>一个学习模块（对应一条学习路径）。</summary>
public sealed record LearningModule(
    string Id,
    int Order,
    string Title,
    string Summary,
    string Icon,
    IReadOnlyList<Course> Lessons);

/// <summary>
/// 站点全部课程目录。模块与课程按学习顺序排列，
/// 具体内容分散在 Catalog.*.cs 分部类文件中。
/// </summary>
public static partial class Catalog
{
    /// <summary>全部模块（按学习顺序）。</summary>
    public static IReadOnlyList<LearningModule> Modules { get; } =
        [Basics, Advanced, Controls, Libraries, Auth, Architecture];

    /// <summary>课程总数。</summary>
    public static int TotalLessons => Modules.Sum(m => m.Lessons.Count);

    public static LearningModule? FindModule(string moduleId) =>
        Modules.FirstOrDefault(m => m.Id == moduleId);

    public static Course? FindLesson(string lessonId) =>
        Modules.SelectMany(m => m.Lessons).FirstOrDefault(l => l.Id == lessonId);

    public static LearningModule? ModuleOf(string lessonId) =>
        Modules.FirstOrDefault(m => m.Lessons.Any(l => l.Id == lessonId));

    public static Course? PreviousLesson(string lessonId)
    {
        var all = Modules.SelectMany(m => m.Lessons).ToList();
        var i = all.FindIndex(l => l.Id == lessonId);
        return i > 0 ? all[i - 1] : null;
    }

    public static Course? NextLesson(string lessonId)
    {
        var all = Modules.SelectMany(m => m.Lessons).ToList();
        var i = all.FindIndex(l => l.Id == lessonId);
        return i >= 0 && i < all.Count - 1 ? all[i + 1] : null;
    }
}
