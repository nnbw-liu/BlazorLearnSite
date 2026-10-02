namespace BlazorLearnSite.Data;

/// <summary>模块四：三方库如何引用。</summary>
public static partial class Catalog
{
    public static LearningModule Libraries { get; } = new(
        Id: "libraries",
        Order: 4,
        Title: "三方库如何引用",
        Summary: "从 NuGet 包管理到组件库实战接入、JS 库封装、HTTP 客户端、日志库，掌握引入外部能力的完整套路。",
        Icon: "bi-box-seam",
        Lessons:
        [
            new Course("lib-01", "NuGet 包管理基础", "添加/更新/移除包、版本选择、私有源。", 20,
                """
                # NuGet 包管理基础

                ## 1. 三种添加方式

                ```bash
                # CLI
                dotnet add package MudBlazor
                dotnet add package Serilog.AspNetCore --version 8.0.1     # 指定版本
                dotnet add package MyLib --prerelease                     # 预发布版

                # 或者用 Visual Studio：右键项目 → 管理 NuGet 程序包
                # 或者直接编辑 .csproj
                ```

                ```xml
                <!-- .csproj 里手动加 -->
                <ItemGroup>
                    <PackageReference Include="Markdig" Version="1.4.0" />
                </ItemGroup>
                ```

                ## 2. 版本选择原则

                - 生产项目用**稳定版**（不带 -preview/-rc）。
                - 大版本升级（如 7.x → 8.x）先看 release notes 再升。
                - `dotnet list package --outdated` 查看可升级的包。
                - 关注目标框架兼容性：`net10.0` 项目用支持 net10.0 的包版本。

                ## 3. 管理操作

                ```bash
                dotnet list package                         # 查看已安装
                dotnet list package --outdated              # 查看可更新
                dotnet remove package PackageName           # 移除
                dotnet restore                              # 恢复全部依赖
                ```

                ## 4. 私有源与配置文件

                `nuget.config`：

                ```xml
                <?xml version="1.0" encoding="utf-8"?>
                <configuration>
                  <packageSources>
                    <clear />
                    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
                    <add key="company" value="https://pkgs.dev.azure.com/xxx/_packaging/yyy/nuget/v3/index.json" />
                  </packageSources>
                </configuration>
                ```

                ## 自测

                - `dotnet add package` 默认装最新稳定版还是最新版（含预览）？
                - 公司内部包源怎么配置？
                """
            ),
            new Course("lib-02", "组件库接入实战：MudBlazor 完整步骤", "一步一步把 MudBlazor 接入项目并验证可用。", 30,
                """
                # 组件库接入实战：MudBlazor 完整步骤

                ## 第 1 步：安装包

                ```bash
                dotnet add package MudBlazor
                ```

                ## 第 2 步：注册服务

                ```csharp
                // Program.cs
                using MudBlazor.Services;
                builder.Services.AddMudServices();
                ```

                ## 第 3 步：注册静态资源

                ```html
                <!-- App.razor 或布局的 <head> 里 -->
                <link href="_content/MudBlazor/MudBlazor.min.css" rel="stylesheet" />

                <!-- 布局底部 -->
                <script src="_content/MudBlazor/MudBlazor.min.js"></script>
                ```

                > `_content/{包名}/` 是 Razor 类库静态资源的固定路径。

                ## 第 4 步：写一个组件验证

                ```razor
                @page "/mud-demo"

                <MudContainer>
                    <MudButton Variant="Variant.Filled" Color="Color.Primary" OnClick="Increment">
                        @($"点击了 {count} 次")
                    </MudButton>
                    <MudProgressLinear Value="count" Max="10" />
                </MudContainer>

                @code {
                    private int count;
                    private void Increment() => count++;
                }
                ```

                ## 第 5 步：常见组件速查

                ```razor
                <MudTextField @bind-Value="name" Label="姓名" />
                <MudSelect @bind-Value="city" Label="城市"> ... </MudSelect>
                <MudTable Items="@users" Hover="true" />          @* 表格 *@
                <MudDialog />                                      @* 对话框 *@
                <MudSnackbar />                                    @* 消息提示 *@
                ```

                ## 第 6 步：常见问题

                | 症状 | 原因与处理 |
                |---|---|
                | 样式错乱 | css 未注册；与 bootstrap 同时用时需调主题 |
                | 组件无交互 | js 未注册或放在 `<body>` 外 |
                | 颜色全灰 | 缺少 `MudThemeProvider` 或主题未应用 |
                | 版本冲突 | 包版本与目标框架不匹配，检查 release notes |

                ## 自测

                - Razor 类库的静态资源路径规律是什么？
                - MudBlazor 的组件无交互，先查哪里？
                """
            ),
            new Course("lib-03", "引用 JS 库：以 Chart.js 为例", "把第三方 JS 库封装成可复用 Blazor 组件。", 30,
                """
                # 引用 JS 库：以 Chart.js 为例

                Blazor 项目可以用任何 JS 库，关键是把「JS 调用」封装进 C# 组件。

                ## 1. 放库文件

                ```
                wwwroot/
                └─ lib/
                   └─ chartjs/
                      └─ chart.umd.min.js        # 从官网/CDN 下载的压缩版
                ```

                ## 2. 页面引入

                ```razor
                @* Chart.razor *@
                <canvas id="@Id"></canvas>

                <script src="lib/chartjs/chart.umd.min.js"></script>
                ```

                ## 3. 封装互操作（.razor.js 模块，.NET 8+ 推荐）

                ```javascript
                // Chart.razor.js
                export function create(canvasId, config) {
                    const ctx = document.getElementById(canvasId);
                    return new Chart(ctx, config);          // 返回实例给 C# 持有
                }
                export function update(chart, config) {
                    chart.data = config.data;
                    chart.update();
                }
                export function destroy(chart) {
                    chart.destroy();
                }
                ```

                ```csharp
                // Chart.razor.cs（与 .razor 同名分部类）
                using Microsoft.JSInterop;

                public partial class Chart : IAsyncDisposable
                {
                    [Parameter] public string Title { get; set; } = "";
                    [Parameter] public IReadOnlyList<int> Values { get; set; } = [];

                    private IJSObjectReference? _js;
                    private IJSObjectReference? _chart;

                    protected override async Task OnAfterRenderAsync(bool firstRender)
                    {
                        if (firstRender)
                        {
                            _js = await JS.InvokeAsync<IJSObjectReference>(
                                "import", "./Components/Chart.razor.js");
                            _chart = await _js.InvokeAsync<IJSObjectReference>("create", Id, MakeConfig());
                        }
                    }

                    protected override async Task OnParametersSetAsync()
                    {
                        if (_chart is not null)
                            await _js.InvokeVoidAsync("update", _chart, MakeConfig());
                    }

                    private object MakeConfig() => new
                    {
                        type = "bar",
                        data = new { labels = new[] { "一月", "二月", "三月" }, datasets = new[] { new { label = Title, data = Values } } }
                    };

                    public async ValueTask DisposeAsync()
                    {
                        if (_js is not null)
                        {
                            if (_chart is not null) await _js.InvokeVoidAsync("destroy", _chart);
                            await _js.DisposeAsync();
                        }
                    }
                }
                ```

                > `.razor.js` 文件与组件同名放一起，使用 `import()` 动态加载，这是 .NET 8+ 官方推荐的互操作隔离方式。

                ## 4. 通用步骤总结

                1. 下载库文件到 `wwwroot/lib/`。
                2. 在需要处 `<script>` 引入或动态 `import()`。
                3. C# 侧调用 `IJSRuntime` 封装 API。
                4. 必要时用 `DotNetObjectReference` 让 JS 回调 C#。
                5. 记得在 `DisposeAsync` 里释放 JS 对象。

                ## 自测

                - `.razor.js` 相比普通 `wwwroot/js` 文件的优势？
                - 为什么要在 `DisposeAsync` 里销毁 Chart 实例？
                """
            ),
            new Course("lib-04", "数据访问库：HttpClient / REST / GraphQL", "IHttpClientFactory、典型 REST 调用、GraphQL 客户端。", 30,
                """
                # 数据访问库：HttpClient / REST / GraphQL

                ## 1. IHttpClientFactory：正确创建 HttpClient

                不要 `new HttpClient()`（socket 泄漏、DNS 不刷新）。用工厂：

                ```csharp
                // Program.cs
                builder.Services.AddHttpClient("api", client =>
                {
                    client.BaseAddress = new Uri("https://api.example.com");
                    client.Timeout = TimeSpan.FromSeconds(30);
                });
                ```

                ```razor
                @inject IHttpClientFactory Factory
                @code {
                    private async Task Load()
                    {
                        var http = Factory.CreateClient("api");
                        var users = await http.GetFromJsonAsync<List<User>>("/users");
                    }
                }
                ```

                ## 2. 典型 REST 操作

                ```csharp
                var http = Factory.CreateClient("api");

                var user = await http.GetFromJsonAsync<User>($"/users/{id}");
                var resp = await http.PostAsJsonAsync("/users", newUser);
                var updated = await http.PutAsJsonAsync($"/users/{id}", user);
                await http.DeleteAsync($"/users/{id}");
                ```

                需要 `using System.Net.Http.Json;`。

                ## 3. 错误处理

                ```csharp
                var resp = await http.GetAsync("/users");
                if (!resp.IsSuccessStatusCode)
                {
                    Logger.LogWarning("接口失败：{Status}", resp.StatusCode);
                    return;
                }
                var data = await resp.Content.ReadFromJsonAsync<List<User>>();
                ```

                ## 4. GraphQL：Strawberry Shake

                ```bash
                dotnet new tool-manifest
                dotnet tool install StrawberryShake.Tools
                dotnet add package StrawberryShake
                ```

                ```bash
                # 从 schema 生成客户端
                dotnet graphql init https://api.example.com/graphql -n ApiClient
                ```

                组件里注入生成的服务，强类型查询：

                ```csharp
                @inject IApiClient Api
                var result = await Api.GetUsers.ExecuteAsync();
                ```

                ## 5. 数据源选择建议

                | 需求 | 方案 |
                |---|---|
                | 调用现有 REST API | IHttpClientFactory + `System.Net.Http.Json` |
                | 与后端同进程共享模型 | 直接注入服务（不用 HTTP 绕圈） |
                | 复杂图查询/多端共用 | GraphQL（Strawberry Shake） |
                | 高性能流式 | gRPC（Grpc.Net.Client） |

                ## 自测

                - 为什么不建议直接 `new HttpClient()`？
                - 同一项目内调用自己的服务，有必要走 HTTP 吗？
                """
            ),
            new Course("lib-05", "日志库：Serilog 接入", "结构化日志、写入文件与控制台、输出模板。", 20,
                """
                # 日志库：Serilog 接入

                ## 1. 安装

                ```bash
                dotnet add package Serilog.AspNetCore
                ```

                ## 2. Program.cs 配置

                ```csharp
                using Serilog;

                // 在创建 builder 之前配置
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .WriteTo.Console(outputTemplate:
                        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .WriteTo.File("logs/app-.log",
                        rollingInterval: RollingInterval.Day,      // 每天一个文件
                        retainedFileCountLimit: 30)
                    .CreateLogger();

                var builder = WebApplication.CreateBuilder(args);
                builder.Host.UseSerilog();                          // 接入宿主
                ```

                ## 3. 组件里使用

                ```csharp
                @inject ILogger<Home> Logger

                Logger.LogInformation("用户 {User} 访问首页", name);
                Logger.LogError(ex, "加载课程失败：{LessonId}", id);
                ```

                ## 4. 结构化字段的价值

                日志输出为 JSON 或字段化后，可以被 Loki / Elasticsearch / Seq 采集，按 `{User}`、`{LessonId}` 检索，而不是全文 grep 字符串。

                ## 5. 生产建议

                - 敏感信息（密码、Token）绝不打日志。
                - 日志级别：Debug（调试）< Information（默认）< Warning < Error < Fatal。
                - 大日志用 `WriteTo.Seq()` / `WriteTo.Http()` 送集中式日志。

                ## 自测

                - Serilog 相比 `Console.WriteLine` 的优势？
                - 日志按天滚动靠哪个配置？
                """
            ),
            new Course("lib-06", "引用三方库的常见坑", "版本冲突、静态资源、互操作时序、WASM 修剪。", 25,
                """
                # 引用三方库的常见坑

                ## 1. 版本冲突

                **症状**：编译报 `NU1605` / 运行 `FileLoadException`。
                **处理**：

                ```bash
                dotnet list package --include-transitive   # 查看传递依赖
                dotnet nuget why BlazorLearnSite SomePackage   # 分析冲突来源（.NET 9+）
                ```

                统一版本：在 Directory.Packages.props 集中管理（Central Package Management）。

                ## 2. 静态资源 404

                - Razor 类库资源路径必须是 `_content/{包名}/{文件}`。
                - 检查 `<script>` 是否放到了渲染后的 `<body>` 里。
                - 发布后验证：直接访问资源 URL 看 404 还是 200。

                ## 3. JS 互操作时序

                - 组件 `OnInitialized` 阶段 **没有** JS 环境 → 调用会抛错。
                - 一切 JS 调用放 `OnAfterRender(firstRender: true)`。
                - 预渲染阶段同样受限，用 `if (OperatingSystem.IsBrowser())` 或延迟到 AfterRender。

                ## 4. WASM 模式修剪

                发布时 `PublishTrimmed=true` 会裁掉未引用代码，反射调用（如 `JsonSerializer` 反射、部分 UI 库）可能被误裁：

                ```xml
                <PropertyGroup>
                    <PublishTrimmed>true</PublishTrimmed>
                    <SuppressTrimAnalysisWarnings>false</SuppressTrimAnalysisWarnings>
                </PropertyGroup>
                ```

                出问题先关闭修剪验证，再按警告逐个加 `[DynamicallyAccessedMembers]` 或 trimmer root。

                ## 5. 升级大版本

                - 先看 **release notes** 的 breaking changes 列表。
                - 用 Git 分支升级，跑通测试再合并。
                - 关注官方迁移指南（如 MudBlazor 8 → MudBlazor 9 的 API 变更）。

                ## 自测

                - 两个包依赖同一个库的不同版本，先执行哪条命令排查？
                - WASM 发布后某个组件白屏，第一怀疑对象是什么？
                """
            ),
        ]);
}
