using System.Text;

namespace VLSU.ScheduleTelegramBot.Domain.Responces;

public class InstituteInfoResponce
{
    public long Value { get; set; } = default;
    public string Text { get; set; } = string.Empty;


    public string GetShortName()
    {
        var splitName = Text.Split(' ');
        var shortName = new StringBuilder();

        foreach (var word in splitName)
        {
            if (word.Length > 1)
                shortName.Append(word[0].ToString().ToUpper());
            else
                shortName.Append(word);
        }

        return shortName.ToString();
    }
}
