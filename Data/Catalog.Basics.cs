namespace BlazorLearnSite.Data;

/// <summary>模块一：Blazor 基础学习路径。</summary>
public static partial class Catalog
{
    public static LearningModule Basics { get; } = new(
        Id: "basics",
        Order: 1,
        Title: "Blazor 基础学习路径",
        Summary: "从零上手 Blazor：组件模型、数据绑定、生命周期、路由、表单、依赖注入，搭建起用 C# 写前端的完整认知。",
        Icon: "bi-mortarboard",
        Lessons:
        [
            new Course("basic-01", "什么是 Blazor：四种运行模式", "认识 Blazor 的定位与 Server / WebAssembly / Auto / SSR 四种模式，知道该选谁。", 20,
                """
                # 什么是 Blazor：四种运行模式

                Blazor 是微软推出的 **用 C# 替代 JavaScript 编写 Web UI** 的框架。组件用 Razor 语法编写，业务逻辑用 C# 编写，前后端共享同一套语言、类库和工具链。

                ## 核心思想：组件

                Blazor 的一切都是**组件（Component）**：一个 `.razor` 文件 + 可选 `.razor.cs` 代码文件。组件可以嵌套、传参、触发事件，最终渲染成 HTML。

                ```razor
                @* Counter.razor —— 一个最简单的组件 *@
                @code {
                    private int count = 0;
                }

                <p>当前计数：@count</p>
                <button class="btn btn-primary" @onclick="() => count++">+1</button>
                ```

                上面的组件渲染出：`当前计数：0` 和一个按钮。点击按钮后，Blazor 自动重渲染并更新 DOM，无需手动操作 DOM。

                ## 四种运行模式（.NET 8 起统称 RenderMode）

                | 模式 | 运行位置 | 特点 | 适用场景 |
                |---|---|---|---|
                | **Static SSR** | 服务器 | 只输出静态 HTML，无交互 | 内容站、SEO 优先页面 |
                | **Interactive Server** | 服务器（SignalR） | 实时交互，客户端很轻 | 后台系统、企业内部应用 |
                | **Interactive WebAssembly** | 浏览器 | 完全离线可用，可部署 CDN | 工具型应用、跨平台体验 |
                | **Interactive Auto** | 先服务器后 WASM | 首次秒开，之后客户端运行 | 兼顾体验与速度的公共应用 |

                > 判断口诀：**要不要实时、要不要离线、首屏快不快**。后台管理系统几乎无脑选 Server；面向公众的大流量应用选 Auto 或 WASM。

                ## 一个 Web 应用的组成

                ```
                项目根
                ├─ Program.cs           入口：配置服务与请求管道
                ├─ Components/
                │  ├─ App.razor        应用根组件（渲染 Routes）
                │  ├─ Routes.razor     路由表
                │  ├─ Layout/          布局组件
                │  └─ Pages/           页面组件（带 @page 路由）
                ├─ wwwroot/            静态资源（css/js/图片）
                └─ Data/               数据模型与数据库上下文
                ```

                ## 自测

                - 用一句话向同事解释 Blazor 和传统 MVC 的区别。
                - 后台管理系统你会选哪种渲染模式？为什么？
                """
            ),
            new Course("basic-02", "环境准备与创建第一个应用", "安装 SDK、用模板创建项目、看懂启动过程。", 20,
                """
                # 环境准备与创建第一个应用

                ## 1. 安装 .NET SDK

                到 [dotnet.microsoft.com/download](https://dotnet.microsoft.com/download) 下载 **.NET 8 及以上**（当前 LTS 为 .NET 8 / .NET 10）。验证安装：

                ```bash
                dotnet --list-sdks
                # 9.0.312 ...
                # 10.0.401 ...
                ```

                ## 2. 用模板创建项目

                ```bash
                # 创建 Blazor Web App（默认 Server 交互模式）
                dotnet new blazor -n MyFirstBlazor -o MyFirstBlazor
                cd MyFirstBlazor

                # 常用选项
                # -int Server | WebAssembly | Auto | None   选择交互模式
                # -au Individual                            启用个人账户认证
                # -ai                                     所有页面默认交互

                # 运行
                dotnet run
                ```

                浏览器打开 `https://localhost:5001`（或终端提示的地址），你会看到模板自带的首页、Counter 和 Weather 页面。

                ## 3. 看懂启动过程（Program.cs）

                ```csharp
                var builder = WebApplication.CreateBuilder(args);

                // 注册 Razor 组件与 Server 交互能力
                builder.Services.AddRazorComponents()
                    .AddInteractiveServerComponents();

                var app = builder.Build();

                // 请求管道：异常处理、HTTPS、静态资源、组件路由
                app.UseExceptionHandler("/Error");
                app.UseHttpsRedirection();
                app.MapStaticAssets();                    // 静态资源（wwwroot）
                app.MapRazorComponents<App>()             // 组件路由入口
                    .AddInteractiveServerRenderMode();    // 声明 Server 渲染模式

                app.Run();
                ```

                `Components/App.razor` 是应用的根组件，它把请求路由到 `Routes.razor`，最终渲染匹配到的页面组件：

                ```razor
                @* App.razor *@
                <Routes />
                <HeadOutlet />
                ```

                ## 4. 三种常用命令

                ```bash
                dotnet run        # 本地运行（开发）
                dotnet build      # 编译检查
                dotnet publish    # 发布可部署产物
                ```

                ## 自测

                - `dotnet new blazor` 生成的项目里，`App.razor` 和 `Routes.razor` 各负责什么？
                - 运行 `dotnet run` 后访问地址是什么？端口在哪里配置？
                """
            ),
            new Course("basic-03", "组件基础与 Razor 语法", "组件的三种写法、@page/@code/@inject 等指令、参数传递。", 30,
                """
                # 组件基础与 Razor 语法

                ## 1. 组件的三种写法

                **写法 A：单文件（.razor）** —— 模板与逻辑在一起，适合小组件。

                ```razor
                @* Greeting.razor *@
                @code {
                    [Parameter] public string Name { get; set; } = "世界";
                }
                <p>你好，@Name！</p>
                ```

                **写法 B：代码分离（.razor + .razor.cs）** —— 大组件推荐，模板与逻辑分开。

                ```razor
                @* Counter.razor *@
                <p>当前值：@Count</p>
                <button @onclick="Increment">+1</button>
                ```

                ```csharp
                // Counter.razor.cs —— 与 .razor 同名的分部类
                public partial class Counter
                {
                    public int Count { get; set; }
                    private void Increment() => Count++;
                }
                ```

                **写法 C：泛型组件 / 代码生成组件（纯 C#）** —— 不常见，跳过。

                ## 2. 核心指令速查

                | 指令 | 作用 | 示例 |
                |---|---|---|
                | `@page "/url"` | 声明路由，组件可被 URL 访问 | `@page "/counter"` |
                | `@code { }` | 组件逻辑代码块 | 字段、方法、属性 |
                | `@inject IService svc` | 注入服务 | `@inject NavigationManager Nav` |
                | `@bind` | 双向数据绑定 | `@bind="Name"` |
                | `@onclick` 等 | 事件绑定 | `@onclick="HandleClick"` |
                | `@if / @foreach` | 条件与循环 | `@if (items.Any())` |
                | `@* 注释 *@` | Razor 注释（不输出到页面） | — |

                ## 3. 参数传递：@Parameter

                父组件给子组件传值，子组件声明 `[Parameter]`：

                ```razor
                @* 父组件使用子组件 *@
                <Greeting Name="张三" />
                <Greeting Name="李四" />

                @* Greeting.razor 接收 *@
                @code {
                    [Parameter] public string Name { get; set; } = "默认";
                }
                ```

                ## 4. 表达式与插值

                ```razor
                <p>@DateTime.Now.ToString("yyyy-MM-dd HH:mm")</p>
                <p class="@(isActive ? "active" : "")">动态 class</p>
                <img src="@($"/images/{pic}.png")" />
                ```

                ## 自测

                - `@page` 与 `[Parameter]` 分别解决什么问题？
                - 把 Counter 改造成代码分离写法需要哪两个文件？
                """
            ),
            new Course("basic-04", "数据绑定与事件处理", "单向/双向绑定、@bind 的语法糖、事件参数与异步事件。", 30,
                """
                # 数据绑定与事件处理

                ## 1. 单向绑定：表达式插值

                UI 每次重渲染时读取表达式的值：

                ```razor
                <p>当前时间：@DateTime.Now</p>
                ```

                ## 2. 双向绑定：@bind

                `@bind` 是「显示值 + 值变化时写回」的语法糖，默认绑定 `onchange` 事件（输入框失焦/回车时更新）：

                ```razor
                <input @bind="name" />
                <p>你好，@name</p>

                @code {
                    private string name = "";
                }
                ```

                等价写法：

                ```razor
                <input value="@name" @onchange="e => name = e.Value?.ToString() ?? "" />
                ```

                **即时更新**（每敲一个字符都更新）用 `@bind:event="oninput"`：

                ```razor
                <input @bind="name" @bind:event="oninput" />
                ```

                **格式化**（如金额）用 `@bind:format`：

                ```razor
                <input @bind="price" @bind:format="F2" />
                ```

                > `@bind` 只对属性/字段生效；要绑定方法返回值请手动拆成 value + @onchange。

                ## 3. 事件处理

                ```razor
                <button @onclick="HandleClick">点击</button>
                <button @onclick="() => count++">匿名方法</button>
                <button @onclick="(e) => Console.WriteLine(e.Button)">事件参数</button>

                @code {
                    private void HandleClick() => count++;
                    private async Task HandleAsync() { await Task.Delay(100); count++; }
                    private int count;
                }
                ```

                常用事件：`@onclick`、`@oninput`、`@onchange`、`@onkeydown`、`@onsubmit`（配合表单）、`@onmouseenter` 等。所有事件都支持同步方法与 `async Task` 方法。

                ## 4. 阻止默认行为与事件冒泡

                ```razor
                @* 阻止表单默认提交 *@
                <form @onsubmit="HandleSubmit" @onsubmit:preventDefault="true">
                ```

                ## 自测

                - `@bind` 默认在什么事件写回值？如何改成输入即更新？
                - 事件处理器返回 `Task` 和返回 `void` 有什么区别？
                """
            ),
            new Course("basic-05", "组件生命周期", "OnInitialized / OnParametersSet / OnAfterRender / OnDispose 的时机与典型用途。", 25,
                """
                # 组件生命周期

                组件有一套明确的生命周期方法，掌握时机是调试 Blazor 的关键。

                ## 生命周期总览

                ```
                创建实例
                └─ SetParametersAsync（接收父组件传来的参数）
                   ├─ OnInitialized / OnInitializedAsync    首次初始化（只执行一次）
                   ├─ OnParametersSet / OnParametersSetAsync  每次收到参数都执行
                   └─ OnAfterRender / OnAfterRenderAsync      渲染完成后（可操作 DOM/JS）
                运行中：父组件重渲染 → 再次 OnParametersSet → 重渲染
                销毁：OnDispose / OnDisposeAsync（释放资源）
                ```

                ```csharp
                public partial class WeatherCard : ComponentBase
                {
                    [Parameter] public string City { get; set; } = "";

                    protected override async Task OnInitializedAsync()
                    {
                        // 只执行一次：加载数据、订阅事件
                        await LoadAsync();
                    }

                    protected override void OnParametersSet()
                    {
                        // 每次参数变化执行：参数变化后的重算
                        Title = $"天气：{City}";
                    }

                    protected override async Task OnAfterRenderAsync(bool firstRender)
                    {
                        // 渲染后：操作 DOM、调用 JS、初始化第三方控件
                        if (firstRender)
                        {
                            await JS.InvokeVoidAsync("initChart");
                        }
                    }

                    public void Dispose()
                    {
                        // 释放资源、退订事件
                        timer?.Dispose();
                    }
                }
                ```

                ## 关键认知

                - **OnInitialized 只执行一次**；OnParametersSet 在「首次 + 每次父组件重渲染传参」时执行。
                - **OnAfterRender 里才能安全操作 DOM / 调用 JS**，因为此时浏览器已渲染。
                - 手动触发重渲染用 `StateHasChanged()`；`Task.Delay`/事件回调后框架会自动重渲染。
                - 大量初始化工作放 `OnInitializedAsync` 前，考虑「异步加载 + 加载动画」避免白屏。

                ## 自测

                - 想在页面第一次渲染后调用 JS 初始化图表，应该写在哪个方法？
                - 父组件重渲染时，子组件哪些生命周期会再次执行？
                """
            ),
            new Course("basic-06", "路由与导航", "@page 路由规则、NavLink 高亮、NavigationManager 编程式导航、路由参数。", 25,
                """
                # 路由与导航

                ## 1. 路由声明与参数

                ```razor
                @* Course.razor *@
                @page "/lesson/{id}"
                @page "/lesson/{id:int}"      @* 可以声明多个路由 *@

                <h3>课程：@Id</h3>

                @code {
                    [Parameter] public string Id { get; set; } = "";
                }
                ```

                路由约束：`{id:int}`、`{id:guid}`、`{id:bool}`、`{name:alpha}`，默认是字符串。

                **可选参数**与**通配路由**（.NET 7+）：

                ```razor
                @page "/search/{term?}"                 @* 可选 *@
                @page "/{**path}"                       @* 匹配所有未命中路径 *@
                ```

                ## 2. 导航链接：NavLink

                ```razor
                @* 自动高亮当前页 *@
                <NavLink class="nav-link" href="/lesson/basic-01">第 1 课</NavLink>
                <NavLink Match="NavLinkMatch.Prefix" href="/module/basics">模块页（前缀匹配高亮）</NavLink>
                ```

                ## 3. 编程式导航：NavigationManager

                ```csharp
                @inject NavigationManager Nav

                Nav.NavigateTo("/lesson/basic-02");                    // 页面内跳转
                Nav.NavigateTo("/Account/Login");                      // 走完整请求（外部页）
                Nav.NavigateTo("/", forceLoad: true);                  // 强制整页刷新
                Nav.NavigateTo(Nav.Uri);                               // 刷新当前页
                ```

                ## 4. 查询参数

                ```csharp
                @inject NavigationManager Nav
                @code {
                    protected override void OnInitialized()
                    {
                        var q = new Uri(Nav.Uri)
                            .Query
                            .Split('&', StringSplitOptions.RemoveEmptyEntries)
                            .Select(p => p.Split('='))
                            .ToDictionary(k => k[0], v => Uri.UnescapeDataString(v[1]));
                        if (q.TryGetValue("page", out var page)) Page = int.Parse(page);
                    }
                }
                ```

                ## 自测

                - 如何让 NavLink 在「前缀匹配」时高亮？
                - 从代码里跳转到另一个组件页面用哪个对象的方法？
                """
            ),
            new Course("basic-07", "表单与数据校验", "EditForm、DataAnnotations 校验、Input 系列组件、自定义校验。", 35,
                """
                # 表单与数据校验

                Blazor 的表单基于 **EditForm + DataAnnotations**，与 MVC 的模型校验一脉相承。

                ## 1. 最小表单

                ```razor
                @using System.ComponentModel.DataAnnotations

                <EditForm Model="@user" OnValidSubmit="Save">
                    <DataAnnotationsValidator />
                    <ValidationSummary />

                    <div>
                        <label>用户名</label>
                        <InputText @bind-Value="user.Name" />
                        <ValidationMessage For="() => user.Name" />
                    </div>

                    <button type="submit">保存</button>
                </EditForm>

                @code {
                    private UserModel user = new();

                    private void Save() { /* 校验通过后执行 */ }

                    public class UserModel
                    {
                        [Required, StringLength(20, MinimumLength = 3)]
                        public string Name { get; set; } = "";
                        [EmailAddress]
                        public string Email { get; set; } = "";
                    }
                }
                ```

                ## 2. 关键点

                - `DataAnnotationsValidator`：把 DataAnnotations 特性变成校验规则。
                - `ValidationSummary`：集中显示全部错误；`ValidationMessage`：单字段错误。
                - `OnValidSubmit` 校验通过触发；`OnInvalidSubmit` 校验失败触发。
                - **EditForm 会输出 `antiforgery token`**，默认内置，无需手动处理。

                ## 3. 内置输入组件

                | 组件 | 说明 |
                |---|---|
                | `InputText` | 单行文本 |
                | `InputNumber` | 数字 |
                | `InputSelect` | 下拉选择 |
                | `InputCheckbox` | 复选框 |
                | `InputDate` | 日期 |
                | `InputRadio` / `InputRadioGroup` | 单选 |
                | `InputFile` | 文件上传 |

                它们都绑定 `@bind-Value`，自动显示校验错误样式。

                ## 4. 异步校验与自定义验证

                ```csharp
                public class UniqueUsername : ValidationAttribute
                {
                    protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
                    {
                        return value?.ToString() == "admin"
                            ? new ValidationResult("该用户名已被占用")
                            : ValidationResult.Success;
                    }
                }
                ```

                ```csharp
                // 表单级自定义校验
                private void ValidateAll(EditContext ctx)
                {
                    if (user.Password != user.Confirm)
                        ctx.AddValidationMessage("确认密码不一致");
                }
                // <EditForm OnValidSubmit="Save" OnValidationRequested="ValidateAll">
                ```

                ## 自测

                - `OnValidSubmit` 和 `OnSubmit` 的区别？
                - 要给密码框加「两次输入一致」校验，用特性还是表单级校验？
                """
            ),
            new Course("basic-08", "依赖注入", "DI 的三种生命周期、@inject 与代码注入、OwningComponentScope、按需注册。", 25,
                """
                # 依赖注入

                Blazor 直接使用 ASP.NET Core 的 DI 容器，在 `Program.cs` 注册服务，在组件里注入。

                ## 1. 注册与生命周期

                ```csharp
                // Program.cs
                builder.Services.AddSingleton<IMemoryCache>(_ => new MemoryCache(new MemoryCacheOptions()));
                builder.Services.AddScoped<WeatherService>();
                builder.Services.AddTransient<IRandomGenerator, RandomGenerator>();
                ```

                | 生命周期 | Blazor Server 下的含义 |
                |---|---|
                | **Singleton** | 全局唯一，所有用户共享 |
                | **Scoped** | **每个用户会话（SignalR Circuit）一个**——Blazor Server 里等同「按用户」 |
                | **Transient** | 每次请求（每次注入）新建 |

                > ⚠️ Blazor Server 的 `Scoped` ≠ Web API 的「每请求一次」，而是「每个用户连接一次」。理解这点才能正确管理状态。

                ## 2. 组件内注入

                ```razor
                @* 方式一：Razor 指令 *@
                @inject WeatherService Weather
                @inject NavigationManager Nav

                @code {
                    // 方式二：代码注入（构造注入需使用 DI 创建组件，不推荐）
                    [Inject] private WeatherService Weather { get; set; } = default!;
                }
                ```

                `@inject` 是最常用方式，等价于 `[Inject]` 属性。

                ## 3. OwningComponentScope：组件私有作用域

                组件销毁时，它的 Scoped 依赖不会自动释放。若某组件使用大对象（如 `HttpClient`、EF `DbContext` 之外的重量级服务），用 `OwningComponentScope` 让组件拥有自己的作用域：

                ```csharp
                @implements IDisposable
                @inject IServiceScopeFactory ScopeFactory
                @code {
                    private OwningComponentScope? scope;
                    private MyHeavyService? svc;

                    protected override void OnInitialized()
                    {
                        scope = ScopeFactory.CreateScope();      // 需要 using Microsoft.Extensions.DependencyInjection
                        svc = scope.ServiceProvider.GetRequiredService<MyHeavyService>();
                    }

                    public void Dispose() => scope?.Dispose();   // 组件销毁时释放
                }
                ```

                ## 4. 常见用法

                ```csharp
                // 注入 HttpClient 访问 Web API
                builder.Services.AddHttpClient("api", c => c.BaseAddress = new Uri("https://api.example.com"));
                // 组件内：@inject IHttpClientFactory HttpFactory
                ```

                ## 自测

                - 想在「每个登录用户之间隔离状态」，注册成什么生命周期？
                - `@inject` 与 `[Inject]` 是两种不同机制还是等价写法？
                """
            ),
        ]);
}
