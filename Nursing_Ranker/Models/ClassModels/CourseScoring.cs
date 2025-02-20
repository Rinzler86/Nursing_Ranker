public static class CourseScoring
{
    public static readonly Dictionary<string, (int PointsForA, int PointsForB, int MinGradePoints)> CourseRules = new()
    {
        { "BIOL 2010", (500, 250, 0) },
        { "BIOL 2020", (500, 250, 0) },
        { "BIOL 2011", (0, 0, 0) },
        { "BIOL 2021", (0, 0, 0) },
        { "BIOL 2230", (0, 0, 0) },
        { "BIOL 2231", (0, 0, 0) },
        { "ENGL 1010", (0, 0, 0) },
        { "MATH 1530", (0, 0, 0) },
        { "PSYC 1030", (0, 0, 0) },
        { "COMM 2025", (0, 0, 0) }
    };
}
