# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the .csproj and restore
COPY ["hospitalManagment/hospitalManagment.csproj", "hospitalManagment/"]
WORKDIR "/src/hospitalManagment"
RUN dotnet restore "hospitalManagment.csproj"

# Copy everything else and publish
COPY . .
RUN dotnet publish "hospitalManagment.csproj" -c Release -o /app/publish

# Stage 2: Run
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
CMD ["dotnet", "hospitalManagment.dll"]
