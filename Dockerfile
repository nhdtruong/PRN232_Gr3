# Base image dùng để chạy ứng dụng (ASP.NET Core 9.0)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Build image dùng SDK để compile code
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy file Solution và toàn bộ các file csproj để restore dependencies (tối ưu cache của Docker)
COPY ["PROJECT_PRN232_.sln", "./"]
COPY ["src/PROJECT_PRN232_.WebApp/PROJECT_PRN232_.WebApp.csproj", "src/PROJECT_PRN232_.WebApp/"]
COPY ["src/PROJECT_PRN232_.Api/PROJECT_PRN232_.Api.csproj", "src/PROJECT_PRN232_.Api/"]
COPY ["src/PROJECT_PRN232_.Application/PROJECT_PRN232_.Application.csproj", "src/PROJECT_PRN232_.Application/"]
COPY ["src/PROJECT_PRN232_.Domain/PROJECT_PRN232_.Domain.csproj", "src/PROJECT_PRN232_.Domain/"]
COPY ["src/PROJECT_PRN232_.Infrastructure/PROJECT_PRN232_.Infrastructure.csproj", "src/PROJECT_PRN232_.Infrastructure/"]

RUN dotnet restore "PROJECT_PRN232_.sln"

# Copy toàn bộ mã nguồn còn lại
COPY . .

# Build project WebApp
WORKDIR "/src/src/PROJECT_PRN232_.WebApp"
RUN dotnet build "PROJECT_PRN232_.WebApp.csproj" -c Release -o /app/build

# Publish project
FROM build AS publish
RUN dotnet publish "PROJECT_PRN232_.WebApp.csproj" -c Release -o /app/publish /p:UseAppHost=false /p:ErrorOnDuplicatePublishOutputFiles=false

# Image cuối cùng (siêu nhẹ) chỉ chứa runtime và file đã compile
FROM base AS final
WORKDIR /app
# Fix lỗi inotify limit trên Render Free tier
ENV DOTNET_USE_POLLING_FILE_WATCHER=true
ENV DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PROJECT_PRN232_.WebApp.dll"]
