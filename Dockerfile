# Blazor 学习站 —— Docker 多阶段构建
# 构建：docker build -t blazor-learn-site .
# 运行：见 docker-compose.yml 或 DEPLOY.md

# ---------- 构建阶段 ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 先复制项目文件与 Razor 组件再还原依赖（利用层缓存）
#
# ⚠️ 关键：restore 之前 .razor 文件必须已经就位。
# Web SDK 只有在 restore 时“看得到” .razor 文件，才会注入
# Microsoft.AspNetCore.App.Internal.Assets（它携带 wwwroot/_framework/*.js）。
# 若只复制 .csproj 就 restore，随后的 --no-restore 发布会“成功”，
# 但产物里没有 wwwroot/_framework/，MapStaticAssets 不注册 _framework 路由，
# 线上表现为 /_framework/blazor.web.js → 404：页面能显示，但 Blazor 电路
# 永远建立不起来，所有 @onclick 按钮（如“开始学习”）全部失效。
COPY BlazorLearnSite.csproj ./
COPY Components/ ./Components/
RUN dotnet restore "BlazorLearnSite.csproj"

# 复制全部源码并发布
COPY . .
RUN dotnet publish "BlazorLearnSite.csproj" -c Release -o /app/publish --no-restore

# 构建期断言：框架脚本必须在产物中，缺失就让构建直接失败，避免静默发出“按钮全失效”的站点
RUN test -f /app/publish/wwwroot/_framework/blazor.web.js \
    || (echo "ERROR: publish output missing wwwroot/_framework/blazor.web.js (restore must run with .razor files present)" && exit 1)

# ---------- 运行阶段 ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# 数据目录：SQLite 数据库持久化挂载点
RUN mkdir -p /app/Data && chown -R 1654:1654 /app/Data

COPY --from=build /app/publish .

# 非 root 运行（镜像内置用户 uid=1654）
USER 1654:1654

ENV ASPNETCORE_URLS=http://+:8080
ENV ConnectionStrings__DefaultConnection="Data Source=/app/Data/app.db"

EXPOSE 8080

ENTRYPOINT ["dotnet", "BlazorLearnSite.dll"]
