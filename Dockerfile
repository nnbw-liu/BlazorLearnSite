# Blazor 学习站 —— Docker 多阶段构建
# 构建：docker build -t blazor-learn-site .
# 运行：见 docker-compose.yml 或 DEPLOY.md

# ---------- 构建阶段 ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 先复制项目文件还原依赖（利用层缓存）
COPY BlazorLearnSite.csproj ./
RUN dotnet restore "BlazorLearnSite.csproj"

# 复制全部源码并发布
COPY . .
RUN dotnet publish "BlazorLearnSite.csproj" -c Release -o /app/publish --no-restore

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
