certs:
	openssl ecparam -name prime256v1 -genkey -noout -out ./caddy/tls.key
	openssl req -new -key ./caddy/tls.key -out ./caddy/tls.csr -subj "/C=RU/ST=Moscow/L=Moscow/O=MyOrg/OU=MyUnit/CN=localhost" -addext "subjectAltName = DNS:localhost,IP:127.0.0.1" -addext "extendedKeyUsage = serverAuth"
	openssl x509 -req -in ./caddy/tls.csr -signkey ./caddy/tls.key -out ./caddy/tls.crt -days 365
	openssl pkcs12 -export -inkey ./caddy/tls.key -in ./caddy/tls.crt -out ./caddy/certificate.pfx -passout pass:localhost
dotnet-certs:
	dotnet dev-certs https --export-path ./server/cert.pfx --password password