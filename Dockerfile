# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies to optimize layer cache
COPY TH4.csproj ./
RUN dotnet restore TH4.csproj

# Copy remaining source code and publish
COPY . .
RUN dotnet publish TH4.csproj -c Release -o /app/publish

# Stage 2: Final runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "MaterialManagementApi.dll"]
