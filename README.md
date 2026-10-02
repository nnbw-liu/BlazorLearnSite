# Blazor 学习站（BlazorLearnSite）

一个用 **Blazor Server** 构建的学习网站，用于系统学习 Blazor 本身：基础、扩展、控件、三方库、身份认证与整体架构，并**按账号记录学习进度**。它本身就是一个可运行的 Blazor 工程范例。

## 功能

- **六大学习模块、40+ 节课**（内置 Markdown 内容 + 代码示例 + 自测题）
  1. Blazor 基础学习路径
  2. Blazor 扩展学习
  3. 常规控件学习
  4. 三方库如何引用
  5. 身份认证
  6. 整体架构应当如何搭建
- **按身份记录进度**：注册登录后，每节课可「标记完成 / 撤销」，首页与课程列表实时显示总进度、模块进度。
- **完整认证**：ASP.NET Core Identity 注册 / 登录 / 账户管理（含 2FA 脚手架），可扩展第三方登录。
- **可部署到服务器**：Docker / IIS / Nginx 三种方式，见 [DEPLOY.md](DEPLOY.md)。

## 技术栈

.NET 10 · Blazor Web App（Interactive Server）· ASP.NET Core Identity · EF Core + SQLite · Markdig

## 快速开始

```bash
dotnet run --project BlazorLearnSite.csproj
```

打开 `https://localhost:5001`，注册账号后开始学习。

## 项目结构

```
BlazorLearnSite/
├─ Program.cs                       入口：服务注册、请求管道、启动自动迁移
├─ Components/
│  ├─ Account/**                     Identity 脚手架（注册/登录/管理）
│  ├─ Layout/                       布局 + 导航
│  └─ Pages/
│     ├─ Home.razor                  学习路线总览 + 进度仪表盘
│     ├─ Courses.razor               全部课程列表（含完成状态）
│     └─ Lesson.razor                课程详情 + 标记完成
├─ Data/
│  ├─ LearningCatalog.cs             课程目录模型与查找
│  ├─ Catalog.*.cs                   六个模块的课程内容（Markdown）
│  ├─ LearningModels.cs              进度实体
│  ├─ ApplicationDbContext.cs        EF Core 上下文
│  └─ Migrations/                    EF 迁移
├─ Services/
│  └─ LearningProgressService.cs     学习进度读写
├─ wwwroot/                          静态资源与站点样式
├─ Dockerfile / docker-compose.yml   容器化部署
└─ DEPLOY.md                         部署指南
```

## 常用命令

```bash
dotnet run                      # 本地运行
dotnet build                    # 编译
dotnet publish -c Release       # 发布
dotnet ef migrations add <Name> # 新增数据库迁移
docker compose up -d --build    # Docker 部署
```
