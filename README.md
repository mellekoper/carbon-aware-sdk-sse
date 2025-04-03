# Carbon Aware SDK SSE

## Overview
This repository extends the existing [Carbon Aware SDK](https://github.com/mellekoper/carbon-aware-sdk-sse) with additional features aimed at improving sustainability in software engineering. The modifications focus on adding network congestion to the SDK. The original README with details and instructions about the Carbon Aware project can be found [here.](README_Carbon.md)

## Features Added in This Extension
This project enhances the original Carbon Aware SDK with the following:

- A new Congestion Data Type to track differences between predicted generation and actual load.

- Extensions to the Carbon Aware SDK, enabling it to process congestion data.

- A CLI command and API endpoint for retrieving congestion data and making energy aware decisions.

- Integration with the ENTSO-E transparency platform, which provides near-live energy grid data.


## Running Instructions

### Prerequisites
Before running the Carbon Aware SDK SSE, ensure you have the following installed:
- [.NET 6.0 or later](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
- [Docker](https://www.docker.com/get-started) (optional, for containerized deployment)
- Git

We recommend the remote containers extension fro VSCode which can be found [here.](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers)

### running with CLI
1. Clone this repository:
   ```sh
   git clone https://github.com/mellekoper/carbon-aware-sdk-sse.git
   cd carbon-aware-sdk-sse
   ```

2. Navigate to the CLI folder
    ```sh
    cd src/CarbonAware.CLI/src
    ```

3. Update the appsettings.json file with the Entsoe datasource
    ```json
    "DataSources": {
        "CongestionDataSource": "Entsoe",
        "EmissionsDataSource": "<Your Emissions datasource>",
        "ForecastDataSource": "<Your Forecast datasource>",
        "Configurations": {
            "Entsoe": {
            "Type": "Entsoe",
            "BaseURL": "https://web-api.tp.entsoe.eu/api",
            "ApiKey": "<Your API Key here>"
            }
        }
    }
    ```

4. Run the application with congestion awareness:
   ```sh
   dotnet run caw congestion -l [location]
   ```
   Additional command-line options include:
   - `-s, --start-time <start-time>`: Specify a start time for congestion data.
   - `-e, --end-time <end-time>`: Specify an end time for congestion data.
   - `-b, --best`: Filter results to show the best (lowest congestion) data point.
   - `-a, --average`: Compute the weighted average of congestion within the time range.
   - `-v, --verbose`: Enable detailed debugging output.



## API Usage
1. Clone this repository:
   ```sh
   git clone https://github.com/mellekoper/carbon-aware-sdk-sse.git
   cd carbon-aware-sdk-sse
   ```
2. Open VSCode: `code .`
3. Open VSCode Command Palette: (Linux/Windows: `ctrl + shift + P`, MacOS:
   `cmd + shift + P`), and run the command:
    ```sql
   Dev Containers: Open Folder in Container...
   ```
4. If you have a WattTime account registered (or other data source) - you will
   need to configure the application to use them. By default the SDK will use a
   pre-generated JSON file with random data. To configure the application, you
   will need to set up specific environment variables or modify
   `appsettings.json` inside of `src/CarbonAware.WebApi/src` directory. An example of what the `appsettings.json` file might look like is provided below:
```json
{
	"DataSources": {
		"CongestionDataSource": "Entsoe",
		"EmissionsDataSource": "ElectricityMaps",
		"ForecastDataSource": "WattTime",
		"Configurations": {
			"WattTime": {
				"Type": "WattTime",
				"Username": "<YOUR_WATTTIME_USERNAME>",
				"Password": "<YOUR_WATTTIME_PASSWORD>",
				"BaseURL": "https://api2.watttime.org/v2/"
			},
			"ElectricityMaps": {
				"Type": "ElectricityMaps",
				"BaseURL": "https://api.electricitymap.org/v3/",
				"APITokenHeader": "auth-token",
				"APIToken": "<YOUR_ELECTRICITYMAPS_TOKEN>"
			},
			"Entsoe": {
				"Type": "Entsoe",
				"BaseURL": "https://web-api.tp.entsoe.eu/api",
				"ApiKey": "<YOUR_ENTSOE_APIKEY>"
			}
		}
	},
	"Logging": {
		"LogLevel": {
			"Default": "Information",
			"Microsoft.AspNetCore": "Warning"
		}
	},
	"AllowedHosts": "*"
}
```

6. Change directory to the WebApi src: 
```sh
cd src/CarbonAware.WebApi/src
```
7. And run the application using: 
```sh
dotnet run
```
8. By default, it will be hosted on `localhost:5073`

### Calling the Web API via command line

Prerequisites:

- `curl` or other tool that allows making HTTP requests (e.g. `wget`)
- Recommended: `jq` for parsing JSON output: [https://stedolan.github.io/jq/](https://stedolan.github.io/jq/)

With the API running on `localhost:5073`, we can make HTTP requests to its
endpoints.

To get a list of all locations supported, you can use the Locations API endpoint
`/locations` referenced in
`src/CarbonAware.WebApi/src/Controllers/LocationsController.cs`.

Expected Output:

```JSON
{
  "eastus": {
    "Latitude": 37.3719,
    "Longitude": -79.8164,
    "Name": "eastus"
  },
  ...
  "switzerlandnorth":{
    "Latitude": 47.451542,
    "Longitude": 8.564572,
    "Name": "switzerlandnorth"
  }
}
```

#### Calling the `/congestion/bylocation` endpoint

In console, we can run the below command, to request data for a single location
(currently Azure region names supported) in a particular timeframe:

```bash
curl "http://localhost:5073/congestion/bylocation?location=Netherlands" | jq
```

You can omit the `| jq` to get the JSON data raw and unparsed. This is a request
for data in the `Netherlands` region.

The output should look something like:

```JSON
[
	{
		"location":"Netherlands",
		"time":"2025-03-30T23:00:00+00:00",
		"load":8440,
		"generation":12218,
		"difference":3778,
		"duration":"00:00:00"
	},
	{
		"location":"Netherlands",
		"time":"2025-03-30T23:15:00+00:00",
		"load":8559,
		"generation":12097,
		"difference":3538,
		"duration":"00:00:00"
	}
]
```




