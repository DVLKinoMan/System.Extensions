namespace System.Exts
{
    public static partial class Extensions
    {
        public static int GetAge(this DateTime birthDate, DateTime? deathDate = null)
        {
            var end = deathDate ?? DateTime.Now;
            int age = end.Year - birthDate.Year;
            if (birthDate > end.AddYears(-age))
                age--;

            return age;
        }

        public static int DateDiff(string interval, DateTime starting, DateTime ending) =>
           throw new NotImplementedException();
    }
}
