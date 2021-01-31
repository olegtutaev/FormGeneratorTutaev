using System.Diagnostics;



namespace JSON_Tutaev
{
    
    public class Program
    {
        public static void Main(string[] args)
        {
            var form = new Form();
            form.ReadFromJson(@"../../json.json");
            form.WriteToHtml(@"html.html");
        }
    }
}
