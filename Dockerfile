# ---- Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app /p:UseAppHost=false

# ---- Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
# Render مقدار PORT را ست می‌کند؛ با این خط روی همان پورت گوش می‌دهیم
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}
# (اختیاری) صرفاً سندی؛ Render نیاز ندارد ولی بد نیست
EXPOSE 10000
CMD ["sh", "-c", "dotnet BankeKhodroBot.dll"]
