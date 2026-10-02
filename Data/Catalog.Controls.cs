namespace BlazorLearnSite.Data;

/// <summary>模块三：常规控件学习。</summary>
public static partial class Catalog
{
    public static LearningModule Controls { get; } = new(
        Id: "controls",
        Order: 3,
        Title: "常规控件学习",
        Summary: "系统掌握内置输入控件、导航控件、数据展示、布局与 CSS 隔离，并了解主流第三方控件库的取舍。",
        Icon: "bi-ui-checks-grid",
        Lessons:
        [
            new Course("ctl-01", "内置表单控件全家桶", "InputText/InputNumber/InputSelect/InputCheckbox/InputDate/InputRadio 逐个上手。", 30,
                """
                # 内置表单控件全家桶

                所有输入控件都配合 `EditForm` 使用，自动接入校验（红框 + 错误消息）。

                ## 1. 文本与数字

                ```razor
                <InputText  @bind-Value="user.Name"   placeholder="用户名" />
                <InputNumber @bind-Value="user.Age"   min="0" max="120" />
                ```

                `InputNumber` 的值类型可以是 `int` / `decimal` / `double`，非法输入自动显示校验错误。

                ## 2. 下拉选择

                ```razor
                <InputSelect @bind-Value="user.City">
                    <option value="">请选择城市</option>
                    <option value="hz">杭州</option>
                    <option value="sh">上海</option>
                </InputSelect>

                @code {
                    public string City { get; set; } = "";   // 与 option value 对应
                }
                ```

                动态选项（绑定数据源）：

                ```razor
                <InputSelect @bind-Value="selectedId">
                    @foreach (var c in cities)
                    {
                        <option value="@c.Id">@c.Name</option>
                    }
                </InputSelect>
                ```

                ## 3. 复选框与开关

                ```razor
                <label>
                    <InputCheckbox @bind-Value="user.Agree" />
                    同意服务条款
                </label>
                ```

                ## 4. 日期

                ```razor
                <InputDate @bind-Value="user.Birthday" />
                <InputDate @bind-Value="range.Start" /> ~ <InputDate @bind-Value="range.End" />
                ```

                ## 5. 单选组

                ```razor
                <InputRadioGroup @bind-Value="user.Plan">
                    <InputRadio Value="free" /> 免费版
                    <InputRadio Value="pro" /> 专业版
                    <InputRadio Value="team" /> 团队版
                </InputRadioGroup>
                ```

                ## 6. 文件上传

                ```razor
                <InputFile OnChange="OnFile" accept=".pdf,.docx" multiple />

                @code {
                    private async Task OnFile(InputFileChangeEventArgs e)
                    {
                        foreach (var f in e.GetMultipleFiles(10))
                        {
                            var ms = new MemoryStream();
                            await f.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024).CopyToAsync(ms);
                            // 上传 ms.ToArray()
                        }
                    }
                }
                ```

                ## 自测

                - `InputSelect` 绑定的是什么类型的值？
                - 文件上传最大尺寸在哪控制？
                """
            ),
            new Course("ctl-02", "按钮与导航控件", "按钮、事件修饰符、NavLink、Link 组件。", 20,
                """
                # 按钮与导航控件

                ## 1. 按钮的三种形态

                ```razor
                <button @onclick="Save">普通按钮</button>
                <button class="btn btn-primary" @onclick="Save">带样式</button>
                <button @onclick="Save" disabled="@saving">禁用态</button>
                ```

                **防重复提交**（异步保存期间禁用）：

                ```razor
                <button @onclick="Save" disabled="@saving">
                    @(saving ? "保存中…" : "保存")
                </button>
                @code {
                    private bool saving;
                    private async Task Save()
                    {
                        saving = true;
                        await Task.Delay(1000);   // 模拟接口
                        saving = false;
                    }
                }
                ```

                ## 2. 事件修饰符

                ```razor
                <button @onclick="Save" @onclick:preventDefault="true">阻止默认行为</button>
                <button @onclick="Save" @onclick:stopPropagation="true">阻止冒泡</button>
                ```

                ## 3. 导航控件

                ```razor
                @* NavLink：自动高亮当前路由 *@
                <NavLink class="nav-link" href="/">首页</NavLink>
                <NavLink class="nav-link" href="/courses" Match="NavLinkMatch.All">全部课程</NavLink>

                @* Link 组件（.NET 9+）：受管导航的 <a> *@
                <Link href="/lesson/basic-01">第 1 课</Link>
                ```

                ## 4. 前进/后退

                ```csharp
                @inject NavigationManager Nav
                Nav.NavigateTo("javascript:history.back()", forceLoad: true);
                // 更推荐：自己记录来源 URL，用 Nav.NavigateTo(returnUrl)
                ```

                ## 自测

                - `NavLinkMatch.All` 与 `Prefix` 的区别？
                - 保存按钮如何防止用户连点？
                """
            ),
            new Course("ctl-03", "数据展示：表格、列表与条件渲染", "表格渲染、排序分页、@if/@foreach、空态处理。", 30,
                """
                # 数据展示：表格、列表与条件渲染

                ## 1. 基础表格

                ```razor
                <table class="table table-striped">
                    <thead>
                        <tr><th>姓名</th><th>邮箱</th><th>角色</th></tr>
                    </thead>
                    <tbody>
                        @foreach (var u in users)
                        {
                            <tr>
                                <td>@u.Name</td>
                                <td>@u.Email</td>
                                <td>@u.Role</td>
                            </tr>
                        }
                    </tbody>
                </table>
                ```

                ## 2. 空态与加载态

                ```razor
                @if (loading)
                {
                    <div class="text-muted">加载中…</div>
                }
                else if (users.Count == 0)
                {
                    <div class="alert alert-info">暂无数据</div>
                }
                else
                {
                    <table> ... </table>
                }
                ```

                ## 3. 排序（前端）

                ```razor
                <button @onclick="() => SortBy(nameof(User.Name))">按姓名</button>

                @code {
                    private List<User> users = [];
                    private bool asc = true;

                    private void SortBy(string prop) => users = (prop switch
                    {
                        "Name" => asc ? users.OrderBy(u => u.Name) : users.OrderByDescending(u => u.Name),
                        _ => users.AsEnumerable()
                    }).ToList();
                }
                ```

                更通用：用 `IQueryable` + 后端排序分页（大数据量时）。

                ## 4. 分页

                ```razor
                @for (var p = 1; p <= totalPages; p++)
                {
                    <button class="@(p == page ? "btn-primary" : "btn-outline-primary")" @onclick="() => GoPage(p)">@p</button>
                }
                ```

                ## 5. 条件渲染：@switch / 模板

                ```razor
                @switch (status)
                {
                    case "draft":  <span class="badge bg-secondary">草稿</span>; break;
                    case "done":   <span class="badge bg-success">已完成</span>; break;
                    default:       <span>未知</span>; break;
                }
                ```

                ## 自测

                - 列表为空时为什么要显示空态而不是空白？
                - 千级数据排序分页应该在前端还是后端做？
                """
            ),
            new Course("ctl-04", "布局与 CSS 隔离", "布局组件、嵌套布局、CSS Isolation 与组件样式作用域。", 25,
                """
                # 布局与 CSS 隔离

                ## 1. 布局组件

                `Components/Layout/MainLayout.razor` 是站点主布局：

                ```razor
                @inherits LayoutComponentBase

                <div class="page">
                    <div class="sidebar">@* 导航 *@</div>
                    <main>
                        <article class="content px-4">
                            @Body          @* 页面内容注入点 *@
                        </article>
                    </main>
                </div>
                ```

                页面通过 `@layout` 指定布局；子布局可以嵌套：

                ```razor
                @* AdminLayout.razor *@
                @inherits LayoutComponentBase
                <div class="admin">
                    <aside>管理菜单</aside>
                    @Body
                </div>

                @* 页面 *@
                @page "/admin/users"
                @layout AdminLayout
                ```

                ## 2. CSS 隔离（CSS Isolation）

                组件同级放 `.razor.css`，样式**自动加作用域属性**，只影响本组件：

                ```css
                /* Counter.razor.css */
                .count-display { font-size: 2rem; color: #0d6efd; }
                ```

                ```razor
                <p class="count-display">@count</p>
                ```

                生成的 HTML 里 class 会变成 `count-display b-xxxxx`，其他组件同名类互不干扰。

                **穿透隔离**（给子组件内部元素定义样式）：

                ```css
                ::deep .child-inner { background: #f8f9fa; }
                ```

                ## 3. 全局样式放哪

                - `wwwroot/app.css`：全站全局样式。
                - 布局级：`MainLayout.razor.css`。
                - 组件级：`Xxx.razor.css`。

                ## 自测

                - CSS 隔离如何避免组件间样式冲突？
                - 想给第三方组件内部元素定制样式用什么写法？
                """
            ),
            new Course("ctl-05", "模板化控件与自定义控件", "把重复 UI 封装成可复用组件：参数、RenderFragment、泛型组件。", 30,
                """
                # 模板化控件与自定义控件

                当同一段 UI 在多个页面重复时，就该封装成组件。

                ## 1. 简单封装：StatusBadge

                ```razor
                @* StatusBadge.razor *@
                <span class="badge @CssClass">@Text</span>

                @code {
                    [Parameter] public string Status { get; set; } = "";
                    private string CssClass => Status switch
                    {
                        "done" => "bg-success", "doing" => "bg-primary",
                        "todo" => "bg-secondary", _ => "bg-dark"
                    };
                    private string Text => Status switch
                    {
                        "done" => "已完成", "doing" => "学习中",
                        "todo" => "未开始", _ => Status
                    };
                }
                ```

                使用：`<StatusBadge Status="@lesson.Status" />`

                ## 2. 通用加载按钮

                ```razor
                @* LoadingButton.razor *@
                <button class="btn @CssClass" disabled="@Busy" @onclick="OnClick">
                    @if (Busy) { <span>⏳</span> } @Label
                </button>

                @code {
                    [Parameter] public string Label { get; set; } = "确定";
                    [Parameter] public string CssClass { get; set; } = "btn-primary";
                    [Parameter] public bool Busy { get; set; }
                    [Parameter] public EventCallback OnClick { get; set; }
                }
                ```

                ## 3. 泛型模板组件：ConfirmDialog

                ```razor
                @* ConfirmDialog.razor *@
                @typeparam T
                <div class="modal">
                    <div class="modal-body">@Message</div>
                    <button @onclick="() => OnYes.InvokeAsync(Data)">确认</button>
                    <button @onclick="() => OnNo.InvokeAsync()">取消</button>
                </div>

                @code {
                    [Parameter] public string Message { get; set; } = "";
                    [Parameter] public T? Data { get; set; }
                    [Parameter] public EventCallback<T?> OnYes { get; set; }
                    [Parameter] public EventCallback OnNo { get; set; }
                }
                ```

                使用：`<ConfirmDialog T="User" Message="删除该用户？" Data="user" OnYes="Delete" />`

                ## 4. 封装原则

                - 输入用 `[Parameter]`，输出用 `EventCallback`，别直接操作父状态。
                - 单一职责：一个组件只做一件事。
                - 需要 JS 的复杂组件（图表、富文本）放到独立目录并配 `.razor.js`。

                ## 自测

                - 自定义组件的「输出」为什么用 EventCallback 而不是直接改父组件字段？
                - 泛型组件用什么指令声明类型参数？
                """
            ),
            new Course("ctl-06", "第三方控件库纵览", "MudBlazor / Blazorise / Radzen / Ant Design Blazor 怎么选。", 25,
                """
                # 第三方控件库纵览

                ## 1. 主流控件库对比

                | 库 | 风格 | 组件丰富度 | 社区 | 适合 |
                |---|---|---|---|---|
                | **MudBlazor** | Material Design | 极丰富（表格/图表/对话框/表单） | 大、活跃 | 后台管理、通用业务系统 |
                | **Blazorise** | 多主题（Bootstrap/Material/Ant） | 丰富 | 中 | 需要换肤的企业应用 |
                | **Radzen** | 企业风 | 丰富（含 BI 组件） | 中 | 数据密集型后台 |
                | **Ant Design Blazor** | Ant Design | 丰富 | 中（中文文档好） | 中文企业系统 |
                | **Fluent UI Blazor** | 微软 Fluent | 中 | 微软官方 | 与 Office 风格一致 |

                ## 2. 本学习站的建议

                - 先精通**内置控件**（上一课），第三方库只是加速器。
                - 后台类应用首选 **MudBlazor**：文档全、示例多、上手快。
                - 不要同时引两套大 UI 库（体积、样式冲突）。

                ## 3. 快速体验 MudBlazor

                ```bash
                dotnet add package MudBlazor
                ```

                ```csharp
                // Program.cs
                builder.Services.AddMudServices();
                ```

                ```razor
                @* 布局里引入 *@
                <link href="_content/MudBlazor/MudBlazor.min.css" rel="stylesheet" />
                <script src="_content/MudBlazor/MudBlazor.min.js"></script>
                ```

                ```razor
                @* 页面直接用 *@
                <MudButton Variant="Variant.Filled" Color="Color.Primary" OnClick="Save">保存</MudButton>
                <MudTable Items="@users" />          @* 自带排序、分页、过滤 *@
                ```

                ## 4. 接入检查清单

                - [ ] NuGet 包版本与目标框架兼容
                - [ ] 静态资源（css/js）已注册
                - [ ] 服务已注册（如 `AddMudServices`）
                - [ ] 组件交互需要的 JS 在 `OnAfterRender` 后调用

                ## 自测

                - 选择第三方控件库主要看哪三个维度？
                - 接入一个组件库最少需要哪几步？
                """
            ),
        ]);
}
