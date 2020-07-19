Before running this example, open `TestApp/Program.cs` and change the authorization configuration to match the value provided and the location of your private key file:

````
config.Initialize( 
    "<WILL_BE_PROVIDED>",     // Client Id
    "<WILL_BE_PROVIDED>",     // Client Secret
    "<WILL_BE_PROVIDED>",     // Subscriber
    "<PATH_TO_PRIVATE_KEY>"   // Key File
);
````

Then spin up a Docker container:

````
docker run \
    -v <path/to/this/code>:/code/ \
    -it microsoft/dotnet /bin/bash
````

Then type in the container prompt:

    > cd code/TestApp
    > dotnet run

If everything is setup properly, you will see several lines of output from the `user/info` service:

````
Demo Account
demouser
Xenqu Demo
````
