FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 5001
EXPOSE 11111
EXPOSE 30000

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/CreditCheck/CreditCheck.csproj", "src/CreditCheck/"]
COPY ["src/Abstractions/Abstractions.csproj", "src/Abstractions/"]
COPY ["src/SiloInfrastructure/SiloInfrastructure.csproj", "src/SiloInfrastructure/"]
COPY ["Diagnostics/Diagnostics.csproj", "Diagnostics/"]
RUN dotnet restore "src/CreditCheck/CreditCheck.csproj"
COPY . .
WORKDIR "/src/src/CreditCheck"
RUN dotnet build "CreditCheck.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CreditCheck.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CreditCheck.dll"]