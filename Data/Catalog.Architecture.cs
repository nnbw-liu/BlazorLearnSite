namespace BlazorLearnSite.Data;

/// <summary>模块六：整体架构应当如何搭建。</summary>
public static partial class Catalog
{
    public static LearningModule Architecture { get; } = new(
        Id: "architecture",
        Order: 6,
        Title: "整体架构应当如何搭建",
        Summary: "从分层架构、项目结构、数据访问、配置、可观测性到部署形态，给出可直接落地的 Blazor 工程化架构方案，并逐文件解读本项目。",
        Icon: "bi-diagram-3",
        Lessons:
        [
            new Course("arc-01", "分层架构总览", "三层架构、Clean Architecture、洋葱模型怎么选。", 30,
                """
                # 分层架构总览

                ## 1. 三层架构（最常见、最易落地）

                ```
                ┌─────────────────────────────────────┐
                │ 表现层（Blazor 组件、页面、布局）      │  UI 与交互
                ├─────────────────────────────────────┤
                │ 应用层（服务、用例、DTO、校验）        │  业务规则编排
                ├─────────────────────────────────────┤
                │ 领域层（实体、值对象、领域服务）        │  核心业务（可选）
                ├─────────────────────────────────────┤
                │ 基础设施层（EF Core、仓储、外部 API）  │  技术细节
                └─────────────────────────────────────┘
                ```

                **依赖方向**：上层依赖下层，基础设施层被「接口反向」到应用层（依赖倒置）。

                ## 2. 三种架构怎么选

                | 架构 | 复杂度 | 适用 |
                |---|---|---|
                | **三层架构** | 低 | 中小型业务系统（本项目这类） |
                | **Clean / 洋葱架构** | 中 | 复杂领域、长期演进、多团队 |
                | **模块化单体** | 中 | 按业务模块拆分，兼顾单体简单与模块边界 |

                > 忠告：**别为学习网站过度设计**。20 个页面的项目用 Clean Architecture 是负资产。起步三层，业务复杂到痛了再演进。

                ## 3. 分层与 Blazor 的对应

                ```
                Components/Pages/*.razor   → 表现层
                Services/*.cs              → 应用层（业务服务）
                Data/*.cs                  → 基础设施层（EF Core、模型）
                ```

                ## 自测

                - 依赖方向为什么是「上层依赖下层」而不是反过来？
                - 什么信号出现时，才需要从三层升级到 Clean Architecture？
                """
            ),
            new Course("arc-02", "解决方案与项目结构", "单项目 vs 多项目、按功能 vs 按层组织。", 25,
                """
                # 解决方案与项目结构

                ## 1. 单项目 vs 多项目

                **单项目（本项目）**：

                ```
                BlazorLearnSite/
                ├─ Program.cs
                ├─ Components/
                │  ├─ Account/         认证脚手架
                │  ├─ Layout/          布局
                │  └─ Pages/           页面
                ├─ Data/               模型 + DbContext
                ├─ Services/           业务服务
                ├─ wwwroot/            静态资源
                └─ appsettings.json
                ```

                优点：简单、部署容易、改起来快。适合中小项目。

                **多项目（解决方案）**：

                ```
                Solution.sln
                ├─ src/
                │  ├─ App.Web/         表现层（Blazor）
                │  ├─ App.Application/ 应用服务与接口
                │  ├─ App.Domain/      领域实体
                │  └─ App.Infrastructure/ EF Core、仓储
                └─ tests/
                   ├─ App.UnitTests/   bUnit + xUnit
                   └─ App.E2ETests/    Playwright
                ```

                优点：边界清晰、可独立测试、适合大团队。

                ## 2. 组织方式：按层 vs 按功能

                ```
                按技术层（推荐起步）          按业务功能（推荐演进）
                ├─ Pages/                  ├─ Features/
                │  ├─ Courses/             │  ├─ Courses/
                │  ├─ Account/             │  ├─ Account/
                │  └─ Admin/               │  └─ Admin/
                ```

                功能模块化后，每个 Feature 自带页面、服务、模型，团队并行开发互不干扰。

                ## 3. 本项目为什么单项目

                学习站体量小、一人维护，单项目 + 清晰目录 = 最低成本、最高可读性。**架构服务于规模**。

                ## 自测

                - 什么规模适合拆多项目？
                - 「按功能组织」的最大收益是什么？
                """
            ),
            new Course("arc-03", "数据访问层：EF Core 与仓储", "DbContext 生命周期、仓储要不要用、迁移策略。", 30,
                """
                # 数据访问层：EF Core 与仓储

                ## 1. DbContext 生命周期

                ```csharp
                // Program.cs
                builder.Services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlite(connectionString));
                ```

                - Blazor Server 中 DbContext 注册为 **Scoped**：每个用户 Circuit 一个实例。
                - **不要**把 DbContext 注册成 Singleton（并发冲突）。
                - 组件里直接 `@inject ApplicationDbContext` 简单够用；长任务注意释放。

                ## 2. 仓储模式：要不要用

                | 观点 | 理由 |
                |---|---|
                | 用仓储 | 隔离 EF Core、便于替换/单测 |
                | 不用仓储（推荐起步） | EF Core 本身就是抽象（IQueryable、ChangeTracker），再包一层是重复抽象 |

                **建议**：中小项目直接用 DbContext + 服务层方法；只有需要替换 ORM 或多数据源时才上仓储接口。

                ## 3. 查询最佳实践

                ```csharp
                // 只查需要的列
                var names = await db.Users
                    .Where(u => u.IsActive)
                    .Select(u => new { u.Id, u.UserName })
                    .ToListAsync();

                // 关联用 Include / AsNoTracking（只读查询）
                var lessons = await db.Lessons
                    .AsNoTracking()
                    .Include(l => l.Module)
                    .ToListAsync();
                ```

                ## 4. 迁移策略

                ```bash
                dotnet ef migrations add AddLessonProgress
                dotnet ef database update
                ```

                - 开发期频繁迁移；**生产用发布脚本执行**（或启动时 `Database.Migrate()`，本项目采用启动自动迁移以简化部署）。
                - 别在生产库上 `EnsureCreated`（不会更新已有表结构）。

                ## 5. 本项目数据模型

                ```
                ApplicationUser（Identity）── 1 ── * LessonProgress（UserId, LessonId, CompletedAt）
                课程目录是静态数据（LearningCatalog），进度表按 Id 关联，无需课程表
                ```

                ## 自测

                - DbContext 注册成 Singleton 会有什么问题？
                - 什么时候值得引入仓储模式？
                """
            ),
            new Course("arc-04", "配置管理", "appsettings、环境变量、Options 模式、Secret Manager。", 20,
                """
                # 配置管理

                ## 1. 配置来源优先级

                ```
                appsettings.json
                → appsettings.{Environment}.json（开发/生产覆盖）
                → 环境变量
                → 命令行参数 / 用户机密
                ```

                ```csharp
                // Program.cs
                var builder = WebApplication.CreateBuilder(args);
                var conn = builder.Configuration.GetConnectionString("DefaultConnection");
                ```

                ## 2. Options 模式：强类型配置

                ```csharp
                public class SiteOptions
                {
                    public const string Section = "Site";
                    public string Name { get; set; } = "";
                    public int PageSize { get; set; } = 10;
                }
                ```

                ```csharp
                builder.Services.Configure<SiteOptions>(builder.Configuration.GetSection(SiteOptions.Section));
                ```

                ```csharp
                // 组件里用
                @inject IOptions<SiteOptions> Options
                ```

                ## 3. 敏感信息：Secret Manager（开发）

                ```bash
                dotnet user-secrets set "Auth:GitHub:ClientSecret" "xxxx"
                ```

                ## 4. 生产环境：环境变量

                ```bash
                # Linux
                export ConnectionStrings__DefaultConnection="Data Source=/data/app.db"
                export Auth__GitHub__ClientSecret="xxxx"
                ```

                命名规则：`层级__键名`（双下划线）。Docker 里用 `-e` 传入。

                ## 5. 本项目实践

                ```json
                // appsettings.json
                {
                  "ConnectionStrings": {
                    "DefaultConnection": "Data Source=Data/app.db"
                  }
                }
                ```

                部署时用环境变量覆盖连接串（把数据库放到持久化卷），见 `DEPLOY.md`。

                ## 自测

                - 生产环境覆盖连接串有几种方式？
                - Options 模式相比直接读 Configuration 的优势？
                """
            ),
            new Course("arc-05", "日志、健康检查与监控", "结构化日志、/health 端点、OpenTelemetry 可选。", 20,
                """
                # 日志、健康检查与监控

                ## 1. 健康检查端点

                ```csharp
                // Program.cs
                builder.Services.AddHealthChecks()
                    .AddDbContextCheck<ApplicationDbContext>();
                app.MapHealthChecks("/health");
                ```

                部署后可访问 `https://你的域名/health` 返回 `Healthy`，供负载均衡器、监控系统探测。

                ## 2. 结构化日志（复习）

                ```csharp
                builder.Host.UseSerilog();  // 见 lib-05 课
                Logger.LogInformation("用户 {User} 完成了课程 {Course}", uid, lessonId);
                ```

                ## 3. 集中监控选型

                | 方案 | 说明 |
                |---|---|
                | **Seq / Loki + Grafana** | 轻量，适合小团队 |
                | **OpenTelemetry + Prometheus** | 标准协议，可观测性全家桶 |
                | **Azure App Insights** | 云厂商方案，零运维 |

                ```csharp
                // 接入 OpenTelemetry 示例
                builder.Services.AddOpenTelemetry()
                    .WithTracing(t => t.AddAspNetCoreInstrumentation())
                    .WithMetrics(m => m.AddAspNetCoreInstrumentation());
                ```

                ## 4. 部署后的常规巡检

                - `GET /health` 是否 200。
                - 日志里是否有 Error 级别告警。
                - 数据库文件大小与磁盘水位。
                - HTTPS 证书到期时间。

                ## 自测

                - 健康检查端点有什么用？
                - 小团队监控日志最省的方案是什么？
                """
            ),
            new Course("arc-06", "部署架构：IIS / Nginx / Docker", "三种部署形态与 Blazor Server 的 SignalR 特殊要求。", 30,
                """
                # 部署架构：IIS / Nginx / Docker

                Blazor Server 基于 **SignalR（WebSocket）**，反向代理必须正确转发升级头，否则页面「一直重连」。

                ## 1. 方案一：Windows + IIS

                - 安装 ASP.NET Core Hosting Bundle + IIS。
                - 发布：`dotnet publish -c Release`，站点指向 `publish` 目录。
                - 应用池启用 WebSocket：**站点 → 配置编辑器 → system.webServer/webSocket → enabled=true**。

                ## 2. 方案二：Linux + Nginx（反向代理）

                ```nginx
                server {
                    listen 80;
                    server_name your-domain.com;

                    location / {
                        proxy_pass http://localhost:5000;
                        proxy_http_version 1.1;
                        proxy_set_header Upgrade $http_upgrade;      # 关键！
                        proxy_set_header Connection "upgrade";        # 关键！
                        proxy_set_header Host $host;
                        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
                        proxy_set_header X-Forwarded-Proto $scheme;
                        proxy_cache_bypass $http_upgrade;
                        proxy_read_timeout 3600s;                     # SignalR 长连接
                    }
                }
                ```

                > `Upgrade` / `Connection` 两个头缺失是 Blazor Server 部署最常见的坑。

                ## 3. 方案三：Docker（本项目提供 Dockerfile）

                ```bash
                docker build -t blazor-learn-site .
                docker run -d -p 8080:8080 \
                  -v blazor-data:/app/Data \
                  -e ConnectionStrings__DefaultConnection="Data Source=/app/Data/app.db" \
                  blazor-learn-site
                ```

                关键点：**数据库文件挂持久化卷**，容器重建不丢数据。

                ## 4. 应用本身需注意

                ```csharp
                // Program.cs 生产分支
                if (!app.Environment.IsDevelopment())
                {
                    app.UseExceptionHandler("/Error");
                    app.UseHsts();                    // 强制 HTTPS
                    app.UseHttpsRedirection();
                }
                ```

                - 反向代理后面临 HTTPS 时，代理需传 `X-Forwarded-Proto`。
                - 配置 `ForwardedHeaders`（`UseForwardedHeaders`）让重定向与 HTTPS 正确。

                ## 自测

                - Nginx 转发 SignalR 必须配置哪两个头？
                - Docker 部署时数据库文件为什么要挂卷？
                """
            ),
            new Course("arc-07", "本项目架构逐文件解读", "把 BlazorLearnSite 每个关键文件串成一张架构图。", 25,
                """
                # 本项目架构逐文件解读

                ## 一张图看懂本项目

                ```
                浏览器（远端访问）
                    │  HTTPS / WebSocket（SignalR）
                    ▼
                Program.cs ── 请求管道：静态资源、认证、路由、/health
                    │
                    ▼
                App.razor → Routes.razor → 页面组件
                    │
                    ├── Components/Layout/MainLayout.razor   站点布局 + 导航
                    ├── Components/Account/**                Identity 登录/注册/管理
                    ├── Components/Pages/Home.razor          学习路线总览 + 进度
                    ├── Components/Pages/Courses.razor       全部课程列表 + 完成状态
                    ├── Components/Pages/Course.razor        课程正文 + 标记完成
                    │        │
                    │        ├── Data/LearningCatalog.cs     课程静态数据（Markdown）
                    │        ├── Markdig                    渲染 Markdown → HTML
                    │        └── Services/LearningProgressService.cs  读写进度
                    │
                    ▼
                ApplicationDbContext ── SQLite（Data/app.db）
                    ├── AspNetUsers 等 Identity 表     身份
                    └── LessonProgresses               学习进度
                ```

                ## 各文件职责

                | 文件 | 职责 |
                |---|---|
                | `Program.cs` | 服务注册、请求管道、启动自动迁移 |
                | `Components/App.razor` | 应用根组件 |
                | `Components/Routes.razor` | 组件路由表 |
                | `Data/ApplicationDbContext.cs` | EF Core 上下文（Identity + 进度表） |
                | `Data/LearningCatalog.cs` | 课程目录静态数据与查找 |
                | `Data/Catalog.*.cs` | 六个模块的课程内容 |
                | `Services/LearningProgressService.cs` | 学习进度读写（按用户） |
                | `Components/Pages/Course.razor` | 课程详情 + 完成按钮 + 上/下节 |
                | `wwwroot/app.css` | 全站样式 |

                ## 分层映射

                ```
                表现层：Components/**（Razor 页面）
                应用层：Services/LearningProgressService.cs
                基础设施层：Data/**（DbContext、模型、迁移）
                静态内容：课程目录 = 数据（Data/Catalog.*）
                ```

                ## 如果要继续演进

                1. 课程正文从静态数据改为数据库表 + 管理界面。
                2. 增加「完成时间统计」「学习日历」报表。
                3. 拆多项目、引入 Clean Architecture（见 arc-01/02）。
                4. 加 MudBlazor 等控件库统一 UI（见 ctl-06）。

                ## 自测

                - 本项目的课程内容属于哪一层？
                - 想加一个「我的学习统计」页面，需要动哪些文件？
                """
            ),
        ]);
}
