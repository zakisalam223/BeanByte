
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["CPSCForum.csproj", "./"]
RUN dotnet restore "CPSCForum.csproj"

# Copy everything else and publish
COPY . .
RUN dotnet publish "CPSCForum.csproj" -c Release -o /app/publish /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app


RUN apt-get update && apt-get install -y ca-certificates && update-ca-certificates

COPY --from=build /app/publish .

ENV PORT=8080
ENV ASPNETCORE_URLS=http://+:${PORT}

EXPOSE 8080

ENTRYPOINT ["dotnet", "CPSCForum.dll"]
