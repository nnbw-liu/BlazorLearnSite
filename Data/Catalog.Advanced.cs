namespace BlazorLearnSite.Data;

/// <summary>模块二：Blazor 扩展学习。</summary>
public static partial class Catalog
{
    public static LearningModule Advanced { get; } = new(
        Id: "advanced",
        Order: 2,
        Title: "Blazor 扩展学习",
        Summary: "渲染模式深度、JS 互操作、组件通信、状态管理、性能优化、国际化、错误处理与测试，把组件开发推向工程级。",
        Icon: "bi-stack",
        Lessons:
        [
            new Course("adv-01", "渲染模式深度与流式渲染", "SSR/Server/WASM/Auto 的切换方式、流式渲染与预渲染原理。", 30,
                """
                # 渲染模式深度与流式渲染

                ## 1. 渲染模式怎么指定

                组件级指定（.NET 8+），可以按页面甚至按组件混合：

                ```razor
                @* 页面级 *@
                @page "/products"
                @rendermode InteractiveServer            @* 该页面用 Server 交互 *@

                @* 组件级（默认 SSR） *@
                <ProductTable @rendermode="InteractiveAuto" />   @* 仅此组件交互 *@
                ```

                在 App 根组件上用 `@rendermode InteractiveServer` 可以让全站交互（本项目即如此）。

                ## 2. 预渲染（Prerendering）

                首次请求时，Blazor 先在服务器端把组件渲染成静态 HTML 发给浏览器（首屏秒开、利于 SEO），随后 SignalR 建立连接接管交互。这就是「预渲染 + 交互升级」。

                ```csharp
                // Program.cs：给整个应用启用 Server 交互（含预渲染）
                app.MapRazorComponents<App>()
                    .AddInteractiveServerRenderMode();
                ```

                ⚠️ 预渲染时**没有**浏览器环境：`OnInitializedAsync` 里调用 JS 会失败，应把 JS 调用放 `OnAfterRender(firstRender: true)`。

                ## 3. 流式渲染（Streaming Rendering）

                长耗时页面（如报表）不必等全部数据，先把骨架 HTML 发出去，数据到达后再增量更新：

                ```razor
                @attribute [StreamRendering(true)]
                ```

                ```csharp
                protected override async Task OnInitializedAsync()
                {
                    await Task.Delay(3000);   // 模拟慢接口：页面先显示占位，3 秒后填充
                    items = Enumerable.Range(1, 10).ToList();
                }
                ```

                实际效果：浏览器立即显示「加载中…」，数据就绪后自动刷新。

                ## 4. Server 模式的技术原理

                ```
                浏览器 ←→ SignalR（WebSocket 长连接）←→ 服务器上的 Circuit
                每次交互：事件 → 服务器重渲染组件 → 计算 DOM diff → 增量下发
                ```

                - 组件状态存在服务器内存的 Circuit 里，**刷新页面/断网重连会丢失**（应用层状态需持久化）。
                - 服务器内存 = 在线用户数 × 每个 Circuit 的状态，要注意容量规划。

                ## 自测

                - 预渲染阶段为什么不能安全调用 JS？
                - 报表页面加载慢，用什么特性让用户先看到页面骨架？
                """
            ),
            new Course("adv-02", "JavaScript 互操作", "C# 调 JS、JS 调 C#（[JSInvokable]）、动态加载脚本。", 30,
                """
                # JavaScript 互操作

                Blazor 允许在需要时调用 JS（比如地图、图表、浏览器 API）。

                ## 1. C# 调用 JS

                ```csharp
                @inject IJSRuntime JS

                // 无返回值
                await JS.InvokeVoidAsync("console.log", "hello");

                // 有返回值
                var width = await JS.InvokeAsync<int>("getWidth");
                var result = await JS.InvokeAsync<string>("myApp.format", "2026-01-01");
                ```

                ```javascript
                // wwwroot/js/app.js —— 在 wwwroot 下新建
                window.myApp = {
                    format: function (s) { return s.replaceAll('-', '/'); }
                };
                ```

                ```razor
                @* 页面顶部引入 *@
                <script src="js/app.js"></script>
                ```

                ## 2. JS 调用 C#：DotNetObjectReference

                ```csharp
                @inject IJSRuntime JS
                @implements IAsyncDisposable

                @code {
                    private DotNetObjectReference<Counter>? ref;
                    private int count;

                    protected override async Task OnAfterRenderAsync(bool firstRender)
                    {
                        if (firstRender)
                        {
                            ref = DotNetObjectReference.Create(this);
                            await JS.InvokeVoidAsync("myApp.attach", ref);
                        }
                    }

                    [JSInvokable]
                    public void AddOne() => count++;

                    public async ValueTask DisposeAsync()
                    {
                        if (ref is not null)
                            await JS.InvokeVoidAsync("myApp.detach", ref);
                    }
                }
                ```

                ```javascript
                window.myApp = {
                    attach: function (ref) {
                        // 从 JS 回调 C# 方法
                        document.getElementById('btn').onclick = () => ref.invokeMethodAsync('AddOne');
                    },
                    detach: function (ref) { ref.dispose(); }
                };
                ```

                ## 3. 动态加载 JS（需要时才下载）

                ```csharp
                await JS.InvokeVoidAsync("loadScript", "/lib/chart/chart.min.js");
                // 或用 Blazor 内置：await JS.InvokeVoidAsync("eval", ...) 不推荐
                ```

                更现代的做法是写一个「JS 初始化模块」封装，例如在 `wwwroot/js/myapp.js` 用 ES Module + `import()`。

                ## 4. 常见坑

                - 调用 JS 必须在 **OnAfterRender** 之后（预渲染阶段没有 JS 环境）。
                - `DotNetObjectReference` 用完后要 `Dispose()`，否则内存泄漏。
                - 方法名大小写敏感；返回复杂对象时用 JSON 序列化（`JsonSerializer`）。

                ## 自测

                - C# 里如何把一个方法暴露给 JS 调用？
                - 为什么初始化图表代码要写在 `OnAfterRenderAsync(firstRender)` 里？
                """
            ),
            new Course("adv-03", "组件通信：EventCallback / 级联参数 / RenderFragment", "子传父、跨层传值、模板化组件的三种机制。", 30,
                """
                # 组件通信：EventCallback / 级联参数 / RenderFragment

                ## 1. 子组件 → 父组件：EventCallback

                ```razor
                @* ConfirmButton.razor（子） *@
                <button @onclick="() => OnConfirm.InvokeAsync(null)">确认</button>
                @code {
                    [Parameter] public EventCallback OnConfirm { get; set; }
                }
                ```

                ```razor
                @* 父组件 *@
                <ConfirmButton OnConfirm="HandleConfirm" />
                @code {
                    private void HandleConfirm() => Console.WriteLine("用户确认了");
                }
                ```

                带参数的事件回调：`EventCallback<T>`，父组件方法签名接收 `T`。

                ## 2. 跨层传值：级联参数 CascadingValue

                父子孙多级传递时，避免逐层 `[Parameter]` 传导：

                ```razor
                <CascadingValue Value="theme" Name="Theme">
                    <ChildA />
                    <ChildB />
                </CascadingValue>
                @code { private string theme = "dark"; }
                ```

                ```razor
                @* 任意后代组件 *@
                @code {
                    [CascadingParameter(Name = "Theme")] public string Theme { get; set; } = "";
                }
                ```

                > 注意：`CascadingValue` 变化会触发**所有后代重渲染**，别滥用。适合「主题、登录状态、文化信息」这类全局值。

                ## 3. 模板化组件：RenderFragment

                让父组件自定义子组件的展示内容：

                ```razor
                @* DataList.razor：泛型模板化列表 *@
                @typeparam TItem
                @foreach (var item in Items)
                {
                    @ItemTemplate(item)
                }

                @code {
                    [Parameter] public IReadOnlyList<TItem> Items { get; set; } = [];
                    [Parameter] public RenderFragment<TItem>? ItemTemplate { get; set; }
                }
                ```

                ```razor
                @* 使用 *@
                <DataList Items="@users" Context="u">
                    <ItemTemplate>
                        <div>@u.Name —— @u.Email</div>
                    </ItemTemplate>
                </DataList>
                ```

                ## 4. 决策表

                | 场景 | 机制 |
                |---|---|
                | 父→子传值 | `[Parameter]` |
                | 子→父通知 | `EventCallback` |
                | 祖→孙传值 | `CascadingValue` |
                | 父定义子渲染内容 | `RenderFragment` |
                | 全局共享状态 | Scoped 服务（见下一课） |

                ## 自测

                - 想给三个不同层级的组件共享「当前登录主题」，用哪种机制？
                - `EventCallback<T>` 里 T 表示什么？
                """
            ),
            new Course("adv-04", "状态管理：从 Circuit 到持久化", "Scoped 服务做会话状态、状态容器、ProtectedLocalStorage 持久化。", 30,
                """
                # 状态管理：从 Circuit 到持久化

                Blazor Server 的组件状态默认只活在**当前 Circuit（用户连接）**内存里。跨页面、跨刷新、跨设备都需要显式设计。

                ## 1. 会话级状态：Scoped 服务

                ```csharp
                public class CartState
                {
                    public List<CartItem> Items { get; } = [];
                    public event Action? Changed;
                    public void Add(CartItem item)
                    {
                        Items.Add(item);
                        Changed?.Invoke();
                    }
                }
                ```

                ```csharp
                // Program.cs
                builder.Services.AddScoped<CartState>();
                ```

                ```razor
                @inject CartState Cart
                <h3>购物车 @Cart.Items.Count 件</h3>
                <button @onclick="() => Cart.Add(new CartItem())">加入</button>
                ```

                所有组件共享同一个 `CartState` 实例，刷新页面则重置。

                ## 2. 跨刷新持久化：ProtectedLocalStorage

                ```csharp
                @inject ProtectedLocalStorage Storage
                @inject CartState Cart

                protected override async Task OnInitializedAsync()
                {
                    var saved = await Storage.GetAsync<List<CartItem>>("cart");
                    if (saved.Success) Cart.Items.AddRange(saved.Value);
                }

                private async Task Save()
                {
                    await Storage.SetAsync("cart", Cart.Items);
                }
                ```

                `ProtectedLocalStorage` 存储在浏览器 localStorage 且**加密**，适合「记住我」类数据。需要注册：

                ```csharp
                // Program.cs
                builder.Services.AddBlazorWebApp(); // 或 AddRazorComponents 已内置相关服务
                ```

                ## 3. 服务端持久化：数据库

                真正的跨设备进度（如本学习网站的学习进度）要存数据库，按用户 Id 关联：

                ```csharp
                public class LearningProgressService(ApplicationDbContext db)
                {
                    public async Task<List<string>> GetCompletedAsync(string userId)
                        => await db.LessonProgresses
                            .Where(p => p.UserId == userId)
                            .Select(p => p.LessonId)
                            .ToListAsync();
                }
                ```

                ## 4. 全局事件通知（简化版状态管理）

                ```csharp
                public class NotificationService
                {
                    private readonly List<Action<string>> _subs = [];
                    public void Subscribe(Action<string> s) => _subs.Add(s);
                    public void Notify(string msg) { foreach (var s in _subs) s(msg); }
                }
                ```

                ## 自测

                - Scoped 服务在 Blazor Server 里对应什么粒度？
                - 需要「用户关掉浏览器再打开，进度还在」应该用哪种方案？
                """
            ),
            new Course("adv-05", "性能优化与 Virtualize", "减少不必要的重渲染、Virtualize 虚拟滚动、渲染模式选择。", 25,
                """
                # 性能优化与 Virtualize

                ## 1. 减少不必要的重渲染

                ```csharp
                // 只改自己关心的字段，别整体重建对象
                private void Toggle() => isOn = !isOn;   // 好
                private void Toggle() { state = new() { ... }; } // 差：每次都触发全量 diff
                ```

                - 事件处理器里避免无意义地给不相关字段赋值。
                - 大量静态内容用 `@rendermode` 分离，别让重渲染波及整页。
                - `ShouldRender()` 手动短路：

                ```csharp
                protected override bool ShouldRender() => _dirty;
                ```

                ## 2. Virtualize：只渲染可见行

                长列表（上千行）必须用 `Virtualize`，它只渲染视口内的行：

                ```razor
                <Virtualize Items="@items" Context="item">
                    <div>@item.Title</div>
                </Virtualize>
                ```

                大数据源用 `ItemProvider` 分页加载：

                ```razor
                <Virtualize Context="item" ItemProvider="LoadPage">
                    <div>@item.Title</div>
                </Virtualize>

                @code {
                    private async ValueTask<ItemsProviderResult<Item>> LoadPage(ItemsProviderRequest req)
                    {
                        var page = await Api.GetPageAsync(req.StartIndex, req.Count);
                        return new(page.Items, page.Total);
                    }
                }
                ```

                > `Virtualize` 要求行高可预测；不确定高度时给 `item-size` 估计值。

                ## 3. 异步渲染与流式渲染

                慢数据用异步加载 + 加载态，避免阻塞首帧：

                ```razor
                @if (loading)
                {
                    <div>加载中…</div>
                }
                else
                {
                    @foreach (var x in data) { <div>@x</div> }
                }
                ```

                ## 4. 打包与静态资源

                - `MapStaticAssets`（.NET 9+）自动指纹化、压缩静态资源。
                - 图片用 `loading="lazy"`；字体/大 JS 用 `defer`。
                - WASM 模式关注裁剪：`PublishTrimmed` 与 `InvariantGlobalization`。

                ## 自测

                - 一万行的表格为什么必须用 `Virtualize`？
                - `ShouldRender` 返回 false 会发生什么？
                """
            ),
            new Course("adv-06", "国际化与多语言", "IStringLocalizer、资源文件、文化切换。", 20,
                """
                # 国际化与多语言

                ## 1. 注册本地化服务

                ```csharp
                // Program.cs
                builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
                ```

                ## 2. 资源文件

                在 `Resources/` 下新建：

                - `SharedResources.resx`（默认语言）
                - `SharedResources.zh-CN.resx`
                - `SharedResources.en.resx`

                每个 resx 里写键值对，如 `Welcome=欢迎使用 Blazor 学习站`。

                ## 3. 组件内使用

                ```csharp
                public class SharedResources { }   // 用于强类型定位资源文件
                ```

                ```razor
                @using Microsoft.Extensions.Localization
                @inject IStringLocalizer<SharedResources> L

                <h1>@L["Welcome"]</h1>
                <button>@L["Save"]</button>
                ```

                ## 4. 支持文化切换

                ```csharp
                // Program.cs：请求中间件
                app.UseRequestLocalization(new RequestLocalizationOptions()
                    .AddSupportedCultures("zh-CN", "en")
                    .AddSupportedUICultures("zh-CN", "en")
                    .SetDefaultCulture("zh-CN"));
                ```

                浏览器语言、Cookie、QueryString 都会影响当前文化（默认从 `Accept-Language` 头读取）。

                ## 5. 表单与日期格式

                设置文化后，`DateTime` 的显示、`InputDate` 的解析自动跟随文化。

                ## 自测

                - 资源文件按什么规则命名才能被 `IStringLocalizer` 找到？
                - 切换语言需要改代码还是只加 resx 文件？
                """
            ),
            new Course("adv-07", "错误处理与日志", "ErrorBoundary、全局异常中间件、结构化日志。", 25,
                """
                # 错误处理与日志

                ## 1. 组件级错误边界：ErrorBoundary

                ```razor
                <ErrorBoundary>
                    <ChildComponent />
                </ErrorBoundary>
                ```

                子组件抛异常时，ErrorBoundary 显示默认错误 UI，不会整页崩溃。自定义错误界面：

                ```razor
                <ErrorBoundary>
                    <ChildComponent />
                    <ErrorContent>
                        <div class="alert alert-danger">组件出错了，请稍后重试。</div>
                    </ErrorContent>
                </ErrorBoundary>
                ```

                > 注意：Server 模式组件异常会导致**当前 Circuit 终止**并重连。ErrorBoundary 主要兜底渲染层。

                ## 2. 全局异常处理（Program.cs）

                ```csharp
                if (!app.Environment.IsDevelopment())
                {
                    app.UseExceptionHandler("/Error", createScopeForErrors: true);
                    app.UseHsts();
                }
                ```

                `Error.razor` 展示友好错误页；生产环境不要泄露堆栈。

                ## 3. 结构化日志：ILogger

                ```csharp
                @inject ILogger<WeatherCard> Logger

                Logger.LogInformation("加载天气：{City}", City);
                Logger.LogWarning("接口响应慢：{Ms}ms", ms);
                Logger.LogError(ex, "天气接口失败");
                ```

                占位符 `{City}` 是结构化日志的关键——能被 Serilog / OpenTelemetry 采集为字段。

                ## 4. 捕获异步事件异常

                ```csharp
                private async Task HandleClick()
                {
                    try
                    {
                        await api.PostAsync(...);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "保存失败");
                        error = "保存失败，请重试";
                    }
                }
                ```

                ## 自测

                - `ErrorBoundary` 能捕获事件处理器里的异常吗？
                - 结构化日志的占位符写法有什么好处？
                """
            ),
            new Course("adv-08", "组件测试：bUnit", "用 bUnit 做组件单元测试、Playwright 做端到端测试。", 25,
                """
                # 组件测试：bUnit

                ## 1. 为什么测组件

                组件 = 输入（参数/服务）+ 输出（渲染 DOM/事件）。bUnit 把组件渲染进内存，断言 DOM 与行为，无需启动浏览器。

                ## 2. 建测试项目

                ```bash
                dotnet new xunit -n MyApp.Tests
                cd MyApp.Tests
                dotnet add package bunit
                dotnet add reference ../MyApp/MyApp.csproj
                ```

                ## 3. 渲染并断言

                ```csharp
                using Bunit;
                using Xunit;

                public class CounterTests : TestContext
                {
                    [Fact]
                    public void Click_Button_Increments_Count()
                    {
                        var cut = RenderComponent<Counter>();
                        cut.Find("button").Click();

                        cut.Find("p").MarkupMatches("<p>当前计数：1</p>");
                    }
                }
                ```

                传参数、注入服务：

                ```csharp
                Services.AddSingleton<IWeatherService, FakeWeatherService>();
                var cut = RenderComponent<WeatherCard>(p => p.Add(x => x.City, "杭州"));
                ```

                ## 4. 事件与异步

                ```csharp
                cut.Find("input").Input("张三");          // 触发 oninput
                cut.Find("form").Submit();               // 触发提交
                cut.WaitForAssertion(() => ...);         // 等待异步渲染
                ```

                ## 5. 端到端：Playwright

                ```bash
                dotnet new xunit -n MyApp.E2E
                dotnet add package Microsoft.Playwright
                ```

                ```csharp
                using var page = await browser.NewPageAsync();
                await page.GotoAsync("http://localhost:5000");
                await page.ClickAsync("button");
                await Expect(page.Locator("p")).ToContainTextAsync("1");
                ```

                ## 自测

                - bUnit 的 `TestContext` 解决了什么测试痛点？
                - 单元测试与 E2E 测试的分工是什么？
                """
            ),
        ]);
}
