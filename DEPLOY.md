# Blazor 学习站 —— 部署指南

一个可部署到服务器的 **Blazor Server** 学习网站：六大学习模块、按账号记录学习进度、完整身份认证（ASP.NET Core Identity + SQLite）。

## 技术栈

| 组件 | 选型 |
|---|---|
| 框架 | .NET 10 · Blazor Web App（Interactive Server） |
| 认证 | ASP.NET Core Identity（Cookie，注册/登录/账户管理开箱即用） |
| 数据 | EF Core + SQLite（用户、角色、学习进度一张库） |
| 内容 | 内置 Markdown 课程 + Markdig 渲染 |
| 部署 | Docker（推荐）/ IIS / Nginx |

---

## 1. 本地运行

```bash
dotnet run --project BlazorLearnSite.csproj
```

浏览器打开 `https://localhost:5001`（或终端提示的地址）。注册账号 → 首页点开课程 → 「标记为已完成」→ 进度按账号保存。

> 开发环境无需邮箱确认：注册即登录（`RequireConfirmedAccount=false`）。

## 2. 方式一：Docker（推荐，一键上服务器）

```bash
# 在项目目录（含 Dockerfile）执行
docker build -t blazor-learn-site .

# 单容器运行（数据库挂持久化卷）
docker run -d --name blazor-learn-site \
  -p 8080:8080 \
  -v blazor-data:/app/Data \
  blazor-learn-site

# 或者用 compose（含数据卷、自动重启）
docker compose up -d --build
```

验证：`curl http://服务器IP:8080` 应返回页面 HTML；`curl http://服务器IP:8080/health` 返回 `Healthy`；
`curl -I http://服务器IP:8080/_framework/blazor.web.js` 必须返回 **200**（返回 404 说明镜像里缺 `wwwroot/_framework`，站点会“能看不能用”，见第 7 节）。

**远端访问**：服务器防火墙放行 8080 端口（云厂商安全组 + 系统防火墙）。之后浏览器访问 `http://服务器IP:8080`。

**数据持久化**：用户与进度都存在 `blazor-data` 卷中（容器内 `/app/Data/app.db`）。备份：

```bash
docker run --rm -v blazor-data:/data -v $(pwd):/backup alpine tar czf /backup/blazor-data.tar.gz -C /data .
```

## 3. 方式二：Linux + Nginx（域名 + HTTPS 反代）

适合有域名的正式部署。

### 3.1 发布

```bash
dotnet publish -c Release -o /opt/blazor-learn
# 产物在 /opt/blazor-learn，直接可运行（自托管 Kestrel）
```

用 systemd 托管：

```ini
# /etc/systemd/system/blazor-learn.service
[Unit]
Description=Blazor Learn Site
After=network.target

[Service]
WorkingDirectory=/opt/blazor-learn
ExecStart=/usr/bin/dotnet /opt/blazor-learn/BlazorLearnSite.dll
Restart=always
Environment=ASPNETCORE_URLS=http://127.0.0.1:5000
Environment=ConnectionStrings__DefaultConnection=Data Source=/var/lib/blazor-learn/app.db

[Install]
WantedBy=multi-user.target
```

```bash
sudo mkdir -p /var/lib/blazor-learn && sudo chown -R $USER /var/lib/blazor-learn
sudo systemctl daemon-reload && sudo systemctl enable --now blazor-learn
```

### 3.2 Nginx 反向代理

> **关键**：Blazor Server 走 SignalR（WebSocket），必须转发 `Upgrade` / `Connection` 头，否则页面「一直重连」。

```nginx
# /etc/nginx/sites-available/blazor-learn
server {
    listen 80;
    server_name your-domain.com;

    location / {
        proxy_pass http://127.0.0.1:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;      # 关键：WebSocket 升级
        proxy_set_header Connection "upgrade";       # 关键
        proxy_set_header Host $host;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
        proxy_read_timeout 3600s;                    # SignalR 长连接
    }
}
```

```bash
sudo ln -s /etc/nginx/sites-available/blazor-learn /etc/nginx/sites-enabled/
sudo nginx -t && sudo systemctl reload nginx
```

HTTPS 用 certbot 一键签发：`sudo certbot --nginx -d your-domain.com`。

## 4. 方式三：Windows + IIS

1. 安装 [ASP.NET Core Hosting Bundle](https://dotnet.microsoft.com/download/dotnet/10.0)（含 IIS 模块）。
2. 发布：`dotnet publish -c Release -o C:\sites\blazor-learn`。
3. IIS 新建站点：物理路径指向发布目录，端口 80/443。
4. **启用 WebSocket**：站点 → 配置编辑器 → `system.webServer/webSocket` → `enabled=true`，应用。
5. 应用池：若数据库文件在站点目录下，给应用池账号写入权限。

## 5. 远程访问要点

- **端口**：Docker 用 8080；Nginx/IIS 用 80/443。
- **防火墙**：云安全组 + `sudo ufw allow 80,443`（或 8080）。
- **HTTPS**：生产环境务必上 HTTPS（Cookie 认证 + HSTS 已内置，`UseHsts` 仅在生产启用）。
- **健康检查**：`GET /health` 用于探活与监控。

## 6. 数据说明

- 数据库：`Data/app.db`（SQLite 单文件）。
- 表：`AspNetUsers` 等 Identity 表 + `LessonProgresses`（学习进度，UserId+LessonId 唯一）。
- **启动时自动执行 EF 迁移**，新版本升级只需替换程序文件/重建镜像，无需手工跑迁移。
- 备份 = 复制 `app.db`（停止写入时更稳妥）。

## 7. 常见问题

| 现象 | 原因与处理 |
|---|---|
| 页面能正常显示，但所有按钮（如首页「开始学习」）点了没反应、登录后进度不更新 | 交互脚本没加载：F12 → Network 搜 `blazor.web.js`，若是 **404** 说明发布产物缺少 `wwwroot/_framework`。根因是 Docker 构建时 `dotnet restore` 早于 `.razor` 源码复制，Web SDK 未注入 `Microsoft.AspNetCore.App.Internal.Assets`（Dockerfile 已修正为 restore 前先复制 `Components/`）。处理：`docker compose build --no-cache` 重建镜像并重新部署；改完 Dockerfile 后务必确认**构建目录里那份 Dockerfile 是新版**（新版带有构建期断言，旧版会让这种坏镜像静默构建成功） |
| 页面「正在重新连接…」 | 反代没转发 `Upgrade`/`Connection` 头（Nginx 必须配，见 3.2） |
| 注册后无法登录 | 旧版要求邮箱确认；本项目已设 `RequireConfirmedAccount=false` |
| 数据库只读报错 | 容器内 `Data` 目录无写权限，挂卷时确认权限；IIS 检查应用池账号 |
| 首次访问慢 | Server 预渲染 + 首次 JIT；属正常，可加大容器内存 |
| `dotnet ef` 未找到 | 全局工具未装：`dotnet tool install --global dotnet-ef` |

## 8. 发布到服务器流程（Docker 速查）

```bash
# 本机/CI
docker build -t blazor-learn-site .
docker save blazor-learn-site | ssh user@server 'docker load'

# 服务器
docker run -d --name blazor-learn-site -p 8080:8080 -v blazor-data:/app/Data blazor-learn-site
```
