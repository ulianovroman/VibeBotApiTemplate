namespace BotApiTemplate.Dto
{
    public class CardDto
    {
        public string t { get; set; }          // translation
        public string pos { get; set; }        // part of speech
        public string m { get; set; }          // morphology
        public List<string> forms { get; set; } = new();
        public List<string> ex { get; set; } = new(); // 1-2 short examples
    }
}
