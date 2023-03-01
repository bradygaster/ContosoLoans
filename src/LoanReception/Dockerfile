FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 5002
EXPOSE 11112
EXPOSE 30002

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/LoanReception/LoanReception.csproj", "src/LoanReception/"]
COPY ["src/Abstractions/Abstractions.csproj", "src/Abstractions/"]
COPY ["src/SiloInfrastructure/SiloInfrastructure.csproj", "src/SiloInfrastructure/"]
COPY ["src/Diagnostics/Diagnostics.csproj", "src/Diagnostics/"]
RUN dotnet restore "src/LoanReception/LoanReception.csproj"
COPY . .
WORKDIR "/src/src/LoanReception"
RUN dotnet build "LoanReception.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LoanReception.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LoanReception.dll"]