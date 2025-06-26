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
                "Mystery" => "bg-indigo text-white",
                "Biography" => "bg-light text-dark",
                "Autobiography" => "bg-light text-dark",
                "Science Fiction" => "bg-secondary",
                "Historical Fiction" => "bg-teal text-white",
                "Horror" => "bg-black text-white",
                "Thriller" => "bg-danger text-white",
                "Non-Fiction" => "bg-secondary text-white",
                "Young Adult" => "bg-primary text-white",
                "Adventure" => "bg-orange text-white",
                "Poetry" => "bg-purple text-white",
                _ => "bg-secondary"
            };
        }
    }
}