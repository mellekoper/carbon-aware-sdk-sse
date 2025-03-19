using CarbonAware.CLI.Common;
using CarbonAware.CLI.Model;
using GSF.CarbonAware.Handlers;
using CarbonAware.Model;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Text.Json;

namespace CarbonAware.CLI.Commands.Congestion;

class CongestionCommand : Command
{
    private readonly Option<string[]> _requiredLocation = CommonOptions.RequiredLocationOption;
    private readonly Option<DateTimeOffset?> _startTime = new Option<DateTimeOffset?>(
            new string[] { "--start-time", "-s" },
            LocalizableStrings.StartTimeDescription)
            {
                Arity = ArgumentArity.ZeroOrOne,
            };
    private readonly Option<DateTimeOffset?> _endTime = new Option<DateTimeOffset?>(
             new string[] { "--end-time", "-e" },
            LocalizableStrings.EndTimeDescription)
            {
                Arity = ArgumentArity.ZeroOrOne,
            };
    private readonly Option<bool> _best = new Option<bool>(
            new string[] { "--best", "-b" },
            LocalizableStrings.BestDescription)
            {
                Arity = ArgumentArity.ZeroOrOne,
            };
    private readonly Option<bool> _average = new Option<bool>(
            new string[] { "--average", "-a" },
           LocalizableStrings.AverageDescription)
            {
                Arity = ArgumentArity.ZeroOrOne,
            };
    private readonly Dictionary<string, string> _displayNameMap = new Dictionary<string, string>()
            {
                { "MultipleLocations", "--location"},
                { "SingleLocation", "--location"},
                { "Start", "--start-time" },
                { "End", "--end-time" },
            };
    public CongestionCommand() : base("congestion", LocalizableStrings.CongestionCommandDescription)
    {
        AddOption(_requiredLocation);
        AddOption(_startTime);
        AddOption(_endTime);
        AddOption(_best);
        AddOption(_average);

        AddValidator(ValidateMutuallyExclusiveOptions);

        this.SetHandler(this.Run);
    }

    private void ValidateMutuallyExclusiveOptions(CommandResult commandResult)
    {
        // Validate mutually exclusive options 
        var average = commandResult.GetValueForOption<bool>(_average);
        var best = commandResult.GetValueForOption<bool>(_best);
        if (average && best)
        {
            commandResult.ErrorMessage = "Options --average and --best cannot be used together";
        }
    }
    internal async Task Run(InvocationContext context)
    {
        // Get handler via DI.
        var serviceProvider = context.BindingContext.GetService(typeof(IServiceProvider)) as IServiceProvider ?? throw new NullReferenceException("ServiceProvider not found");
        var congestionHandler = serviceProvider.GetService(typeof(ICongestionHandler)) as ICongestionHandler ?? throw new NullReferenceException("ICongestionHandler not found");

        // Get the arguments and options to build the parameters.
        var locations = context.ParseResult.GetValueForOption<string[]>(_requiredLocation);
        var startTime = context.ParseResult.GetValueForOption<DateTimeOffset?>(_startTime);
        var endTime = context.ParseResult.GetValueForOption<DateTimeOffset?>(_endTime);
        var best = context.ParseResult.GetValueForOption<bool>(_best);
        var average = context.ParseResult.GetValueForOption<bool>(_average);

        var parameters = new CarbonAwareParametersBaseDTO(_displayNameMap)
        {
            Start = startTime,
            End = endTime
        };
        // Call the handler.
        List<CongestionDataDTO> congestion = new();
        
        if (best)
        {
            parameters.MultipleLocations = locations;

            var results = await congestionHandler.GetCongestionDataAsync(parameters.MultipleLocations!, parameters.Start, parameters.End);

            congestion = results.Select(congestion => (CongestionDataDTO)congestion).ToList();
        }
        else if (average) 
        {
            foreach (var location in locations!)
            {
                parameters.SingleLocation = location;

                var averageCongestion = await congestionHandler.GetAverageCongestionAsync(
                    parameters.SingleLocation!,
                    (DateTimeOffset)parameters.Start!,
                    (DateTimeOffset)parameters.End!);
                
                // If startTime or endTime were not provided, the handler would have thrown an error as startTime and endTime are required and validated in it. So, at this point it is safe to assume that the start/end values are not null. 
                var congestionData = new CongestionDataDTO()
                {
                    Location = location,
                    Time = startTime,
                    Duration = endTime - startTime,
                    Difference = averageCongestion
                };
                congestion.Add(congestionData);
            }
        }
        else
        {
            parameters.MultipleLocations = locations;
            var results = await congestionHandler.GetCongestionDataAsync(parameters.MultipleLocations!, parameters.Start, parameters.End);
            if (results != null)
            {
                congestion = results.Select(congestion => (CongestionDataDTO)congestion).ToList();
            }
        }
       
        var serializedOuput = JsonSerializer.Serialize(congestion);
        context.Console.WriteLine(serializedOuput);
        context.ExitCode = 0;
    }
}
