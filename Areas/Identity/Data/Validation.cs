namespace Platinum.Areas.Identity.Data
{
    public static class Validation
    {
        public static bool IsItADate(string d)
        {
            DateTime date;

            return DateTime.TryParse(d, out date);
          
        }
    }
}
