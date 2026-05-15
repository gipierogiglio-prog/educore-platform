FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY src/building-blocks/Core/Educore.Core.csproj src/building-blocks/Core/
COPY src/building-blocks/Shared/Educore.Shared.csproj src/building-blocks/Shared/
COPY src/building-blocks/Database/Educore.Database.csproj src/building-blocks/Database/
COPY src/modules/Organization/Educore.Organization.csproj src/modules/Organization/
COPY src/modules/Secretary/Educore.Secretary.csproj src/modules/Secretary/
COPY src/modules/Academic/Educore.Academic.csproj src/modules/Academic/
COPY src/modules/Grading/Educore.Grading.csproj src/modules/Grading/
COPY src/api/Gateway/Educore.Api.csproj src/api/Gateway/
RUN dotnet restore src/api/Gateway/Educore.Api.csproj
COPY . .
RUN dotnet publish src/api/Gateway/Educore.Api.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
COPY --from=build /out .
ENTRYPOINT ["dotnet", "Educore.Api.dll"]
