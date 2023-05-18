[AllowUnauthorized]
public class AllProjects : IQuery<List<ProjectDTO>>
{
    public bool SortByNameDescending { get; set; }
}

public class ProjectDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
}
