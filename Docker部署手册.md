# Docker 部署手册（从零开始，保姆级）

> 写给第一次用 Docker 的人。照着做，把 Blazor 学习站跑起来。
> 用 Docker 部署能绕开上一回那个 `GLIBC_2.33 not found` 报错——官方镜像自带兼容环境，不再依赖服务器系统版本。

---

## 第 1 步：先确认服务器是什么系统、项目在哪

```bash
# 看系统版本（决定用哪套安装命令）
cat /etc/os-release | grep PRETTY_NAME

# 看项目目录（下面的手册以这个路径为例，换成你实际的）
ls /home/www/code/BlazorLearnSite
```

确认目录里有这几个文件（部署必需）：

```
Dockerfile            # 镜像怎么构建
docker-compose.yml    # 一键编排：端口、数据卷、重启策略
.dockerignore         # 构建时排除 bin/obj
```

没有的话，从本地项目（`C:\Code\SDemo\BlazorLearnSite`）把整个目录传到服务器再继续。

## 第 2 步：检查 Docker 装没装

```bash
docker --version
docker compose version
```

**看到版本号** → 跳到第 4 步。

**提示 `command not found`** → 没装，走第 3 步。

> 小知识：新版 Docker 自带 `docker compose`（子命令）；老版本是独立的 `docker-compose` 命令。本手册用新写法 `docker compose`。

## 第 3 步：安装 Docker

最省事的方式是官方一键脚本，它自动识别系统版本：

```bash
curl -fsSL https://get.docker.com | sh
```

跑完启动并设为开机自启：

```bash
systemctl enable --now docker
systemctl status docker
```

看到 `active (running)` 就成功了。如果 `systemctl` 提示不存在（极老系统），改用：

```bash
service docker start
service docker enable 2>/dev/null || true
```

## 第 4 步：验证 Docker 能不能用

```bash
docker run --rm hello-world
```

正常会打印一段 "Hello from Docker!"。

**如果卡住或超时**（国内服务器很常见）：是拉镜像太慢，配置镜像加速。阿里云用户建议先去容器镜像服务控制台领一个专属加速地址，也可以直接用公共源：

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

配完重新跑 `docker run --rm hello-world`。

## 第 5 步：构建并启动（核心一步）

```bash
cd /home/www/code/BlazorLearnSite
docker compose up -d --build
```

解释一下这条命令在干什么：**--build** 先按 Dockerfile 构建镜像（下载 .NET SDK → 编译发布 → 打包成运行镜像），然后 **-d** 后台启动容器。

- 首次执行要下载镜像 + 编译，**耐心等几分钟**，看到 `Started` 或 `done` 字样即成功。
- 之后每次更新代码重新执行这条命令，只需几秒到几十秒。

## 第 6 步：验证跑起来了

```bash
# 看容器状态：STATUS 应该是 Up，端口 8080 有映射
docker ps

# 本机试请求：返回 HTML 即正常
curl -s http://localhost:8080 | head -20

# 看日志（Ctrl+C 退出）：
docker compose logs -f
```

## 第 7 步：浏览器访问

打开 `http://服务器公网IP:8080`。

**打不开？九成是安全组没放行**。阿里云控制台 → ECS 实例 → 安全组 → 配置规则 → 入方向 → 添加规则：

- 协议：TCP
- 端口：8080
- 授权对象：`0.0.0.0/0`

加完再刷新浏览器。

## 第 8 步：日常管理

```bash
# 停止 / 启动 / 重启
docker compose stop
docker compose start
docker compose restart

# 看日志（-f 持续跟随，Ctrl+C 退出）
docker compose logs -f

# 更新代码后重新部署（进项目目录执行）
docker compose up -d --build

# 完全停掉并删除容器（数据卷保留，数据不丢）
docker compose down
```

**数据在哪**：用户、进度都存在 Docker 卷里（`/app/Data` 挂载点），容器删了重建数据也还在。

```bash
docker volume ls        # 会看到 blazor-learnsite_blazor-data 之类的名字
```

备份数据：

```bash
docker run --rm -v blazor-learnsite_blazor-data:/data -v $(pwd):/backup alpine tar czf /backup/blazor-data-backup.tar.gz -C /data .
```

## 常见问题速查

| 现象 | 处理 |
|---|---|
| `command not found` | 没装或刚装没生效：重新登录 SSH，或重启服务器 |
| `permission denied ... docker.sock` | 当前用户不在 docker 组：前面加 `sudo`，或 `sudo usermod -aG docker $USER` 后重新登录 |
| 拉镜像超时 | 第 4 步的镜像加速配置 |
| `8080: bind: address already in use` | 端口被占：把 compose 里 `"8080:8080"` 改成 `"8081:8080"`（左边是宿主机端口） |
| 容器一直在重启 | `docker compose logs` 看具体报错，大多是代码或数据库权限问题 |
| 想直接用 80 端口访问 | compose 里改成 `"80:8080"`，安全组放行 80 |
| 数据丢了 | 确认没用过 `docker compose down -v`（-v 会连卷一起删，慎用） |

## 部署完成后

- 想要域名 + HTTPS：用 Nginx 反代，见 `DEPLOY.md` 第 3 节（注意 SignalR 的 Upgrade 头配置）。
- 代码改动后更新：重新上传代码 → `docker compose up -d --build`，数据卷不动，进度不丢。
