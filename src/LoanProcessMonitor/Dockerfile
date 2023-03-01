FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 5003
EXPOSE 11113
EXPOSE 30003

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/LoanProcessMonitor/LoanProcessMonitor.csproj", "src/LoanProcessMonitor/"]
COPY ["src/Diagnostics/Diagnostics.csproj", "src/Diagnostics/"]
COPY ["src/Abstractions/Abstractions.csproj", "src/Abstractions/"]
RUN dotnet restore "src/LoanProcessMonitor/LoanProcessMonitor.csproj"
COPY . .
WORKDIR "/src/src/LoanProcessMonitor"
RUN dotnet build "LoanProcessMonitor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LoanProcessMonitor.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LoanProcessMonitor.dll"]