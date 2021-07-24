# LogMechanicParser
Parsing a set of jsons produced by EliteInsights to get player and mechanic statistics across multiple logs for the purpose of tracking progression across multiple evenings.

# How to use

1. Install [Elite Insights](https://github.com/baaron4/GW2-Elite-Insights-Parser) and configure it to output .json.
2. Parse the logs you want to learn about and copy the .json files into a new folder.
3. Run this tool and pass the path of the folder with the .json files as the argument.

The result is put into a mechanicsSummary.csv that is dropped into the folder with the .json files.
