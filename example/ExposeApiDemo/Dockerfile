# 使用SDK镜像直接运行应用
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS runtime
WORKDIR /app

# 复制整个项目目录
COPY . .



# 回到主项目
WORKDIR /app/example/ExposeApiDemo
RUN dotnet publish -c Release -o /app -f net8.0

# 暴露端口
EXPOSE 80

# 运行应用
ENTRYPOINT ["dotnet", "/app/ExposeApiDemo.dll"]