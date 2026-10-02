namespace BlazorLearnSite.Data;

/// <summary>模块五：身份认证。</summary>
public static partial class Catalog
{
    public static LearningModule Auth { get; } = new(
        Id: "auth",
        Order: 5,
        Title: "身份认证",
        Summary: "从认证与授权概念出发，掌握 ASP.NET Core Identity、Blazor 授权组件、角色策略与第三方登录，并解读本项目的认证实现。",
        Icon: "bi-shield-lock",
        Lessons:
        [
            new Course("auth-01", "认证与授权基础概念", "Authentication / Authorization / Claims / Cookie / JWT 一次讲清。", 25,
                """
                # 认证与授权基础概念

                ## 1. 两个词的严格区别

                | 概念 | 英文 | 回答的问题 | 例子 |
                |---|---|---|---|
                | **认证** | Authentication | 「你是谁？」 | 输入用户名密码登录 |
                | **授权** | Authorization | 「你能做什么？」 | 只有管理员能删除用户 |

                ## 2. 核心术语

                - **Principal**：当前操作者（含身份与声明）。
                - **Identity**：身份（如登录用户名）。
                - **Claim（声明）**：关于用户的一个事实键值对，如 `role=admin`、`email=a@b.com`。
                - **Ticket / Token**：认证成功后的凭证。

                ## 3. Cookie 认证（服务端场景）

                ```
                登录：提交凭证 → 服务器验证 → 签发加密 Cookie
                后续：浏览器每次请求自动带 Cookie → 服务器解密恢复身份
                ```

                特点：简单、HttpOnly 防脚本窃取、适合 Blazor Server / MVC。

                ## 4. JWT（无状态，适合 API / WASM）

                ```
                登录：提交凭证 → 服务器签发 JWT（含 Claims 与签名）
                后续：请求头 Authorization: Bearer <token> → 服务器验签即可
                ```

                特点：无会话存储、可跨域、适合 SPA / WebAssembly 客户端。

                ## 5. 流程对比图

                ```
                Cookie 认证                         JWT 认证
                浏览器 ──登录──► 服务器              浏览器 ──登录──► 服务器
                服务器 ──Set-Cookie──► 浏览器        服务器 ──签发 JWT──► 浏览器
                浏览器 ──带Cookie──► 服务器          浏览器 ──Bearer token──► 服务器
                服务器 解密恢复身份                   服务器 验签 + 解 Claims
                ```

                ## 6. 本学习网站用哪种？

                本项目是 **Blazor Server + ASP.NET Core Identity + Cookie**：浏览器里没有敏感代码，Cookie 方案最安全、最简单。

                ## 自测

                - 认证和授权各回答什么问题？
                - Blazor Server 项目选 Cookie 还是 JWT？为什么？
                """
            ),
            new Course("auth-02", "ASP.NET Core Identity 接入", "用户表、注册登录、密码哈希、依赖注入配置。", 30,
                """
                # ASP.NET Core Identity 接入

                ## 1. 项目模板自带的 Identity

                `dotnet new blazor -au Individual` 会生成完整的 Identity 脚手架：用户模型、登录/注册/管理页面、数据库迁移。本学习站即基于此。

                ## 2. 核心配置（Program.cs）

                ```csharp
                builder.Services.AddCascadingAuthenticationState();

                builder.Services.AddAuthentication(options =>
                    {
                        options.DefaultScheme = IdentityConstants.ApplicationScheme;
                        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
                    })
                    .AddIdentityCookies();

                builder.Services.AddIdentityCore<ApplicationUser>(options =>
                    {
                        options.SignIn.RequireConfirmedAccount = false;   // 是否强制邮箱确认
                        options.User.RequireUniqueEmail = true;
                        options.Password.RequiredLength = 8;
                    })
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddSignInManager()
                    .AddDefaultTokenProviders();

                // 认证状态提供者（Blazor 专用）
                builder.Services.AddScoped<AuthenticationStateProvider,
                    IdentityRevalidatingAuthenticationStateProvider>();
                ```

                ## 3. 用户模型

                ```csharp
                // Data/ApplicationUser.cs
                public class ApplicationUser : IdentityUser
                {
                    // 可以加自定义字段，如 NickName、AvatarUrl
                }
                ```

                ## 4. 注册/登录由脚手架页面完成

                - `/Account/Register`：注册
                - `/Account/Login`：登录
                - `/Account/Logout`：登出（POST 表单）
                - `/Account/Manage`：改密码、邮箱、2FA

                ## 5. 用户表结构（EF Core 自动建表）

                ```
                AspNetUsers        用户主表（UserName、Email、PasswordHash…）
                AspNetRoles        角色表
                AspNetUserRoles    用户-角色关联
                AspNetUserClaims   用户声明（如自定义权限）
                AspNetUserLogins   第三方登录关联
                AspNetUserTokens   重置密码等令牌
                ```

                ## 自测

                - Identity 的密码是明文存储吗？
                - `RequireConfirmedAccount` 设为 true 后会发生什么？
                """
            ),
            new Course("auth-03", "Blazor 授权：AuthorizeView / [Authorize] / AuthenticationStateProvider", "页面级与组件级授权、获取当前用户身份。", 30,
                """
                # Blazor 授权：AuthorizeView / [Authorize] / AuthenticationStateProvider

                ## 1. 页面级授权：[Authorize]

                ```razor
                @* Profile.razor *@
                @page "/profile"
                @attribute [Authorize]

                <h3>@context.User.Identity?.Name 的个人中心</h3>
                ```

                未登录访问 `/profile` 会被重定向到登录页（模板的 `RedirectToLogin` 组件处理）。

                ## 2. 组件级授权：AuthorizeView

                ```razor
                <AuthorizeView>
                    <Authorized>
                        <p>你好，@context.User.Identity?.Name</p>
                    </Authorized>
                    <NotAuthorized>
                        <a href="Account/Login">请先登录</a>
                    </NotAuthorized>
                </AuthorizeView>
                ```

                ## 3. 在代码里获取身份：AuthenticationStateProvider

                ```csharp
                @inject AuthenticationStateProvider AuthProvider

                @code {
                    private string? userId;
                    private string? userName;

                    protected override async Task OnInitializedAsync()
                    {
                        var state = await AuthProvider.GetAuthenticationStateAsync();
                        var user = state.User;

                        if (user.Identity?.IsAuthenticated == true)
                        {
                            userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                            userName = user.Identity.Name;
                        }
                    }
                }
                ```

                > 本学习站的学习进度服务正是用这个 `userId` 来区分每个人的进度。

                ## 4. 模板的 AuthorizationMessage / 级联状态

                - `AddCascadingAuthenticationState()` 让全站组件自动获得认证状态级联参数。
                - `IdentityRevalidatingAuthenticationStateProvider` 定期（默认 30 分钟）重新校验用户是否有效。

                ## 5. 注意：静态 SSR 页面拿不到登录状态

                纯静态渲染的页面里 `AuthenticationStateProvider` 需要 `prerender=false` 或改用 `AuthorizeView` 包裹交互部分。

                ## 自测

                - 页面级与组件级授权分别怎么写？
                - 想拿到当前登录用户的 Id 用什么 API？
                """
            ),
            new Course("auth-04", "角色、策略与 Claims 授权", "[Authorize(Roles=...)]、策略（Policy）、基于 Claims 的细粒度授权。", 30,
                """
                # 角色、策略与 Claims 授权

                ## 1. 角色授权

                ```csharp
                // Program.cs：注册角色管理服务
                builder.Services.AddScoped<RoleManager<IdentityRole>>();
                ```

                创建角色并分配给用户（在启动或管理页面里）：

                ```csharp
                var roleExists = await roleManager.RoleExistsAsync("admin");
                if (!roleExists) await roleManager.CreateAsync(new IdentityRole("admin"));
                await userManager.AddToRoleAsync(user, "admin");
                ```

                页面/组件限制：

                ```razor
                @attribute [Authorize(Roles = "admin")]

                <AuthorizeView Roles="admin">
                    <Authorized>管理员菜单</Authorized>
                </AuthorizeView>
                ```

                ## 2. 策略授权（推荐：把规则集中定义）

                ```csharp
                // Program.cs
                builder.Services.AddAuthorization(options =>
                {
                    options.AddPolicy("RequireAdmin", p => p.RequireRole("admin"));
                    options.AddPolicy("AtLeast18", p => p.RequireClaim("age", "18+"));
                    options.AddPolicy("CanManageLessons",
                        p => p.RequireAssertion(ctx =>
                            ctx.User.IsInRole("admin") ||
                            ctx.User.HasClaim("permission", "lessons:write")));
                });
                ```

                ```razor
                @attribute [Authorize(Policy = "RequireAdmin")]
                ```

                ## 3. Claims 授权

                登录时给用户加声明：

                ```csharp
                await userManager.AddClaimAsync(user, new Claim("permission", "lessons:write"));
                ```

                组件里判断：

                ```razor
                @if (context.User.HasClaim("permission", "lessons:write"))
                {
                    <button>编辑课程</button>
                }
                ```

                ## 4. 三层授权模型

                ```
                Claims（事实）→ Roles（角色=声明的组合）→ Policies（策略=规则的组合）
                声明驱动角色，策略可以组合角色/声明/自定义逻辑
                ```

                ## 自测

                - 角色授权与策略授权的本质区别？
                - 想表达「管理员或拥有课程编辑权限的人」应该用哪种？
                """
            ),
            new Course("auth-05", "第三方登录：Google / GitHub OAuth", "外部登录的配置、回调流程、账号关联。", 25,
                """
                # 第三方登录：Google / GitHub OAuth

                ## 1. 原理

                ```
                用户点击「使用 GitHub 登录」
                → 跳转 GitHub 授权页（带 ClientId）
                → 用户同意 → GitHub 回调本机 /signin-github
                → ASP.NET Core 验证令牌 → 创建/关联本地账号 → 登录
                ```

                ## 2. 注册 OAuth 应用

                - GitHub：Settings → Developer settings → OAuth Apps，回调地址填 `https://你的域名/signin-github`。
                - Google：Google Cloud Console → OAuth 2.0 Client，回调 `/signin-google`。

                ## 3. 配置（Program.cs）

                ```csharp
                builder.Services.AddAuthentication(...)
                    .AddIdentityCookies()
                    .AddGitHub(options =>
                    {
                        options.ClientId = builder.Configuration["Auth:GitHub:ClientId"]!;
                        options.ClientSecret = builder.Configuration["Auth:GitHub:ClientSecret"]!;
                        options.Scope.Add("read:user");
                    });
                ```

                ```json
                // appsettings.json（密钥放环境变量/Secret Manager）
                { "Auth": { "GitHub": { "ClientId": "", "ClientSecret": "" } } }
                ```

                需要包：`Microsoft.AspNetCore.Authentication.GitHub`（或 `...Google`）。

                ## 4. 登录页显示按钮

                ```razor
                @* Account/Pages/Login.razor 里模板已生成 *@
                <form action="Account/ExternalLogin" method="post">
                    <button type="submit" name="provider" value="GitHub">使用 GitHub 登录</button>
                </form>
                ```

                ## 5. 账号关联与坑

                - 首次第三方登录会自动创建本地账号并关联（`AspNetUserLogins` 表）。
                - 同一邮箱多次第三方登录可能报错，需处理邮箱唯一冲突。
                - 生产环境务必把 ClientSecret 放**环境变量或密钥管理**，不进代码库。

                ## 自测

                - OAuth 回调地址填什么格式？
                - ClientSecret 应该放在哪里？
                """
            ),
            new Course("auth-06", "本项目认证实现解读", "逐文件看懂 BlazorLearnSite 的认证与进度关联。", 25,
                """
                # 本项目认证实现解读

                这个学习网站本身就是一个可运行的认证范例。对照以下文件看实现。

                ## 1. 认证配置：Program.cs

                ```csharp
                builder.Services.AddCascadingAuthenticationState();
                builder.Services.AddAuthentication(...).AddIdentityCookies();
                builder.Services.AddIdentityCore<ApplicationUser>(...)
                    .AddEntityFrameworkStores<ApplicationDbContext>()...;
                ```

                - 注册页面：`Components/Account/Pages/Register.razor`
                - 登录页面：`Components/Account/Pages/Login.razor`
                - 账户管理：`Components/Account/Pages/Manage/`

                ## 2. 用户身份 → 学习进度

                `Services/LearningProgressService.cs`：

                ```csharp
                public sealed class LearningProgressService(ApplicationDbContext db)
                {
                    // 某用户的全部已完成课程 Id
                    public async Task<List<string>> GetCompletedAsync(string userId)
                        => await db.LessonProgresses
                            .Where(p => p.UserId == userId)
                            .Select(p => p.LessonId)
                            .ToListAsync();

                    // 切换完成状态
                    public async Task<bool> ToggleAsync(string userId, string lessonId)
                    {
                        var existing = await db.LessonProgresses
                            .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == lessonId);
                        if (existing is null)
                        {
                            db.LessonProgresses.Add(new() { UserId = userId, LessonId = lessonId, CompletedAt = DateTime.UtcNow });
                        }
                        else
                        {
                            db.LessonProgresses.Remove(existing);
                        }
                        await db.SaveChangesAsync();
                        return existing is null;
                    }
                }
                ```

                ## 3. 页面如何拿 userId

                `Components/Pages/Course.razor`：

                ```csharp
                [CascadingParameter]
                private Task<AuthenticationState>? AuthState { get; set; }

                private string? UserId => AuthState?.Result.User
                    .FindFirst(ClaimTypes.NameIdentifier)?.Value;
                ```

                未登录用户看到「登录后记录进度」提示，登录后按钮变为「标记完成/撤销完成」。

                ## 4. 数据表

                - `AspNetUsers`：用户
                - `LessonProgresses`：`(UserId, LessonId)` 唯一，一人一课一条记录

                ## 5. 扩展点

                - 加课程表：把 `LessonId` 换成外键到课程表，即可做「完成时间统计」。
                - 加角色：给管理员开「课程编辑」权限。
                - 换 JWT：仅需把 Cookie 换 JWT，Blazor Server 端鉴权逻辑不变。

                ## 自测

                - 学习进度表为什么给 (UserId, LessonId) 加唯一索引？
                - 把「完成」变「撤销」的逻辑在哪个方法？
                """
            ),
        ]);
}
