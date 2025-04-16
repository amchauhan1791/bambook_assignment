# Stage 1 - build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and restore
COPY . .
RUN dotnet restore src/NopCommerce.sln

# Publish the app
RUN dotnet publish src/Presentation/Nop.Web/Nop.Web.csproj -c Release -o /app/published

WORKDIR /app/published

RUN mkdir logs bin
RUN set -e; \
  for dir in App_Data App_Data/DataProtectionKeys bin logs Plugins wwwroot/bundles wwwroot/db_backups wwwroot/files/export; do \
    if [ -d "$dir" ]; then \
      echo "Setting permissions for $dir"; \
      chmod 775 "$dir"; \
    else \
      echo "Directory $dir does not exist. Skipping..."; \
    fi; \
  done

# Stage 2 - runtime (Alpine)
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
			  
# Add globalization support
RUN apk add --no-cache icu-libs icu-data-full
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Install image processing libraries
RUN apk add --no-cache \
    tiff \
    libgdiplus \
    libc-dev \
    tzdata \
    --repository http://dl-3.alpinelinux.org/alpine/edge/main/ \
    --repository http://dl-3.alpinelinux.org/alpine/edge/community/

# Copy and prepare entrypoint
COPY ./entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

WORKDIR /app

# Copy published output from build stage
COPY --from=build /app/published .

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["/entrypoint.sh"]
