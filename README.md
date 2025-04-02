# Carbon Aware SDK SSE

## Overview
This repository extends the existing [Carbon Aware SDK](https://github.com/mellekoper/carbon-aware-sdk-sse) with additional features aimed at improving sustainability in software engineering. The modifications focus on adding network congestion to the SDK. The original README with details and instructions about the Carbon Aware project can be found [here.](README_Carbon.md)


## Running Instructions

### Prerequisites
Before running the Carbon Aware SDK SSE, ensure you have the following installed:
- [.NET 6.0 or later](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
- [Docker](https://www.docker.com/get-started) (optional, for containerized deployment)
- Git

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
        "EmissionsDataSource": [Your Emissions datasource],
        "ForecastDataSource": [Your Forecast datasource],
        "Configurations": {
            "Entsoe": {
            "Type": "Entsoe",
            "BaseURL": "https://web-api.tp.entsoe.eu/api",
            "ApiKey": [Your API Key here]
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
### Running with Docker
Alternatively, you can run the project in a Docker container:
```sh
docker build -t carbon-aware-sdk-sse 
docker run -p 5000:5000 carbon-aware-sdk-sse
```
Once the service is running, you can query network congestion data using:

```sh
curl -X GET "http://localhost:5000/network-congestion?location=US" -H "Accept: application/json"
```

### Sample Response:
```json
{
  "location": "US",
  "networkCongestion": 75,
  "timestamp": "2025-04-01T12:00:00Z"
}
```

## Features Added in This Extension
This project enhances the original Carbon Aware SDK with the following:

- A new Congestion Data Type to track differences between predicted generation and actual load.

- Extensions to the Carbon Aware SDK, enabling it to process congestion data.

- A CLI command and API endpoint for retrieving congestion data and making energy aware decisions.

- Integration with the ENTSO-E transparency platform, which provides near-live energy grid data.

