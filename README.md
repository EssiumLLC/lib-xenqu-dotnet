This example can be run inside a Docker container:

docker run \
    -v ./share/code/dotnet/:/code/ \
    -it microsoft/dotnet /bin/bash

Then type in the container prompt:

> cd code/TestApp
> dotnet run


