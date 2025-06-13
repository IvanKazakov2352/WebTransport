certs:
	openssl ecparam -name prime256v1 -genkey -noout -out ./caddy/tls.key
	openssl req -new -key ./caddy/tls.key -out ./caddy/tls.crt -subj "/C=RU/ST=Moscow/L=Moscow/O=Organization/OU=Department/CN=localhost:8443"
	openssl x509 -req -in ./caddy/tls.crt -signkey ./caddy/tls.key -out ./caddy/cert.pem -days 14
	openssl pkcs12 -export -out ./caddy/certificate.pfx -inkey ./caddy/tls.key -in ./caddy/cert.pem -passout pass:localhost