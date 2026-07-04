public class DashboardResponse
{
    public int TotalEmployees { get; set; }

    public int TotalDepartments { get; set; }

    public int TotalDesignations { get; set; }

    // YoY Headcount Growth
    public decimal GrowthRate { get; set; }

}
public class HeadcountChartResponse
{
    public string Name { get; set; } = string.Empty;
    public int Headcount { get; set; }
}
public class DepartmentEmployeeCountResponse
{
    public string DepartmentName { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
}

