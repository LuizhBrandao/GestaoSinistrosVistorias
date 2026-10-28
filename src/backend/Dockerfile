FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

# Copia os arquivos de projeto para restauração otimizada com cache de camadas
COPY Sinistros.slnx ./
COPY Sinistros.Domain/*.csproj ./Sinistros.Domain/
COPY Sinistros.Application/*.csproj ./Sinistros.Application/
COPY Sinistros.Infrastructure/*.csproj ./Sinistros.Infrastructure/
COPY Sinistros.Web/*.csproj ./Sinistros.Web/
COPY tests/Sinistros.Domain.Tests/*.csproj ./tests/Sinistros.Domain.Tests/
COPY tests/Sinistros.Application.Tests/*.csproj ./tests/Sinistros.Application.Tests/

RUN dotnet restore Sinistros.slnx

# Copia o código completo e publica versão de produção
COPY . .
RUN dotnet publish Sinistros.Web/Sinistros.Web.csproj -c Release -o /app/publish --no-restore

# Imagem final de runtime enxuta
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=5000
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

EXPOSE 5000
ENTRYPOINT ["dotnet", "Sinistros.Web.dll"]
