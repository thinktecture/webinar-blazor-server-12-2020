docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=<Strong!Passw0rd>" `
  -p 1433:1433 --name blazor-server-webinar-sql `
  -d mcr.microsoft.com/mssql/server:2019-latest

docker run -e "ACCEPT_EULA=Y" `
  -p 5341:5341 --name blazor-server-webinar-seq `
  -d datalust/seq:latest
