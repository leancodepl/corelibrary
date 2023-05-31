#!/usr/bin/env bash
set -e

NAME=local.lncd.pl
SUBJ="
C=PL
O=LeanCode DEV
commonName=*.$NAME
organizationalUnitName=LeanCode DEV
emailAddress=admin@leancode.pl
"
PASSWD=Passwd1!

openssl genrsa -des3 -out CA.key -passout pass:"$PASSWD" 2048
openssl req -x509 -new -nodes -subj "$(echo -n "$SUBJ" | tr "\n" "/")" -key CA.key -passin pass:"$PASSWD" -sha256 -days 825 -out CA.pem

openssl genrsa -out $NAME.key 2048
openssl req -new -subj "$(echo -n "$SUBJ" | tr "\n" "/")" -key "$NAME.key" -out "$NAME.csr"
>$NAME.ext cat <<-EOF
authorityKeyIdentifier=keyid,issuer
basicConstraints=CA:FALSE
keyUsage = digitalSignature, nonRepudiation, keyEncipherment, dataEncipherment
subjectAltName = @alt_names
[alt_names]
DNS.1 = $NAME # Be sure to include the domain name here because Common Name is not so commonly honoured by itself
DNS.2 = *.$NAME # Optionally, add additional domains (I've added a subdomain here)
EOF

openssl x509 -req -in $NAME.csr -CA CA.pem -CAkey CA.key -CAcreateserial \
    -out $NAME.crt -days 825 -sha256 -extfile $NAME.ext -passin pass:"$PASSWD"

rm $NAME.ext $NAME.csr CA.srl
