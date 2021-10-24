#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["tests/MDS.TestSite/MDS.TestSite.csproj", "tests/MDS.TestSite/"]
RUN dotnet restore "tests/MDS.TestSite/MDS.TestSite.csproj"
COPY . .
WORKDIR "/src/tests/MDS.TestSite"
RUN dotnet build "MDS.TestSite.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MDS.TestSite.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MDS.TestSite.dll"]