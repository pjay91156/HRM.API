public class DesignationResponse
{
    public Guid Id { get; set; }

    public string DesignationName { get; set; } = string.Empty;
    public string DepartmentName{get;set;}=string.Empty;
    public Guid DepartmentId{get;set;}
}