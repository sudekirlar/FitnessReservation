FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["FitnessReservation.Api/FitnessReservation.Api.csproj", "FitnessReservation.Api/"]
RUN dotnet restore "FitnessReservation.Api/FitnessReservation.Api.csproj"

COPY . .
WORKDIR "/src/FitnessReservation.Api"
RUN dotnet publish "FitnessReservation.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

RUN mkdir -p /app/data

EXPOSE 7001
ENV ASPNETCORE_URLS=http://+:7001

ENTRYPOINT ["dotnet", "FitnessReservation.Api.dll"]