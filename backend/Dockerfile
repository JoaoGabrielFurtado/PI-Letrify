# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia apenas o arquivo .csproj primeiro (respeitando a estrutura de pastas da sua imagem)
COPY ["pi-projetolivros/pi-projetolivros/pi-projetolivros.csproj", "pi-projetolivros/pi-projetolivros/"]

# Restaura as dependências do projeto
RUN dotnet restore "pi-projetolivros/pi-projetolivros/pi-projetolivros.csproj"

# Copia todo o resto do código do backend para dentro do container
COPY . .

# Entra na pasta final onde o código fonte está de fato
WORKDIR "/src/pi-projetolivros/pi-projetolivros"

# Compila o projeto
RUN dotnet publish "pi-projetolivros.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa 2: Produção (Imagem mais leve apenas para rodar o app)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Expõe a porta que configuramos no Fly.io
EXPOSE 8080

# Comando final apontando para a DLL gerada com o nome do projeto
ENTRYPOINT ["dotnet", "pi-projetolivros.dll"]