using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BotApiTemplate.Storage
{
    public class Card
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public string SourceLang { get; set; } = string.Empty;
        public string TargetLang { get; set; } = string.Empty;
        public string EnglishVersion { get; set; } = string.Empty;
        public string OriginalVersion { get; set; } = string.Empty;
        public string Translation { get; set; } = string.Empty;
        public string AddInfo { get; set; } = string.Empty;
        public long CreatedUserId { get; set; }
        public DateTime Created { get; set; }
        public int Level { get; set; }
        public string PartOfSpeech { get; set; } = string.Empty;
    }
}
