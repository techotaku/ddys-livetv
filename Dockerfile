FROM alpine

WORKDIR /app

COPY ./publish/ .

RUN apk add --no-cache icu-libs krb5-libs libc6-compat libgcc libintl libssl1.1 libstdc++ zlib

CMD ["./DDRK.LiveTV", "--urls", "http://*:80"]