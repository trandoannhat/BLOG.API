# Sử dụng SDK để build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# 1. Copy file Solution ngoài cùng
COPY ["NhatSoft.sln", "./"]

# 2. Copy toàn bộ các file .csproj vào ĐÚNG cấu trúc thư mục src/
COPY ["src/NhatSoft.API/NhatSoft.API.csproj", "src/NhatSoft.API/"]
COPY ["src/NhatSoft.Application/NhatSoft.Application.csproj", "src/NhatSoft.Application/"]
COPY ["src/NhatSoft.Common/NhatSoft.Common.csproj", "src/NhatSoft.Common/"]
COPY ["src/NhatSoft.Domain/NhatSoft.Domain.csproj", "src/NhatSoft.Domain/"]
COPY ["src/NhatSoft.Infrastructure/NhatSoft.Infrastructure.csproj", "src/NhatSoft.Infrastructure/"]

# 3. Restore các package (Tải thư viện) cho toàn bộ Solution
RUN dotnet restore "NhatSoft.sln"

# 4. Copy toàn bộ source code thực tế vào
COPY . .

# 5. Di chuyển vào thư mục API và tiến hành đóng gói (Publish)
WORKDIR "/app/src/NhatSoft.API"
RUN dotnet publish "NhatSoft.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 6. Sử dụng môi trường Runtime siêu nhẹ để chạy thực tế
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Mở cổng 8080 và chạy ứng dụng
EXPOSE 8080
ENTRYPOINT ["dotnet", "NhatSoft.API.dll"]