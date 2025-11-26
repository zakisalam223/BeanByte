FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file first
COPY ["CPSCForum.csproj", "./"]

# Restore dependencies
RUN dotnet restore "CPSCForum.csproj"

# Copy everything else and publish
COPY . .
RUN dotnet publish "CPSCForum.csproj" -c Release -o /app/publish /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install CA certificates and OpenSSL libraries
RUN apt-get update && apt-get install -y \
    ca-certificates \
    libssl-dev \
    && update-ca-certificates

# Expose the port
EXPOSE ${PORT}
ENV ASPNETCORE_URLS=http://+:${PORT}

# Copy published app
COPY --from=build /app/publish .

# Run the application
ENTRYPOINT ["dotnet", "CPSCForum.dll"]
