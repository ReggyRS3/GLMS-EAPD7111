# ---------- Build stage: compile and publish the API ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["GLMS.Api/GLMS.Api.csproj", "GLMS.Api/"]
RUN dotnet restore "GLMS.Api/GLMS.Api.csproj"
COPY . .
WORKDIR /src/GLMS.Api
RUN dotnet publish "GLMS.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ---------- Runtime stage: small image with only the published app ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "GLMS.Api.dll"]