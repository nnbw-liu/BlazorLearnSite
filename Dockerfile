# Blazor 学习站 —— Docker 多阶段构建
# 构建：docker build -t blazor-learn-site .
# 运行：见 docker-compose.yml 或 DEPLOY.md

# ---------- 构建阶段 ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 先把全部源码复制进来，再 restore。
#
# ⚠️ 顺序不能改：Web SDK 只有在 restore 期间“看得见” .razor 文件时，才会注入
# Microsoft.AspNetCore.App.Internal.Assets —— 该包携带 wwwroot/_framework/*.js。
# 若采用“只复制 .csproj → restore → 再 COPY 源码”的写法（看似能利用层缓存），
# 发布一样会成功，但产物里没有 wwwroot/_framework/，MapStaticAssets 也就不会
# 注册 _framework 路由。线上表现：页面能正常显示，但 /_framework/blazor.web.js
# 返回 404 → Blazor 电路建立不起来 → 所有 @onclick 按钮（如首页“开始学习”）
# 全部失效，而普通 <a href> 链接仍然可用。参见 dotnet/aspnetcore#69341。
COPY . .
RUN dotnet restore "BlazorLearnSite.csproj"

# 发布（restore 已完成）
RUN dotnet publish "BlazorLearnSite.csproj" -c Release -o /app/publish --no-restore

# 构建期断言：框架脚本必须在产物中，缺失就让构建直接失败，
# 而不是静默发出一个“按钮全失效”的站点
RUN test -f /app/publish/wwwroot/_framework/blazor.web.js \
    || (echo "FATAL: publish output is missing wwwroot/_framework/blazor.web.js" \
        && echo "HINT: do not run dotnet restore with only the .csproj present" \
        && exit 1)

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
