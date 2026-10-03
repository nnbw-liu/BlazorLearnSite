# Docker 部署手册（宝塔无软件商店版，纯命令行）

> 你的宝塔没有软件商店，那就全程用命令行（SSH）操作。
> 宝塔这边只需要做两件事：**传项目文件**、**放行端口**。其余全部在终端里完成。

---

## 第 0 步：准备一个终端

用宝塔的「终端」功能（左侧菜单有终端图标），或者自己用 SSH 工具登录（Windows 自带：

```bash
ssh admin@服务器公网IP
```

登录后先确认系统版本——后面有的命令分系统，先看一眼：

```bash
cat /etc/os-release | grep PRETTY_NAME
```

记住输出是 Ubuntu / Debian / CentOS 哪一类。

## 第 1 步：把项目传到服务器（用宝塔文件管理器）

宝塔 → 文件 → 进入 `/home/www/code/BlazorLearnSite`（没有就新建，路径随意）→ **上传**，把本地 `C:\Code\SDemo\BlazorLearnSite` 里的文件传上去。

确认目录里有这些关键文件：

```
Dockerfile
docker-compose.yml
BlazorLearnSite.csproj
Program.cs
Components/
```

## 第 2 步：安装 Docker

官方一键脚本，自动识别你的系统，一行装完：

```bash
curl -fsSL https://get.docker.com | sh
```

装完启动并设置开机自启：

```bash
systemctl enable --now docker
```

验证装好了（三条命令都要有输出）：

```bash
docker --version
docker compose version
```

> 如果 `docker compose version` 报错（提示不是 docker 命令），说明 Docker 版本太老没带 compose 插件，单独装：
> ```bash
> apt install -y docker-compose-plugin      # Ubuntu/Debian
> # 或
> yum install -y docker-compose-plugin      # CentOS
> ```

## 第 3 步：验证 Docker 能拉镜像（国内网络可能卡在这）

```bash
docker run --rm hello-world
```

正常会打印 "Hello from Docker!"。

**卡住/超时** → 配国内镜像加速：

```bash
mkdir -p /etc/docker
cat > /etc/docker/daemon.json <<'EOF'
{
  "registry-mirrors": [
    "https://docker.m.daocloud.io",
    "https://docker.1panel.live"
  ]
}
EOF
systemctl restart docker
```

配完重新跑 `docker run --rm hello-world`，能打印就 OK。

## 第 4 步：构建并启动（核心一步）

```bash
cd /home/www/code/BlazorLearnSite
docker compose up -d --build
```

**第一次等几分钟**（下载 .NET SDK 镜像 → 编译 → 打包），看到 `Started` 或 `done` 就成功了。

> 图形化界面看不到容器是正常的——你手动装的 Docker 不走宝塔 Docker 管理器，管理全靠命令（见第 6 步）。

## 第 5 步：放行端口（最容易漏，两道墙都要开）

**① 阿里云安全组**（浏览器打开阿里云控制台）
- ECS 实例 → 安全组 → 配置规则 → 入方向 → 手动添加
- 协议 TCP、端口 **8080**、授权对象 `0.0.0.0/0`

**② 服务器系统防火墙**（终端执行，按第 0 步看到的系统选一条）

```bash
# CentOS / Alibaba Cloud Linux
firewall-cmd --permanent --add-port=8080/tcp && firewall-cmd --reload

# Ubuntu / Debian（装过 ufw 才需要，没装忽略）
ufw allow 8080
```

> 如果宝塔面板里有「安全」页，也可以在那里放行 8080，效果一样。

## 第 6 步：验证并访问

```bash
docker ps                 # STATUS 应为 Up
curl -s http://localhost:8080 | head -20    # 返回 HTML
docker compose logs -f    # 看日志，Ctrl+C 退出
```

浏览器打开 `http://服务器公网IP:8080`，注册账号开始用。

## 第 7 步：日常管理（全部命令）

```bash
# 停止 / 启动 / 重启
docker compose stop
docker compose start
docker compose restart

# 看日志
docker compose logs -f

# 更新代码后重新部署（重新上传文件后执行）
docker compose up -d --build

# 完全停掉并删除容器（数据卷保留，数据不丢）
docker compose down
```

**数据在哪**：用户和进度存在 Docker 卷里（容器内 `/app/Data`），容器删了重建数据还在。备份：

```bash
docker volume ls
# 找到类似 blazor-learnsite_blazor-data 的名字，替换到下面命令
docker run --rm -v blazor-learnsite_blazor-data:/data -v $(pwd):/backup alpine tar czf /backup/blazor-data-backup.tar.gz -C /data .
```

## 第 8 步：想用 80 端口 / 域名访问（可选）

把 `docker-compose.yml` 里的端口映射改一下：

```yaml
ports:
  - "80:8080"
```

然后 `docker compose up -d` 重建，安全组和防火墙放行 **80** 端口，浏览器访问 `http://IP`。

要域名 + HTTPS：用宝塔「网站」功能建站反代到 `http://127.0.0.1:8080`，**必须**在反代配置里加这两个请求头，否则页面一直「正在重新连接」：

```
Upgrade    $http_upgrade
Connection "upgrade"
```

## 常见问题速查

| 现象 | 处理 |
|---|---|
| `curl: command not found` | `apt install -y curl` 或 `yum install -y curl` |
| `docker: command not found` | 第 2 步安装失败，重跑；或 `systemctl restart docker` 后重登 |
| `permission denied ... docker.sock` | 命令前加 `sudo`；或 `sudo usermod -aG docker $USER` 后重新登录 |
| 拉镜像超时/卡住 | 第 3 步镜像加速配置 |
| `8080: bind: address already in use` | 端口被占：compose 里改成 `"8081:8080"`（左边是宿主机端口） |
| 容器一直在重启 | `docker compose logs` 看报错 |
| 本机 curl 通、外网打不开 | 阿里云安全组没放行 8080（第 5 步） |
| 页面「正在重新连接」 | 反代缺 WebSocket 头（第 8 步） |
