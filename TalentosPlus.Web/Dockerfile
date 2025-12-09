FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["TalentosPlus.Web/TalentosPlus.Web.csproj", "TalentosPlus.Web/"]
COPY ["TalentosPlus.Application/TalentosPlus.Application.csproj", "TalentosPlus.Application/"]
COPY ["TalentosPlus.Domain/TalentosPlus.Domain.csproj", "TalentosPlus.Domain/"]
COPY ["TalentosPlus.Infrastructure/TalentosPlus.Infrastructure.csproj", "TalentosPlus.Infrastructure/"]
RUN dotnet restore "TalentosPlus.Web/TalentosPlus.Web.csproj"
COPY . .
WORKDIR "/src/TalentosPlus.Web"
RUN dotnet build "TalentosPlus.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TalentosPlus.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TalentosPlus.Web.dll"]
