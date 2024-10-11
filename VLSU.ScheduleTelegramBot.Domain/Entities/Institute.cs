using System.Text;

namespace VLSU.ScheduleTelegramBot.Domain.Contracts;

public class Institute
{
    public long Id { get; set; } = default;
    public string Name { get; set; } = string.Empty;


    public string GetShortName()
    {
        var splitName = Name.Split(' ');
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
