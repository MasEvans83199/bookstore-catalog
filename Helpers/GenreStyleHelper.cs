namespace BookCatalog.Helpers
{

    public static class GenreStyleHelper
    {
        public static string GetGenreBadgeClass(string genre)
        {
            return genre switch
            {
                "Classic" => "bg-primary",
                "Dystopian" => "bg-danger",
                "Fantasy" => "bg-success",
                "Romance" => "bg-pink text-dark",
                "Gothic" => "bg-dark",
                "Philosophical" => "bg-warning text-dark",
                "Fiction" => "bg-info text-dark",
                _ => "bg-secondary"
            };
        }
    }
}