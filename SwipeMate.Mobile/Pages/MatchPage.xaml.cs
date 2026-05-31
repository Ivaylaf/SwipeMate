using System.Text.Json;
using SwipeMate.Mobile.Services;

namespace SwipeMate.Mobile.Pages;

public partial class MatchPage : ContentPage
{
    private readonly AppState _appState;

    public MatchPage(AppState appState)
    {
        InitializeComponent();
        _appState = appState;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        var match = _appState.CurrentMatch;
        if (match is null)
        {
            return;
        }

        MatchTitleLabel.Text = match.Title;
        MatchMessageLabel.Text = string.IsNullOrWhiteSpace(_appState.CurrentMatchMessage)
            ? "Всички се съгласихте с този избор."
            : _appState.CurrentMatchMessage;
        MatchDescriptionLabel.Text = GetText(match.Meta, "description");
        MatchMetaLabel.Text = BuildMetaText(match.Meta, match.Category);
        MatchImage.Source = string.IsNullOrWhiteSpace(match.ImageUrl)
            ? "dotnet_bot.png"
            : ImageSource.FromUri(new Uri(match.ImageUrl));

        PopulateTags(match.Meta, match.Category);
        MatchedUsersCollectionView.ItemsSource = _appState.CurrentMatchedUsers
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(name => new MatchedUserChip(name))
            .ToList();
    }

    private async void OnHomeClicked(object sender, EventArgs e)
    {
        _appState.CurrentMatch = null;
        _appState.CurrentMatchMessage = null;
        _appState.CurrentMatchedUsers = [];
        await Shell.Current.GoToAsync("//Home");
    }

    private void PopulateTags(JsonElement meta, string category)
    {
        MatchTagsLayout.Children.Clear();

        foreach (var tag in GetTags(meta, category))
        {
            MatchTagsLayout.Children.Add(new Border
            {
                StrokeThickness = 0,
                BackgroundColor = Color.FromArgb("#EEF2FF"),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(10) },
                Padding = new Thickness(10, 5),
                Margin = new Thickness(0, 0, 8, 8),
                Content = new Label
                {
                    Text = tag,
                    FontSize = 12,
                    TextColor = Color.FromArgb("#312E81")
                }
            });
        }
    }

    private static List<string> GetTags(JsonElement meta, string category)
    {
        return category switch
        {
            "Restaurant" => GetStrings(meta, "cuisines").Take(3).ToList(),
            "Recipe" => GetStrings(meta, "ingredients").Take(3).ToList(),
            "BoardGame" => new List<string> { GetTextValue(meta, "gameType") }.Where(x => !string.IsNullOrWhiteSpace(x)).ToList(),
            _ => GetStrings(meta, "genres").Take(3).ToList()
        };
    }

    private static string BuildMetaText(JsonElement meta, string category)
    {
        return category switch
        {
            "Restaurant" => $"★ {GetNumber(meta, "rating")}  •  {GetTextValue(meta, "priceRange")}  •  {GetTextValue(meta, "district")}, {GetTextValue(meta, "city")}",
            "Recipe" => $"★ {GetNumber(meta, "rating")}  •  {GetTextValue(meta, "cuisine")}  •  {GetNumber(meta, "prepTime")} мин",
            "BoardGame" => $"★ {GetNumber(meta, "rating")}  •  {GetNumber(meta, "playersMin")}-{GetNumber(meta, "playersMax")} играчи",
            _ => $"★ {GetNumber(meta, "rating")}  •  {GetNumber(meta, "year")}  •  {JoinArray(meta, "genres")}"
        };
    }

    private static string GetText(JsonElement meta, string property)
        => meta.ValueKind == JsonValueKind.Object && meta.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : string.Empty;

    private static string GetTextValue(JsonElement meta, string property)
        => meta.ValueKind == JsonValueKind.Object && meta.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : string.Empty;

    private static string GetNumber(JsonElement meta, string property)
    {
        if (meta.ValueKind != JsonValueKind.Object || !meta.TryGetProperty(property, out var value))
        {
            return string.Empty;
        }

        return value.ValueKind switch
        {
            JsonValueKind.Number when value.TryGetInt32(out var integer) => integer.ToString(),
            JsonValueKind.Number when value.TryGetDouble(out var floating) => floating.ToString("0.0"),
            _ => string.Empty
        };
    }

    private static IEnumerable<string> GetStrings(JsonElement meta, string property)
    {
        if (meta.ValueKind != JsonValueKind.Object || !meta.TryGetProperty(property, out var value) || value.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return value.EnumerateArray().Select(x => x.GetString() ?? string.Empty).Where(x => !string.IsNullOrWhiteSpace(x));
    }

    private static string JoinArray(JsonElement meta, string property)
        => string.Join(", ", GetStrings(meta, property));

    private sealed record MatchedUserChip(string Name)
    {
        public string Initials => string.Concat(Name.Split([' ', '_', '-'], StringSplitOptions.RemoveEmptyEntries).Select(x => x[0]).Take(2)).ToUpperInvariant();
    }
}
