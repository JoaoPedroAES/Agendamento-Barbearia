# Estágio 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia todos os arquivos da pasta backend para o container
COPY . .

# Restaura as dependências
# ATENÇÃO: Como o csproj está na raiz da pasta backend, não usamos subpastas aqui
RUN dotnet restore "barbearia.api.csproj"

# Compila e publica
RUN dotnet publish "barbearia.api.csproj" -c Release -o /app/publish

# Estágio 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Configura a porta
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Inicia a aplicação
ENTRYPOINT ["dotnet", "barbearia.api.dll"]