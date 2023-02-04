# Rapid CRUD API
A simple template for rapidly making a CRUD API in .NET.

## Required Environmental Vriables
The path to the required .env file is passed in as an argument to the program via the --env option. The .env file should contain the following required variables:
- `ROOT_API_KEY` - The root API key for the API. This should be a GUID and is used to authenticate the API.
- `LOG_LEVEL` - [trace, debug, info, warn, error, critical] The log level for the API