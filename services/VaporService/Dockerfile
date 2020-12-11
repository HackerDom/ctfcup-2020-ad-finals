FROM mcr.microsoft.com/dotnet/aspnet:5.0

WORKDIR /app

COPY ./publish/ /app/
COPY ./start.sh /app/

RUN adduser --system --home /app --shell /usr/sbin/nologin --no-create-home --disabled-password -u 10000 vaporservice

ENTRYPOINT ["./start.sh"]
