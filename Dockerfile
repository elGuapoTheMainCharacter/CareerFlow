# ---- build stage: compile the API ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only the project file first, so the restore step is cached between builds.
COPY server/CareerFlow.Web/CareerFlow.Web.csproj server/CareerFlow.Web/
RUN dotnet restore server/CareerFlow.Web/CareerFlow.Web.csproj

COPY server/CareerFlow.Web/ server/CareerFlow.Web/
RUN dotnet publish server/CareerFlow.Web/CareerFlow.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---- run stage: a small image with only the runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 10000

# Render tells the app which port to listen on through the PORT variable.
# "exec" makes dotnet the main process so it receives shutdown signals.
CMD ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-10000} exec dotnet CareerFlow.Web.dll"]
