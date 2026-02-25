# Sử dụng SDK để build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# 1. Copy toàn bộ các file .csproj vào ĐÚNG cấu trúc (Bỏ qua file .sln)
COPY ["src/NhatSoft.API/NhatSoft.API.csproj", "src/NhatSoft.API/"]
COPY ["src/NhatSoft.Application/NhatSoft.Application.csproj", "src/NhatSoft.Application/"]
COPY ["src/NhatSoft.Common/NhatSoft.Common.csproj", "src/NhatSoft.Common/"]
COPY ["src/NhatSoft.Domain/NhatSoft.Domain.csproj", "src/NhatSoft.Domain/"]
COPY ["src/NhatSoft.Infrastructure/NhatSoft.Infrastructure.csproj", "src/NhatSoft.Infrastructure/"]

# 2. CHỈ RESTORE PROJECT API (Nó sẽ tự động kéo theo các project Core bên dưới)
RUN dotnet restore "src/NhatSoft.API/NhatSoft.API.csproj"

# 3. Copy toàn bộ source code thực tế vào
COPY . .

# 4. Di chuyển vào thư mục API và tiến hành đóng gói (Publish)
WORKDIR "/app/src/NhatSoft.API"
RUN dotnet publish "NhatSoft.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 5. Sử dụng môi trường Runtime siêu nhẹ để chạy thực tế
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Mở cổng 8080 và chạy ứng dụng
EXPOSE 8080
ENTRYPOINT ["dotnet", "NhatSoft.API.dll"]