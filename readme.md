# Rapid CRUD API
A simple template for rapidly making a CRUD API in .NET.

## Required Environmental Vriables
The path to the required .env file is passed in as an argument to the program via the --env option. The .env file should contain the following required variables:
- `ROOT_API_KEY` - The root API key for the API. This should be a GUID and is used to authenticate the API.
- `LOG_LEVEL` - [trace, debug, info, warn, error, critical] The log level for the API
- `FILE_ROOT` - The root directory for the API to store files in. This should be a path to a directory that exists on the machine running the API.
- `PORT` - The optional port for the API to listen on. This should be a valid port number. If not specified, the API will listen on port 80.
- `DISABLE_AUTH` - Optional. If this is set to true, the API will not require authentication. This is useful for testing the API locally.