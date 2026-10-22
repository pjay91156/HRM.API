namespace HRM.API.Enums;

public enum AuditModule
{
    Employee = 1,
    Leave = 2,
    Attendance = 3,
    Performance = 4,
    MeetingRoom = 5,
    Department = 6,
    Designation = 7,
    UserManagement = 8,
    RolePermission = 9,
    System = 10
}

public enum AuditAction
{
    Created = 1,
    Updated = 2,
    Deleted = 3,
    Approved = 4,
    Rejected = 5,
    Cancelled = 6,
    Activated = 7,
    Deactivated = 8,
    Submitted = 9,
    Assigned = 10
}
