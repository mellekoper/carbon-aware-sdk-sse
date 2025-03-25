"""
This script is a proof of concept for our Sustainable Software Engineering project.
It integrates with ENTSO-E and a .NET CLI (from the Carbon Aware SDK) to retrieve congestion data,
which includes energy load and generation metrics. The data is then analyzed to calculate
the average 'Difference' over a user-specified number of timestamps. This average is used to
decide whether it is an opportune moment to run energy-intensive operations (e.g., during periods
of excess renewable energy).

Usage:
  The script accepts command-line arguments to define:
    - The folder in which to run the dotnet command.
    - The location for which congestion data should be fetched.
    - The threshold for the average 'Difference'.
    - The number of timestamps to average over.

The resulting decision (if the average 'Difference' exceeds the threshold) helps inform whether
to trigger energy-intensive tasks, thereby aligning compute operations with periods of energy surplus.
"""

import subprocess
import json
import sys
import os
import argparse

def run_dotnet_command(cwd, location):
    """
    Executes the .NET CLI command to fetch congestion data for a specified location.

    Parameters:
      cwd (str): The directory in which the dotnet command will be executed.
      location (str): The geographical location used as a parameter for the CLI command.

    Functionality:
      - Constructs the dotnet command:
          dotnet run --property WarningLevel=0 caw congestion -l <location>
      - Executes the command in the provided working directory.
      - Captures the combined standard output and standard error.
      - Parses the output to extract the JSON section (starting from the first line that begins with '[').
      - Converts the JSON string into a Python object. If the data is a list of entries with a "Time" key,
        it is transformed into a dictionary keyed by the timestamp.

    Returns:
      dict: The parsed congestion data, optionally keyed by "Time".

    Raises:
      ValueError: If no JSON output is found or if JSON parsing fails.
    """
    # Define the dotnet command with the provided location parameter.
    command = [
        "dotnet", "run",
        "--property", "WarningLevel=0",
        "caw", "congestion",
        "-l", location
    ]

    # Execute the command in the specified folder, capturing both stdout and stderr.
    result = subprocess.run(command, capture_output=True, text=True, cwd=cwd)
    combined_output = result.stdout + "\n" + result.stderr

    # Process the output: find the first line that starts with '[' indicating the beginning of JSON data.
    lines = combined_output.splitlines()
    json_lines = []
    found_json = False

    for line in lines:
        stripped_line = line.strip()
        # Identify the start of the JSON output.
        if not found_json and stripped_line.startswith('['):
            found_json = True
        if found_json:
            json_lines.append(line)

    if not json_lines:
        raise ValueError("No JSON output found in the command output.")

    # Combine the collected lines into a single JSON string.
    json_str = "\n".join(json_lines)

    # Attempt to parse the JSON string.
    try:
        data = json.loads(json_str)
    except json.JSONDecodeError as e:
        raise ValueError(f"Failed to decode JSON: {e}")

    # If the data is a list and contains a "Time" field, convert it into a dictionary keyed by "Time".
    if isinstance(data, list) and data and "Time" in data[0]:
        data = {entry["Time"]: {k: v for k, v in entry.items() if k != "Time"} for entry in data}

    return data

def store_json(data, output_path):
    """
    Saves the given JSON data to a file.

    Parameters:
      data (dict): The JSON data to be stored.
      output_path (str): The file path where the JSON data should be saved.

    The data is written in an indented format for readability.
    """
    with open(output_path, "w") as f:
        json.dump(data, f, indent=2)
    print(f"JSON output stored in: {output_path}")

def check_average_difference(data, threshold, num_timestamps):
    """
    Computes the average of the 'Difference' values from the last 'num_timestamps' data points,
    and checks whether this average exceeds a specified threshold.

    Parameters:
      data (dict): A dictionary with ISO 8601 timestamp keys and corresponding congestion metrics
                   (each containing a 'Difference' value).
      threshold (float): The threshold value to compare the average 'Difference' against.
      num_timestamps (int): The number of the most recent timestamps to include in the average.

    Returns:
      tuple:
        - float: The computed average of the 'Difference' values.
        - bool: True if the average is greater than the threshold, False otherwise.

    Raises:
      ValueError: If the number of available data points is less than 'num_timestamps'.
    """
    if len(data) < num_timestamps:
        raise ValueError(f"Not enough data to calculate the average over the final {num_timestamps} timestamps.")

    # Sort the ISO 8601 timestamps to ensure chronological order.
    sorted_timestamps = sorted(data.keys())
    last_entries = sorted_timestamps[-num_timestamps:]
    
    # Extract the 'Difference' values for the selected timestamps.
    differences = [data[t]['Difference'] for t in last_entries]
    avg_difference = sum(differences) / len(differences)
    
    return avg_difference, avg_difference > threshold

if __name__ == "__main__":
    # Set up command-line argument parsing.
    parser = argparse.ArgumentParser(
        description="Run dotnet command and analyze congestion data. "
                    "This proof-of-concept script is part of a sustainable software engineering project. "
                    "It retrieves congestion data from the Carbon Aware SDK and ENTSO-E, calculates the average 'Difference' "
                    "between network load and generation, "
                    "and helps decide whether to run energy-intensive operations based on energy surplus."
    )
    parser.add_argument("--folder", "-f", type=str, default="src/CarbonAware.CLI/src",
                        help="Folder path where the dotnet command should be executed")
    parser.add_argument("--threshold", "-t", type=float, default=100,
                        help="Threshold for average 'Difference'")
    parser.add_argument("--location", "-l", type=str, default="Netherlands",
                        help="Location for the dotnet command (replaces hard-coded location)")
    parser.add_argument("--num_timestamps", "-n", type=int, default=4,
                        help="Number of final timestamps to average over")
    args = parser.parse_args()

    # Verify that the specified folder exists.
    if not os.path.isdir(args.folder):
        print(f"Error: The folder '{args.folder}' does not exist.")
        sys.exit(1)

    try:
        # Retrieve congestion data by executing the dotnet command.
        data = run_dotnet_command(args.folder, args.location)
        
        # Calculate the average 'Difference' over the specified number of timestamps.
        avg_diff, is_above = check_average_difference(data, args.threshold, args.num_timestamps)
        print(f"\nAverage 'Difference' over the final {args.num_timestamps} timestamps: {avg_diff:.2f}")
        
        # Inform the user whether the average exceeds the threshold.
        if is_above:
            print(f"The average 'Difference' is greater than the threshold of {args.threshold}.")
            print("This indicates a surplus, potentially a good time to run energy-intensive programs.")
        else:
            print(f"The average 'Difference' is not greater than the threshold of {args.threshold}.")
            print("It may not be an optimal time to run energy-intensive programs.")
        
    except Exception as e:
        # Handle and report any errors that occur during execution.
        print(f"An error occurred: {e}")