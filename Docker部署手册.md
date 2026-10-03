# Docker 部署手册（Alibaba Cloud Linux 3，宝塔无软件商店，纯命令行）

> 你的服务器是阿里云轻量应用服务器自带的 **Alibaba Cloud Linux 3（alinux3）**。
> 宝塔没有软件商店，所以全程命令行操作；宝塔这边只需要**传项目文件**和**放行端口**。
> 用 Docker 部署能绕开之前那个 `GLIBC_2.33 not found` 报错——官方镜像自带兼容环境，跟服务器系统版本无关。

---

## 第 0 步：准备终端 + 确认系统

用宝塔的「终端」功能，或者自己 SSH 登录（Windows 自带）：

```bash
ssh admin@服务器公网IP
```

确认系统版本（alinux3 或 alinux2，安装命令不同）：

```bash
cat /etc/alinux-release
```

## 第 1 步：把项目传到服务器（用宝塔文件管理器）

宝塔 → 文件 → 进入 `/home/www/code/BlazorLearnSite`（没有就新建）→ **上传**，把项目文件传上去。

> 推荐用 Git：本地 `git push` 后，服务器 `git clone git@github.com:nnbw-liu/BlazorLearnSite.git`（或 `git pull`），更省事，更新也方便。

确认目录里有这些关键文件：

```
Dockerfile
docker-compose.yml
BlazorLearnSite.csproj
Program.cs
Components/
```

## 第 2 步：安装 Docker（alinux3 专用，不能用 get.docker.com）

> ⚠️ 注意：`get.docker.com` 一键脚本**不支持 alinux**，直接跑会报 `Unsupported distribution 'alinux'`。
> 你这台是 alinux3，用下面的 `dnf` 方式安装。下面命令依次复制执行：

**① 先卸载旧 Docker**（没有就跳过，报错忽略）：

```bash
sudo dnf -y remove docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
sudo rm -f /etc/yum.repos.d/docker*.repo
```

**② 安装 alinux3 兼容插件**（关键！不然源识别失败）：

```bash
sudo dnf -y install dnf-plugin-releasever-adapter --repo alinux3-plus
```

**③ 添加 docker-ce 阿里云镜像源**：

```bash
sudo wget -O /etc/yum.repos.d/docker-ce.repo http://mirrors.cloud.aliyuncs.com/docker-ce/linux/centos/docker-ce.repo
sudo sed -i 's|https://mirrors.aliyun.com|http://mirrors.cloud.aliyuncs.com|g' /etc/yum.repos.d/docker-ce.repo
```

**④ 安装 docker 全套（含 compose 插件）**：

```bash
sudo dnf -y install docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin --nobest
```

**⑤ 启动 + 开机自启**：

```bash
sudo systemctl start docker
sudo systemctl enable docker
```

**⑥ 把当前 admin 用户加入 docker 组**（以后不用 sudo）：

```bash
sudo usermod -aG docker $USER
```

> ⚠️ 用户组生效需要**重新 SSH 登录一次**。不重连的话，执行 docker 命令还是要加 `sudo`。

> 如果你系统是 **alinux2**，把第②步换成 yum 版本：
> ```bash
> sudo yum install yum-plugin-releasever-adapter --repo alinux2-plus
> ```

## 第 3 步：验证 Docker + 配置镜像加速

验证：

```bash
docker --version
docker compose version
sudo docker run hello-world
```

正常会打印 "Hello from Docker!"。**卡住/超时**是拉镜像慢，配置阿里云镜像加速（推荐，拉镜像快很多）：

```bash
sudo tee /etc/docker/daemon.json <<-'EOF'
{
  "registry-mirrors": ["https://mirror.baidubce.com"]
}
EOF
sudo systemctl daemon-reload
sudo systemctl restart docker
```

配完重新跑 `sudo docker run hello-world`，能打印就 OK。

## 第 4 步：构建并启动（核心一步）

```bash
cd /home/www/code/BlazorLearnSite
docker compose up -d --build
```

**第一次等几分钟**（下载 .NET SDK 镜像 → 编译 → 打包），看到 `Started` 或 `done` 就成功了。

> 手动装的 Docker 不走宝塔 Docker 管理器，面板里看不到容器是正常的，管理全靠命令（见第 6 步）。

## 第 5 步：放行端口（最容易漏，两道墙都要开）

**① 阿里云安全组**（浏览器打开阿里云控制台）
- ECS 实例 → 安全组 → 配置规则 → 入方向 → 手动添加
- 协议 TCP、端口 **8080**、授权对象 `0.0.0.0/0`

**② 服务器系统防火墙**（终端执行，alinux 用 firewalld）：

```bash
sudo firewall-cmd --permanent --add-port=8080/tcp && sudo firewall-cmd --reload
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

# 更新代码后重新部署（重新 git pull / 上传文件后执行）
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
| `Unsupported distribution 'alinux'` | 别用 get.docker.com，按第 2 步 dnf 方式装 |
| `curl: command not found` | `sudo dnf install -y curl` |
| `docker: command not found` | 第 2 步安装失败，重跑；或 `systemctl restart docker` 后重登 |
| `permission denied ... docker.sock` | 没重登用户组未生效：命令前加 `sudo`，或重新 SSH 登录 |
| 拉镜像超时/卡住 | 第 3 步镜像加速配置 |
| `8080: bind: address already in use` | 端口被占：compose 里改成 `"8081:8080"`（左边是宿主机端口） |
| 容器一直在重启 | `docker compose logs` 看报错 |
| 本机 curl 通、外网打不开 | 阿里云安全组没放行 8080（第 5 步） |
| 页面「正在重新连接」 | 反代缺 WebSocket 头（第 8 步） |
