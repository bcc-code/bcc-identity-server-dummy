docker container rm bcc-identity --force
docker image rm bcc-identity-image
docker build -t bcc-identity-image -f dockerfile .