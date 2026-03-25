# Estágio 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# 1. Copia apenas o projeto primeiro para aproveitar o cache das camadas (Layer Caching)
COPY ["barbearia.api.csproj", "./"]
RUN dotnet restore "barbearia.api.csproj"

# 2. Agora copia o restante dos arquivos
COPY . .

# 3. Compila e publica
RUN dotnet publish "barbearia.api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estágio 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

# No Render, é comum usar a porta 10000, mas o ASP.NET 9 usa 8080 por padrão.
# Vamos manter 8080, mas certifique-se de configurar a porta 8080 no painel do Render.
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "barbearia.api.dll"]