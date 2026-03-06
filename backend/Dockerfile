# Etapa 1: Build (Usa o SDK completo para compilar o código)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia o arquivo .csproj e restaura as dependências
COPY ["SeuProjetoBackend.csproj", "./"]
RUN dotnet restore "SeuProjetoBackend.csproj"

# Copia o resto do código e compila
COPY . .
RUN dotnet publish "SeuProjetoBackend.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa 2: Produção (Usa apenas o Runtime, deixando a imagem final muito mais leve)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Expõe a porta padrão do .NET
EXPOSE 8080

# Comando para rodar a DLL compilada
ENTRYPOINT ["dotnet", "SeuProjetoBackend.dll"]