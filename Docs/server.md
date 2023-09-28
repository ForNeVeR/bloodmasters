Server Installation
===================
Docker is recommended to install the server. For example, run the following shell command:

```bash
NAME=bloodmasters
BLOODMASTERS_VERSION=latest
CONFIG=/opt/bloodmasters/server/bmserver.config # optional
PORT=4999
docker pull revenrof/bloodmasters:$BLOODMASTERS_VERSION
docker rm -f $NAME
docker run --name $NAME \
    -v $CONFIG:/app/bmserver.config:ro \
    -p 127.0.0.1:$PORT:6969 \
    --restart unless-stopped \
    -d \
    revenrof/bloodmasters:$BLOODMASTERS_VERSION
```

where

- `$NAME` is the container name
- `$BLOODMASTERS_VERSION` is the image version you want to deploy, or `latest` for the latest available one
- `$CONFIG` is the **absolute** path to the configuration file
- `$PORT` is the port on the host system which will be used to expose the server

Note that this command exposes the container port `6969` (which is meant to be passed via config file). Feel free to use
another port if you want, but don't forget to update the command.
