# ---------- Build stage: compile and publish the MVC front end ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["GLMS.Web/GLMS.Web.csproj", "GLMS.Web/"]
RUN dotnet restore "GLMS.Web/GLMS.Web.csproj"
COPY . .
WORKDIR /src/GLMS.Web
RUN dotnet publish "GLMS.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "GLMS.Web.dll"]