FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY . .
RUN dotnet restore src/api/Gateway/Educore.Api.csproj
RUN dotnet publish src/api/Gateway/Educore.Api.csproj -c Release -o /out -p:UseSharedCompilation=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
COPY --from=build /out .
ENTRYPOINT ["dotnet", "Educore.Api.dll"]
