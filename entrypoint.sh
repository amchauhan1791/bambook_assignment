#!/bin/sh
set -e

# Set default DB host if not overridden
DB_HOST="${DB_HOST:-nopcommerce_database}"

echo "Waiting for SQL Server to be ready at $DB_HOST:1433..."

# Wait until SQL Server container is reachable
until nc -z $DB_HOST 1433; do
  echo "Waiting for SQL Server..."
  sleep 1
done

echo "SQL Server is ready. Launching NopCommerce..."
exec dotnet Nop.Web.dll